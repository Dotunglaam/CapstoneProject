using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OData.Edm;

namespace DataAccess.DAO
{
    public class AttendanceDAO
    {
        private readonly kmsContext _context;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;

        public AttendanceDAO(kmsContext dbContext, IMapper mapper)
        {
            _context = dbContext;
            _mapper = mapper;
        }

        public AttendanceDAO(kmsContext dbContext, IMapper mapper, Cloudinary cloudinary)
        {
            _context = dbContext;
            _mapper = mapper;
            _cloudinary = cloudinary;
        }

        public async Task CreateAttendance(string type, string status)
        {
            var currentDate = DateTime.Now.Date;

            var classesWithoutAttendance = await GetClassesWithoutAttendance(type, currentDate);

            if (!classesWithoutAttendance.Any())
            {
                throw new Exception($"All active classes have already been {type.ToLower()}ed");
            }

            var attendanceList = new List<Attendance>();
            var attendanceDetailList = new List<AttendanceDetail>();

            foreach (var classItem in classesWithoutAttendance)
            {
                if (!await HasStudentsInClass(classItem.ClassId))
                {
                    continue;
                }

                var attendance = new Attendance
                {
                    Type = type,
                    CreatedAt = currentDate,
                    ClassId = classItem.ClassId
                };

                await _context.Attendances.AddAsync(attendance);
                await _context.SaveChangesAsync(); 

                foreach (var classHasChild in classItem.ClassHasChildren)
                {
                    var student = classHasChild.Student;

                    var attendanceDetail = new AttendanceDetail
                    {
                        AttendanceId = attendance.AttendanceId,
                        StudentId = student.StudentId,
                        CreatedAt = currentDate,
                        Status = status 
                    };

                    attendanceDetailList.Add(attendanceDetail);
                }
            }

            await _context.AttendanceDetails.AddRangeAsync(attendanceDetailList);
            await _context.SaveChangesAsync();
        }

        public async Task CreateDailyCheckin()
        {
            await CreateAttendance("Checkin", "Absent");
        }

        public async Task CreateDailyAttendance()
        {
            await CreateAttendance("Attend", "Absent");
        }

        public async Task CreateDailyCheckout()
        {
            await CreateAttendance("Checkout", "");
        }

        public async Task<List<AttendanceMapper>> GetAttendanceByClassId(int classId, string type)
        {
            try
            {
                var currentDate = DateTime.Now.Date;

                var attendances = await _context.Attendances
                    .Where(a => a.ClassId == classId && a.Type == type && a.CreatedAt.Date == currentDate)
                    .ToListAsync();

                var attendanceDtos = _mapper.Map<List<AttendanceMapper>>(attendances);

                foreach (var attendance in attendanceDtos)
                {
                    var attendanceDetails = await _context.AttendanceDetails
                        .Where(ad => ad.AttendanceId == attendance.AttendanceId)
                        .ToListAsync();

                    attendance.AttendanceDetail = _mapper.Map<List<AttendanceDetailMapper>>(attendanceDetails);
                }

                return attendanceDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving attendance records: " + ex.Message);
            }
        }

        public async Task<List<AttendanceMapper>> GetAttendanceByDate(int classId, string type, DateTime date)
        {
            try
            {
                var selectedDate = date.Date;

                var attendances = await _context.Attendances
                    .Where(a => a.ClassId == classId && a.Type == type && a.CreatedAt.Date == selectedDate)
                    .ToListAsync();

                var attendanceDtos = _mapper.Map<List<AttendanceMapper>>(attendances);

                foreach (var attendance in attendanceDtos)
                {
                    var attendanceDetails = await _context.AttendanceDetails
                        .Where(ad => ad.AttendanceId == attendance.AttendanceId)
                        .ToListAsync();

                    attendance.AttendanceDetail = _mapper.Map<List<AttendanceDetailMapper>>(attendanceDetails);
                }

                return attendanceDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving attendance records: " + ex.Message);
            }
        }

