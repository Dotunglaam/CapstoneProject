using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using OfficeOpenXml;
using Microsoft.AspNetCore.Http;
using AutoMapper.Execution;

namespace DataAccess.DAO
{
    public class ClassDAO
    {
        private readonly kmsContext _context;
        private readonly IMapper _mapper;

        public ClassDAO(kmsContext dbContext, IMapper mapper)
        {
            _context = dbContext;
            _mapper = mapper;
        }
        public async Task<List<ClassMapper2>> GetAllClass()
        {
            try
            {
                var getAllClasses = await _context.Classes
                    .Include(x => x.Teachers) // Bao gồm Teachers khi lấy Class
                    .ThenInclude(t => t.TeacherNavigation) // Bao gồm TeacherNavigation để lấy Code
                    .ToListAsync();
                // Ánh xạ các lớp với Teachers và mã Code của Teacher
                return _mapper.Map<List<ClassMapper2>>(getAllClasses);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving classes: " + ex.Message);
            }
        }
        public async Task<ClassMapper2> GetClassById(int classID)
        {
            try
            {
                var classEntity = await _context.Classes
                    .Include(x => x.Teachers)
                    .ThenInclude(t => t.TeacherNavigation)
                    .Include(x => x.School)
                    .FirstOrDefaultAsync(p => p.ClassId == classID);


                return _mapper.Map<ClassMapper2>(classEntity);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving class: " + ex.Message);
            }
        }

        public async Task<List<ClassMapper2>> GetClassesByTeacherId(int teacherId)
        {
            try
            {
                if (teacherId <= 0)
                {
                    throw new ArgumentException("TeacherId must greater than  0.");
                }

                var classes = await _context.Classes
                    .Where(c => c.Teachers.Any(t =>
                        t.TeacherId == teacherId &&
                        t.TeacherNavigation.Status == 1))
                    .Include(c => c.Teachers)
                    .ThenInclude(t => t.TeacherNavigation)
                    .ToListAsync();

                return _mapper.Map<List<ClassMapper2>>(classes);
            }
            catch (Exception ex)
            {
                throw new Exception( ex.Message);
            }
        }

        public async Task<List<ClassMapper2>> GetClasssByStudentId(int studentId)
        {
            if (studentId <= 0)
            {
                throw new ArgumentException("StudentId phải lớn hơn 0.");
            }

            try
            {
                var classes = await _context.ClassHasChildren
                    .Where(ch => ch.StudentId == studentId &&
                                 ch.Class.Teachers.Any(t => t.TeacherNavigation.Status == 1))
                    .Include(ch => ch.Class)
                    .ThenInclude(c => c.Teachers)
                    .ThenInclude(t => t.TeacherNavigation)
                    .ToListAsync();

                var classList = classes.Select(ch => _mapper.Map<ClassMapper2>(ch.Class)).ToList();

                return classList;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving classes by student ID: " + ex.Message);
            }
        }

        public async Task<List<ChildrenMapper>> GetStudentsByClassId(int classId)
        {
            try
            {
                var students = await _context.ClassHasChildren
                    .Where(ch => ch.ClassId == classId)
                    .Include(ch => ch.Student)
                    .ToListAsync();

               
                var childrenList = students.Select(ch => _mapper.Map<ChildrenMapper>(ch.Student)).ToList();

                return childrenList;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving students by class: " + ex.Message);
            }
        }

