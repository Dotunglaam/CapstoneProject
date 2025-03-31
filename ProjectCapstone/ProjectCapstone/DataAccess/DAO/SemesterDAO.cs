using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class SemesterDAO
    {
        private readonly kmsContext _context;
        private readonly IMapper _mapper;

        public SemesterDAO(kmsContext dbContext, IMapper mapper)
        {
            _context = dbContext;
            _mapper = mapper;
        }

        public async Task<List<SemesterMapper>> GetAllSemesters()
        {
            try
            {
                var allSemesters = await _context.Semesters.
                    ToListAsync();
                return _mapper.Map<List<SemesterMapper>>(allSemesters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving school years: " + ex.Message);
            }
        }

        public async Task<int> AddSemester(SemesterMapper semesterMapper)
        {
            try
            {
                // Kiểm tra ngày bắt đầu và kết thúc
                ValidateSchoolyearsDates(semesterMapper.StartDate, semesterMapper.EndDate);

                // Ánh xạ và tạo mới năm học
                var semesterEntity = _mapper.Map<Semester>(semesterMapper);
                semesterEntity.Status = 1;

                await _context.Semesters.AddAsync(semesterEntity);
                await _context.SaveChangesAsync();
                return semesterEntity.SemesterId;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding school year: " + ex.Message);
            }
        }

        // Sửa năm học
        public async Task UpdateSemester(SemesterMapper semesterMapper)
        {
            try
            {
                // Tìm năm học hiện tại
                var existingSemester = await _context.Semesters.FindAsync(semesterMapper.SemesterId);
                if (existingSemester == null)
                {
                    throw new Exception("School year not found.");
                }

                // Chỉ kiểm tra ngày bắt đầu và kết thúc nếu chúng thay đổi
                bool isStartDateChanged = existingSemester.StartDate != semesterMapper.StartDate;
                bool isEndDateChanged = existingSemester.EndDate != semesterMapper.EndDate;

                if (isStartDateChanged || isEndDateChanged)
                {
                    ValidateSchoolyearsDates(semesterMapper.StartDate, semesterMapper.EndDate, semesterMapper.SemesterId);
                }

                // Ánh xạ dữ liệu mới sang đối tượng hiện có
                _mapper.Map(semesterMapper, existingSemester);

                // Cập nhật dữ liệu
                _context.Semesters.Update(existingSemester);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating school year: " + ex.Message);
            }
        }

        public async Task DeleteSemester(int semesterId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var semester = await _context.Semesters.FindAsync(semesterId);
                if (semester == null)
                {
                    throw new Exception("School year not found.");
                }

                // Xóa dữ liệu liên quan trong bảng Class
                var relatedClasses = await _context.Classes
                    .Where(c => c.SemesterId == semesterId)
                    .ToListAsync();
                if (relatedClasses.Any())
                {
                    _context.Classes.RemoveRange(relatedClasses);
                }

                // Xóa dữ liệu liên quan trong bảng Tuition
                var relatedTuitions = await _context.Tuitions
                    .Where(t => t.SemesterId == semesterId)
                    .ToListAsync();
                if (relatedTuitions.Any())
                {
                    _context.Tuitions.RemoveRange(relatedTuitions);
                }

                // Lấy các bản ghi trong bảng class_has_children liên quan đến năm học này
                var relatedClassChildren = await _context.ClassHasChildren
                    .Where(c => c.Class.SemesterId == semesterId)
                    .ToListAsync();

                // Cập nhật lại status của các bản ghi child trước khi xóa
                if (relatedClassChildren.Any())
                {
                    var childIds = relatedClassChildren.Select(c => c.StudentId).ToList();
                    var relatedChildren = await _context.Children
                        .Where(c => childIds.Contains(c.StudentId))
                        .ToListAsync();

                    foreach (var child in relatedChildren)
                    {
                        child.Status = 0;  // Cập nhật status về 0
                    }

                    _context.Children.UpdateRange(relatedChildren);
                }

                // Xóa các bản ghi trong bảng class_has_children
                if (relatedClassChildren.Any())
                {
                    _context.ClassHasChildren.RemoveRange(relatedClassChildren);
                }

                // Xóa năm học
                _context.Semesters.Remove(semester);

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                // Xác nhận giao dịch
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Hủy giao dịch khi có lỗi
                await transaction.RollbackAsync();
                throw new Exception("Error deleting school year: " + ex.Message);
            }
        }
        public async Task<int> AddSemesterReal(int schoolYearsID, Semesterreal1 semesterreal1)
        {
            try
            {
                var existingSemesters = await _context.Semesterreals
                   .Where(s => s.SemesterId == schoolYearsID)
                   .ToListAsync();

                var schoolYear = await _context.Semesters.FindAsync(schoolYearsID);

                if (semesterreal1.StartDate <= schoolYear.StartDate || semesterreal1.EndDate >= schoolYear.EndDate)
                {
                    throw new Exception("Semester dates must fall within the school year range.");
                }
                // Kiểm tra ngày bắt đầu và kết thúc
                foreach (var existingSemester in existingSemesters)
                {
                    if ((semesterreal1.StartDate >= existingSemester.StartDate && semesterreal1.StartDate < existingSemester.EndDate) ||
                        (semesterreal1.EndDate > existingSemester.StartDate && semesterreal1.EndDate <= existingSemester.EndDate) ||
                        (semesterreal1.StartDate <= existingSemester.StartDate && semesterreal1.EndDate >= existingSemester.EndDate))
                    {
                        throw new Exception("The specified start date and end date overlap with an existing semester for the same school year.");
                    }
                }
                // Ánh xạ và tạo mới năm học
                var semesterEntity = _mapper.Map<Semesterreal>(semesterreal1);
                semesterEntity.SemesterId = schoolYearsID;

                await _context.Semesterreals.AddAsync(semesterEntity);
                await _context.SaveChangesAsync();
                return semesterEntity.SemesterrealId;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding school year: " + ex.Message);
            }
        }

        public async Task UpdateSemesterReal(int schoolYearsId, Semesterreal1 semesterreal1)
        {
            try
            {   

                // Tìm kỳ học hiện tại
                var existingSemester = await _context.Semesterreals.FindAsync(semesterreal1.SemesterrealId);
                if (existingSemester == null)
                {
                    throw new Exception("Semester not found.");
                }
                var schoolYear = await _context.Semesters.FindAsync(schoolYearsId);

                if (semesterreal1.StartDate <= schoolYear.StartDate || semesterreal1.EndDate >= schoolYear.EndDate)
                {
                    throw new Exception("Semester dates must fall within the school year range.");
                }
                // Chỉ kiểm tra ngày bắt đầu và kết thúc nếu chúng thay đổi
                bool isStartDateChanged = existingSemester.StartDate != semesterreal1.StartDate;
                bool isEndDateChanged = existingSemester.EndDate != semesterreal1.EndDate;

                if (isStartDateChanged || isEndDateChanged)
                {
                    // Lấy danh sách các kỳ học khác trong cùng năm học
                    var overlappingSemesters = await _context.Semesterreals
                        .Where(s => s.SemesterId == schoolYearsId && s.SemesterrealId != semesterreal1.SemesterrealId)
                        .Where(s =>
                            (semesterreal1.StartDate >= s.StartDate && semesterreal1.StartDate < s.EndDate) ||  // Ngày bắt đầu nằm trong khoảng của kỳ học khác
                            (semesterreal1.EndDate > s.StartDate && semesterreal1.EndDate <= s.EndDate) ||     // Ngày kết thúc nằm trong khoảng của kỳ học khác
                            (semesterreal1.StartDate <= s.StartDate && semesterreal1.EndDate >= s.EndDate))   // Bao trùm hoàn toàn kỳ học khác
                        .ToListAsync();

                    if (overlappingSemesters.Any())
                    {
                        throw new Exception("The specified start date and end date overlap with an existing semester for the same school year.");
                    }
                }

                // Ánh xạ dữ liệu mới sang đối tượng hiện có
                _mapper.Map(semesterreal1, existingSemester);

                // Cập nhật dữ liệu
                _context.Semesterreals.Update(existingSemester);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating semester: " + ex.Message);
            }
        }

        public async Task<List<Semesterreal1>> GetAllSemesterRealBySchoolYearID(int schoolYearId)
        {
            try
            {
                // Lấy danh sách kỳ học theo SchoolYearID
                var semesters = await _context.Semesterreals
                    .Where(s => s.SemesterId == schoolYearId)
                    .ToListAsync();

                // Chuyển đổi sang DTO hoặc lớp ánh xạ (nếu cần)
                var result = _mapper.Map<List<Semesterreal1>>(semesters);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving semesters: " + ex.Message);
            }
        }
        public async Task<bool> DeleteSemesterReal(int semesterRealId)
        {
            try
            {
                // Tìm kỳ học cần xóa
                var semesterReal = await _context.Semesterreals.FindAsync(semesterRealId);
                if (semesterReal == null)
                {
                    throw new Exception("The specified semester real does not exist.");
                }

                // Xóa kỳ học
                _context.Semesterreals.Remove(semesterReal);
                await _context.SaveChangesAsync();

                return true; // Trả về true nếu xóa thành công
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting semester real: " + ex.Message);
            }
        }

        private void ValidateSchoolyearsDates(DateTime? startDate, DateTime? endDate, int? currentSemesterId = null)
        {
            if (startDate == null || endDate == null)
            {
                throw new Exception("Start date and end date cannot be null.");
            }

            // Kiểm tra ngày bắt đầu và kết thúc hợp lệ
            if (startDate >= endDate)
            {
                throw new Exception("Start date must be before end date.");
            }

            // Kiểm tra ngày bắt đầu không sớm hơn 5/9
            if (startDate.Value.Month < 9 || (startDate.Value.Month == 9 && startDate.Value.Day < 5))
            {
                throw new Exception("The school year must start on or after September 5.");
            }

            // Kiểm tra ngày kết thúc không muộn hơn 30/6
            if (endDate.Value.Month > 6 || (endDate.Value.Month == 6 && endDate.Value.Day > 30))
            {
                throw new Exception("The school year must end on or before June 30.");
            }

            // Kiểm tra sự trùng lặp với các năm học khác
            var overlappingSemesters = _context.Semesters
                .Where(s => currentSemesterId == null || s.SemesterId != currentSemesterId)
                .Where(s =>
                    (startDate >= s.StartDate && startDate < s.EndDate) ||  // Ngày bắt đầu nằm trong khoảng của năm học khác
                    (endDate > s.StartDate && endDate <= s.EndDate) ||     // Ngày kết thúc nằm trong khoảng của năm học khác
                    (startDate <= s.StartDate && endDate >= s.EndDate)     // Bao trùm hoàn toàn năm học khác
                )
                .ToList();

            if (overlappingSemesters.Any())
            {
                throw new Exception("The specified start date and end date overlap with an existing school year.");
            }
        }
    }
}