        public async Task<List<AttendanceMapper>> GetAttendanceByTypeAndDate(string type, DateTime date)
        {
            try
            {
                var selectedDate = date.Date;

                var attendances = await _context.Attendances
                    .Where(a => a.Type == type && a.CreatedAt.Date == selectedDate)
                    .ToListAsync();

                var attendanceDtos = _mapper.Map<List<AttendanceMapper>>(attendances);

                foreach (var attendance in attendanceDtos)
                {
                    var attendanceDetails = await _context.AttendanceDetails
                        .Where(ad => ad.AttendanceId == attendance.AttendanceId)
                        .ToListAsync();

                    attendance.AttendanceDetail = _mapper.Map<List<AttendanceDetailMapper>>(attendanceDetails);
                }

                return attendanceDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving attendance records: " + ex.Message);
            }
        }

        public async Task<List<AttendanceMapper>> GetAttendanceByStudentId(int studentId, string type, DateTime startOfWeek)
        {
            try
            {
                var classId = await GetClassIdByStudentId(studentId);

                // Tính toán ngày cuối tuần (ngày thứ 7)
                var endOfWeek = startOfWeek.AddDays(6); // Thêm 6 ngày để có được ngày cuối tuần

                // Lấy danh sách điểm danh cho cả tuần
                var attendances = await _context.Attendances
                    .Where(a => a.ClassId == classId && a.Type == type &&
                                a.CreatedAt.Date >= startOfWeek.Date &&
                                a.CreatedAt.Date <= endOfWeek.Date)
                    .ToListAsync();

                var attendanceDtos = _mapper.Map<List<AttendanceMapper>>(attendances);

                foreach (var attendance in attendanceDtos)
                {
                    var attendanceDetails = await _context.AttendanceDetails
                        .Where(ad => ad.AttendanceId == attendance.AttendanceId && ad.StudentId == studentId)
                        .ToListAsync();

                    attendance.AttendanceDetail = _mapper.Map<List<AttendanceDetailMapper>>(attendanceDetails);
                }

                return attendanceDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving attendance records: " + ex.Message);
            }
        }