        public async Task<int> AddNewClass(ClassMapper classMapper)
        {
            try
            {
                // Kiểm tra khóa ngoại
                await ValidateForeignKeysAsync(classMapper);

                // Tìm lớp theo ClassId
                var isExist = await _context.Classes.AnyAsync(a => a.ClassName.ToLower().Trim() == classMapper.ClassName.ToLower().Trim()
                                          && a.ClassId != classMapper.ClassId);
                if (isExist)
                {
                    throw new Exception($"ClassName with name '{classMapper.ClassName}' already exists.");
                }
                // Thiết lập trạng thái mặc định nếu chưa được cung cấp
                classMapper.Status = classMapper.Status ?? 1;

                var classEntity = _mapper.Map<Class>(classMapper);

                // Thêm lớp mới vào cơ sở dữ liệu
                await _context.Classes.AddAsync(classEntity);
                await _context.SaveChangesAsync(); // Lưu thay đổi để lấy ClassId

                return classEntity.ClassId;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new class: " + ex.Message);
            }
        }
        public async Task UpdateClass(ClassMapper updatedClass)
        {
            try
            {
                // Kiểm tra sự tồn tại của School
                if (!await _context.Schools.AnyAsync(s => s.SchoolId == updatedClass.SchoolId))
                {
                    throw new Exception("Invalid School. The School does not exist.");
                }

                // Kiểm tra sự tồn tại của Semester
                if (!await _context.Semesters.AnyAsync(s => s.SemesterId == updatedClass.SemesterId))
                {
                    throw new Exception("Invalid Semester. The Semester does not exist.");
                }

                // Kiểm tra sự tồn tại của Grade
                if (!await _context.Grades.AnyAsync(g => g.GradeId == updatedClass.GradeId))
                {
                    throw new Exception("Invalid Grade. The Grade does not exist.");
                }

                // Tìm lớp theo ClassId
                var existingClass = await _context.Classes.FirstOrDefaultAsync(c => c.ClassId == updatedClass.ClassId);
                if (existingClass == null)
                {
                    throw new Exception($"ClassId {updatedClass.ClassId} does not exist.");
                }

                // Kiểm tra trùng lặp
                var isDuplicate = await _context.Classes.AnyAsync(c =>
                    c.ClassId != updatedClass.ClassId && // Loại trừ chính đối tượng đang cập nhật
                    c.GradeId == updatedClass.GradeId &&
                    c.SemesterId == updatedClass.SemesterId &&
                    c.SchoolId == updatedClass.SchoolId &&
                    c.ClassName.ToLower().Trim() == updatedClass.ClassName.ToLower().Trim()
                );

                if (isDuplicate)
                {
                    throw new Exception($"A class with the same GradeId, SemesterId, SchoolId, and ClassName already exists in the database.");
                }

                // Cập nhật thông tin lớp
                existingClass.ClassName = updatedClass.ClassName ?? existingClass.ClassName; // Giữ giá trị cũ nếu null
                existingClass.Number = updatedClass.Number ?? existingClass.Number;
                existingClass.IsActive = updatedClass.IsActive ?? existingClass.IsActive;
                existingClass.SchoolId = updatedClass.SchoolId;
                existingClass.SemesterId = updatedClass.SemesterId;
                existingClass.GradeId = updatedClass.GradeId;
                existingClass.Status = updatedClass.Status ?? existingClass.Status;

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.Classes.Update(existingClass);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error when updating class: {ex.Message}", ex);
            }
        }
        public async Task UpdateClassStatus(int classId, int newStatus)
        {
            try
            {
                // Tìm lớp theo ClassId
                var existingClass = await _context.Classes.FirstOrDefaultAsync(c => c.ClassId == classId);
                if (existingClass == null)
                {
                    throw new Exception($"ClassId {classId} does not exist.");
                }

                // Cập nhật trường Status
                existingClass.Status = newStatus;

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.Classes.Update(existingClass);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error when updating class status: {ex.Message}", ex);
            }
        }
        public async Task RemoveTeacherFromClass(int classId, int teacherId)
        {
            try
            {
                // Tìm lớp học và bao gồm danh sách giáo viên liên quan.
                var currentClass = await _context.Classes
                    .Include(c => c.Teachers)
                    .FirstOrDefaultAsync(c => c.ClassId == classId);

                if (currentClass == null)
                {
                    throw new Exception("Class not found.");
                }

                // Kiểm tra trạng thái lớp học (Active).
                if (currentClass.IsActive != 1) 
                {
                    throw new Exception("Cannot remove teacher from an inactive class.");
                }

                // Kiểm tra xem giáo viên có thuộc lớp học hay không.
                var teacherToRemove = currentClass.Teachers.FirstOrDefault(t => t.TeacherId == teacherId);
                if (teacherToRemove == null)
                {
                    throw new Exception("The teacher is not assigned to this class.");
                }

                // Kiểm tra xem giáo viên có phải là giáo viên chủ nhiệm hay không.
                if (teacherToRemove.HomeroomTeacher == 1)
                {
                    throw new Exception("Cannot remove the current HomeroomTeacher. Please assign a new HomeroomTeacher before removing this teacher.");
                }

                // Xóa giáo viên khỏi lớp học.
                currentClass.Teachers.Remove(teacherToRemove);

                // Cập nhật lớp học trong context và lưu thay đổi.
                _context.Classes.Update(currentClass);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<TeacherMapper>> GetTeachersWithoutClass()
        {
            try
            {
                // Lấy danh sách giáo viên chưa có lớp hoặc các lớp đều có IsActive == 0
                var teachersWithoutClass = await _context.Teachers
                    .Where(t =>
                        t.TeacherNavigation.Status == 1 // Giáo viên phải có trạng thái 1
                        && (!t.Classes.Any() // Giáo viên không có lớp
                            || t.Classes.All(c => c.IsActive == 0))) // Hoặc tất cả lớp của giáo viên có IsActive == 0
                    .ToListAsync();

                // Ánh xạ danh sách sang TeacherMapper
                return _mapper.Map<List<TeacherMapper>>(teachersWithoutClass);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task SendMailToParentsByClassId(int classId)
        {
            try
            {
                // Lấy danh sách học sinh từ classId
                var students = await GetStudentsByClassId(classId);

                // Lọc các học sinh có Status là 1
                var activeStudentIds = students.Where(s => s.Status == 1).Select(s => s.StudentId).ToList();

                if (!activeStudentIds.Any())
                {
                    throw new Exception("Không có học sinh nào trong lớp với trạng thái active (1).");
                }

                // Lấy thông tin phụ huynh của các học sinh active
                var parentDetails = await _context.Children
                    .Where(c => activeStudentIds.Contains(c.StudentId)) // Chỉ lấy học sinh active
                    .Select(c => new
                    {
                        ParentName = c.Parent.Name,                   // Tên phụ huynh
                        ParentMail = c.Parent.ParentNavigation.Mail,
                        StudentName = c.FullName,                     // Tên học sinh
                        GradeName = c.Grade.Name                      // Tên lớp
                    })
                    .ToListAsync();

                if (parentDetails.Any())
                {
                    foreach (var detail in parentDetails)
                    {
                        string subject = "Thông báo lịch học KMS";
                        string body = $@"
                        <html>
                        <body style=""font-family: Arial, sans-serif;"">
                            <div style=""border: 1px solid #ccc; padding: 20px; width: 100%; box-sizing: border-box;"">
                                <p>Kính gửi Quý khách hàng <b>{detail.ParentName}</b>,</p>
                                <p>Xin trân trọng thông báo lịch học của con Quý khách:</p>
                                <p>- Tên trẻ: <b>{detail.StudentName}</b></p>
                                <p>- Thuộc lớp (Grade): <b>{detail.GradeName}</b></p>
                                <p>Quý khách hãy kiểm tra hệ thống trên Web để xem chi tiết.</p>      
                                <p>Trân trọng cảm ơn Quý khách !!!</p>
                            </div>  
                        </body>
                        </html>";

                        Console.WriteLine($"Preparing to send email to: {detail.ParentMail}");
                        Console.WriteLine($"Subject: {subject}");
                        Console.WriteLine($"Body: {body}");

                        // Gửi email
                        SendMailToParent(detail.ParentMail, subject, body);
                    }
                }
                else
                {
                    throw new Exception("Không có phụ huynh nào để gửi mail.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi gửi mail: " + ex.Message);
            }
        }  
        private async Task ValidateForeignKeysAsync(ClassMapper classMapper)
        {
            var schoolExists = await _context.Schools.AnyAsync(s => s.SchoolId == classMapper.SchoolId);
            if (!schoolExists)
            {
                throw new Exception("Invalid School. The School does not exist.");
            }

            if (classMapper.Number <= 0)
            {
                throw new Exception("Không đủ học sinh chưa có lớp");
            }
            var semesterExists = await _context.Semesters.AnyAsync(s => s.SemesterId == classMapper.SemesterId);
            if (!semesterExists)
            {
                throw new Exception("Invalid Semester. The Semester does not exist.");
            }

            var gradeExists = await _context.Grades.AnyAsync(g => g.GradeId == classMapper.GradeId);
            if (!gradeExists)
            {
                throw new Exception("Invalid Grade. The Grade does not exist.");
            }
        }
        public async Task AddTeacherToClass(int classId, int teacherId)
        {
            try
            {
                // Find the class and include its related teachers.
                var currentClass = await _context.Classes
                    .Include(c => c.Teachers)
                    .FirstOrDefaultAsync(c => c.ClassId == classId );
                if (currentClass == null)
                {
                    throw new Exception("Class is not active");
                }

                if (currentClass.IsActive != 1)
                {
                    throw new Exception("Cannot add teacher to an inactive class.");
                }
                // Find the teacher by their ID.
                var currentTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherId == teacherId);
                if (currentTeacher == null)
                {
                    throw new Exception("Teacher not found.");
                }

                // Check if the teacher is already assigned to this class.
                if (currentClass.Teachers.Any(t => t.TeacherId == teacherId))
                {
                    throw new Exception("The teacher is already assigned to this class.");
                }

                // Check if the teacher is assigned to another class.
                var existingClasses = await _context.Classes
                    .Where(c => c.Teachers.Any(t => t.TeacherId == teacherId) && c.ClassId != classId)
                    .ToListAsync();

                if (existingClasses.Any())
                {
                    throw new Exception("The teacher is already assigned to another class. Cannot add to this class.");
                }

                // Add the teacher to the class.
                currentClass.Teachers.Add(currentTeacher);

                // Update the class in the context and save changes.
                _context.Classes.Update(currentClass);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding teacher to class: " + ex.Message);
            }
        }
        public async Task UpdateTeacherToClass(int classId, int currentTeacherId, int newTeacherId)
        {
            try
            {
                // Tìm lớp học và bao gồm danh sách giáo viên liên kết.
                var currentClass = await _context.Classes
                    .Include(c => c.Teachers)
                    .FirstOrDefaultAsync(c => c.ClassId == classId);

                if (currentClass == null)
                {
                    throw new Exception("Class not found.");
                }
                if (currentClass.IsActive != 1)
                {
                    throw new Exception("Cannot Update Teacher To Class from an inactive class.");
                }
                // Kiểm tra giáo viên hiện tại có tồn tại hay không.
                var currentTeacher = currentClass.Teachers.FirstOrDefault(t => t.TeacherId == currentTeacherId);
                if (currentTeacher == null)
                {
                    throw new Exception("The current teacher is not assigned to this class.");
                }

                // Kiểm tra giáo viên mới có tồn tại trong hệ thống hay không.
                var newTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherId == newTeacherId);
                if (newTeacher == null)
                {
                    throw new Exception("New teacher not found.");
                }

                // Kiểm tra nếu giáo viên mới đã được gán vào lớp học này.
                if (currentClass.Teachers.Any(t => t.TeacherId == newTeacherId))
                {
                    throw new Exception("The new teacher is already assigned to this class.");
                }

                // Kiểm tra nếu giáo viên mới đã được gán vào một lớp học khác.
                var existingClasses = await _context.Classes
                    .Where(c => c.Teachers.Any(t => t.TeacherId == newTeacherId) && c.ClassId != classId)
                    .ToListAsync();

                if (existingClasses.Any())
                {
                    throw new Exception("The new teacher is already assigned to another class. Cannot update to this class.");
                }

                // Thay thế giáo viên hiện tại bằng giáo viên mới.
                currentClass.Teachers.Remove(currentTeacher);
                currentClass.Teachers.Add(newTeacher);

                // Cập nhật và lưu thay đổi.
                _context.Classes.Update(currentClass);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating teacher in class: " + ex.Message);
            }
        }
        public async Task UpdateHomeroomTeacher(int classId, int teacherId)
        {
            try
            {   
                // Tìm lớp học theo classId, bao gồm danh sách giáo viên.
                var currentClass = await _context.Classes
                    .Include(c => c.Teachers)
                    .FirstOrDefaultAsync(c => c.ClassId == classId); // Thêm điều kiện kiểm tra trạng thái lớp là 1
               
                // Kiểm tra xem giáo viên có thuộc lớp này hay không.
                var targetTeacher = currentClass.Teachers.FirstOrDefault(t => t.TeacherId == teacherId);
                if (targetTeacher == null)
                {
                    throw new Exception("Teacher does not belong to this class.");
                }
                if (currentClass.IsActive != 1)
                {
                    throw new Exception("Cannot update homeroom teacher from an inactive class.");
                }
                // Cập nhật trạng thái HomeroomTeacher của các giáo viên trong lớp.
                foreach (var teacher in currentClass.Teachers)
                {
                    teacher.HomeroomTeacher = (teacher.TeacherId == teacherId) ? (sbyte)1 : (sbyte)0;
                }

                // Lưu thay đổi vào cơ sở dữ liệu.
                _context.Classes.Update(currentClass);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception( ex.Message);
            }
        }
        public void SendMailToParent(string email, string sub, string body)
        {
            try
            {
                // Cấu hình SmtpClient
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials =
                    new NetworkCredential("lamdthe163085@fpt.edu.vn", "iyni xhmb rtij tnen");
                smtpClient.EnableSsl = true;

                // Tạo đối tượng MailMessage
                MailMessage message = new MailMessage();
                message.From = new MailAddress("lamdthe163085@fpt.edu.vn");
                message.To.Add(email);
                message.Subject = sub;
                message.Body = body;
                message.IsBodyHtml = true; // Đảm bảo nội dung email là HTML

                // Gửi email
                smtpClient.Send(message);
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
        public async Task<string> ImportClassExcel(IFormFile file)
        {
            try
            {
                // Kiểm tra file
                if (file == null || file.Length == 0)
                {
                    throw new Exception("File Invalid.");
                }

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);

                    // Cấu hình giấy phép EPPlus
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        if (worksheet == null)
                        {
                            throw new Exception("Not Found Sheet In Excel.");
                        }

                        // Lấy thông tin trường, kỳ học, khối lớp từ tiêu đề
                        var schoolInfo = worksheet.Cells[2, 1].Value?.ToString();
                        var semesterInfo = worksheet.Cells[2, 2].Value?.ToString();
                        var gradeInfo = worksheet.Cells[2, 3].Value?.ToString();

                        if (string.IsNullOrEmpty(schoolInfo) || string.IsNullOrEmpty(semesterInfo) || string.IsNullOrEmpty(gradeInfo))
                        {
                            throw new Exception("Missing school, semester, or grade information in the header.");
                        }

                        int schoolId = int.Parse(schoolInfo.Split('-')[0].Trim());
                        int semesterId = int.Parse(semesterInfo.Split('-')[0].Trim());
                        int gradeId = int.Parse(gradeInfo.Split('-')[0].Trim());

                        // Kiểm tra khóa ngoại (tải trước dữ liệu)
                        var schoolExists = await _context.Schools.AnyAsync(s => s.SchoolId == schoolId);
                        var semesterExists = await _context.Semesters.AnyAsync(s => s.SemesterId == semesterId);
                        var gradeExists = await _context.Grades.AnyAsync(g => g.GradeId == gradeId);

                        if (!schoolExists) throw new Exception($"School {schoolId} does not exist.");
                        if (!semesterExists) throw new Exception($"Semester {semesterId} does not exist.");
                        if (!gradeExists) throw new Exception($"Grade {gradeId} does not exist.");

                        // Tải danh sách giáo viên để giảm số lần truy vấn
                        var allTeachers = await _context.Teachers.Include(t => t.TeacherNavigation).ToDictionaryAsync(t => t.TeacherId, t => t);

                        // Tạo dictionary để lưu lớp
                        var classDictionary = new Dictionary<string, Class>();

                        string previousClassName = null; // Biến lưu tên lớp trước đó

                        for (int row = 4; row <= worksheet.Dimension.End.Row; row++)
                        {
                            // Đọc tên lớp từ cột A
                            var className = worksheet.Cells[row, 1].Value?.ToString()?.Trim();

                            // Nếu ô A trống, giữ lại tên lớp từ dòng trước đó
                            if (string.IsNullOrEmpty(className))
                            {
                                className = previousClassName;
                            }
                            else
                            {
                                previousClassName = className; // Cập nhật tên lớp mới
                            }

                            if (string.IsNullOrEmpty(className)) continue;

                            // Đọc số lượng học sinh từ cột B (chỉ khi dòng đầu tiên của lớp được xác định)
                            var numberCell = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                            int numberOfStudents = 0;

                            if (row == 4 || !string.IsNullOrEmpty(numberCell))
                            {
                                if (!string.IsNullOrEmpty(numberCell) && !int.TryParse(numberCell, out numberOfStudents))
                                {
                                    throw new Exception($"Row {row}: Invalid number of students.");
                                }
                            }

                            // Kiểm tra lớp trong dictionary
                            if (!classDictionary.ContainsKey(className))
                            {
                                classDictionary[className] = new Class
                                {
                                    ClassName = className,
                                    Number = numberOfStudents,
                                    SchoolId = schoolId,
                                    SemesterId = semesterId,
                                    GradeId = gradeId,
                                    Status = 1,
                                    IsActive = 1,
                                    Teachers = new List<Teacher>()
                                };
                            }

                            var currentClass = classDictionary[className];

                            // Đọc giáo viên từ cột C
                            var teacherCell = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                            if (string.IsNullOrEmpty(teacherCell)) continue;

                            if (!int.TryParse(teacherCell.Split('-')[0].Trim(), out var teacherId))
                            {
                                throw new Exception($"Row {row}: Invalid Teacher ID format.");
                            }

                            if (!allTeachers.ContainsKey(teacherId))
                            {
                                throw new Exception($"Row {row}: Teacher with ID {teacherId} does not exist.");
                            }

                            if (allTeachers[teacherId].TeacherNavigation?.Status != 1)
                            {
                                throw new Exception($"Row {row}: Teacher with ID {teacherId} is not active.");
                            }
                            var teacher = allTeachers[teacherId];

                            // Kiểm tra nếu giáo viên đã được gán cho lớp khác
                            if (await _context.Classes.AnyAsync(c => c.Teachers.Any(t => t.TeacherId == teacherId) && c.IsActive == 1))
                            {
                                throw new Exception($"Row {row}: Teacher {teacher.Name} is already assigned to another class.");
                            }

                            // Thêm giáo viên vào lớp nếu chưa có
                            if (!currentClass.Teachers.Any(t => t.TeacherId == teacherId))
                            {
                                currentClass.Teachers.Add(teacher);
                            }
                            Console.WriteLine($"End Row: {worksheet.Dimension.End.Row}");
                            Console.WriteLine($"Row {row}: ClassName = {className}, Number = {numberOfStudents}, Teacher = {teacherCell}");
                        }

                        // Lưu các lớp vào cơ sở dữ liệu
                        if (classDictionary.Values.Any())
                        {
                            var newClasses = classDictionary.Values.ToList();
                            await _context.Classes.AddRangeAsync(newClasses);
                            await _context.SaveChangesAsync();
                        }

                        return $"Successfully imported {classDictionary.Count} classes.";
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}