        public async Task UpdateAttendance(List<AttendanceMapper> attendanceMappers, int classId, string type)
        {
            if (attendanceMappers == null || !attendanceMappers.Any())
            {
                throw new ArgumentException("Attendance mappers list cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Type cannot be null or empty.");
            }

            try
            {
                var attendanceDetails = await _context.AttendanceDetails
                    .Include(ad => ad.Attendance)
                    .Where(ad => ad.Attendance.ClassId == classId && ad.Attendance.Type == type)
                    .ToListAsync();

                if (attendanceDetails == null || !attendanceDetails.Any())
                {
                    throw new KeyNotFoundException("No attendance details found for the given ClassId and Type.");
                }

                bool isUpdated = false;

                foreach (var detail in attendanceDetails)
                {
                    var updatedDetail = attendanceMappers
                        .SelectMany(am => am.AttendanceDetail)
                        .FirstOrDefault(ad => ad.AttendanceDetailId == detail.AttendanceDetailId);

                    if (updatedDetail != null)
                    {
                        if (string.IsNullOrWhiteSpace(updatedDetail.Status))
                        {
                            throw new InvalidOperationException($"Invalid status for AttendanceDetailId {detail.AttendanceDetailId}.");
                        }

                        detail.Status = updatedDetail.Status;
                        detail.CreatedAt = DateTime.UtcNow;
                        isUpdated = true;
                    }
                }

                if (!isUpdated)
                {
                    throw new InvalidOperationException("No AttendanceDetails were found.");
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating attendance details: " + ex.Message, ex);
            }
        }

        public async Task UpdateAttendanceByType(List<AttendanceMapper> attendanceMappers , string type)
        {
            if (attendanceMappers == null || !attendanceMappers.Any())
            {
                throw new ArgumentException("Attendance mappers list cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Type cannot be null or empty.");
            }

            try
            {
                var attendanceDetails = await _context.AttendanceDetails
                    .Include(ad => ad.Attendance)
                    .Where(ad => ad.Attendance.Type == type)
                    .ToListAsync();

                if (attendanceDetails == null || !attendanceDetails.Any())
                {
                    throw new Exception("No attendance details found for the given Type.");
                }

                foreach (var detail in attendanceDetails)
                {
                    var updatedDetail = attendanceMappers
                        .SelectMany(am => am.AttendanceDetail)
                        .FirstOrDefault(ad => ad.AttendanceDetailId == detail.AttendanceDetailId);

                    if (updatedDetail != null)
                    {
                        detail.Status = updatedDetail.Status;
                        detail.CreatedAt = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating attendance details: " + ex.Message);
            }
        }

        public async Task UploadImageAndSaveToDatabaseAsync(int attendanceDetailID, IFormFile image)
        {
            var attendanceDetail = await _context.AttendanceDetails
                .FirstOrDefaultAsync(ad => ad.AttendanceDetailId == attendanceDetailID);

            if (attendanceDetail == null)
            {
                throw new Exception("Attendance detail not found.");
            }

            int attendanceID = attendanceDetail.AttendanceId;

            string imageUrl = await UploadImageToCloudinaryAsync(image, attendanceID);

            if (!string.IsNullOrEmpty(imageUrl))
            {
                await UpdateAttendanceDetailImageAsync(attendanceDetailID, imageUrl);
            }
            else
            {
                throw new Exception("Image upload failed");
            }
        }

        private async Task<string> UploadImageToCloudinaryAsync(IFormFile image, int attendanceID)
        {
            var attendanceInfo = await GetAttendanceInfoAsync(attendanceID);

            var firstItem = attendanceInfo.FirstOrDefault();
            if (firstItem == null)
            {
                return null; 
            }

            var createdAt = firstItem.CreatedAt;
            var classId = firstItem.ClassID;
            var type = firstItem.Type;

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(image.FileName, image.OpenReadStream()),
                // Cấu trúc thư mục sẽ có dạng: /AttendanceImages/ClassA/2024-11-12/checkin/123/image.jpg
                Folder = $"AttendanceImages/{classId}/{createdAt:yyyy-MM-dd}/{type}",
                PublicId = $"{Guid.NewGuid()}", 
                Overwrite = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult != null && !string.IsNullOrEmpty(uploadResult.SecureUri.ToString()))
            {
                return uploadResult.SecureUri.ToString();
            }

            return null;
        }

        private async Task UpdateAttendanceDetailImageAsync(int attendanceDetailID, string imageUrl)
        {
            var attendanceDetail = await _context.AttendanceDetails
                .FirstOrDefaultAsync(ad => ad.AttendanceDetailId == attendanceDetailID);

            if (attendanceDetail != null)
            {
                attendanceDetail.ImageUrl = imageUrl;

                await _context.SaveChangesAsync();
            }
        }

        private async Task<List<Class>> GetClassesWithoutAttendance(string type, DateTime date)
        {
            var activeClasses = await _context.Classes
                .Where(c => c.Status == 2)
                .ToListAsync();

            var classesWithAttendance = await _context.Attendances
                .Where(a => a.CreatedAt.Date == date && a.Type == type)
                .Select(a => a.ClassId)
                .ToListAsync();

            return activeClasses
                .Where(c => !classesWithAttendance.Contains(c.ClassId))
                .ToList();
        }
        private async Task<bool> HasStudentsInClass(int classId)
        {
            var classItem = await _context.Classes
                .Where(c => c.ClassId == classId)
                .Include(c => c.ClassHasChildren)
                .ThenInclude(c => c.Student)
                .FirstOrDefaultAsync();

            return classItem?.ClassHasChildren?.Any() ?? false;
        }

        public async Task<int?> GetClassIdByStudentId(int studentId)
        {
            var classId = await _context.Classes
                .Where(c => c.IsActive == 1 && c.ClassHasChildren.Any(cc => cc.StudentId == studentId)) 
                .Select(c => c.ClassId)
                .FirstOrDefaultAsync();

            return classId;
        }

        public async Task<List<AttendanceInfo>> GetAttendanceInfoAsync(int attendanceID)
        {
            var attendanceInfo = await _context.Attendances
                .Where(a => a.AttendanceId == attendanceID)
                .Join(
                    _context.AttendanceDetails,
                    a => a.AttendanceId,
                    ad => ad.AttendanceId,
                    (a, ad) => new AttendanceInfo
                    {
                        CreatedAt = a.CreatedAt,
                        ClassID = a.ClassId,
                        Type = a.Type
                    })
                .ToListAsync();

            return attendanceInfo;
        }
    }
}
