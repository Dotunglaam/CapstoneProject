using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Xunit.Sdk;

namespace ProjectCapstone.Test.Classes
{
    public class ClassDAOTest
    {
        private readonly IMapper _mapper;
        private readonly DbContextOptions<kmsContext> _dbOptions;

        public ClassDAOTest()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                // Configure mapping between DTO and Entity
                cfg.CreateMap<Class, ClassMapper>().ReverseMap();
            });
            _mapper = mapperConfig.CreateMapper();

            _dbOptions = new DbContextOptionsBuilder<kmsContext>()
                .UseInMemoryDatabase("TestDatabase")
                .EnableSensitiveDataLogging()
                .Options;
        }

        // Seed database method to prepare the test data
        // Hàm giả lập dữ liệu cho cơ sở dữ liệu
        private async Task GiảLậpDữLiệu(kmsContext context, bool tồnTại = false, int trườngId = 1, int khốiId = 1, int họcKỳId = 1)
        {
            // Xóa dữ liệu cũ và tạo lại cơ sở dữ liệu
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            if (tồnTại)
            {
                // Thêm trường học nếu chưa có
                if (!await context.Schools.AnyAsync(s => s.SchoolId == trườngId))
                {
                    context.Schools.Add(new School
                    {
                        SchoolId = trườngId,
                        SchoolDes = "Trường Thử Nghiệm"
                    });
                }

                // Thêm khối nếu chưa có
                if (!await context.Grades.AnyAsync(g => g.GradeId == khốiId))
                {
                    context.Grades.Add(new BusinessObject.Models.Grade
                    {
                        GradeId = khốiId,
                        Name = "Khối 1",
                        BaseTuitionFee = 1000000 // Học phí cơ bản
                    });
                }

                // Thêm học kỳ nếu chưa có
                if (!await context.Semesters.AnyAsync(h => h.SemesterId == họcKỳId))
                {
                    context.Semesters.Add(new Semester
                    {
                        SemesterId = họcKỳId,
                        Name = "Học Kỳ 1",
                        StartDate = DateTime.Parse("2025-01-01"),
                        EndDate = DateTime.Parse("2025-05-31"),
                        Status = 1
                    });
                }

                // Thêm lớp học nếu chưa có
                if (!await context.Classes.AnyAsync(c => c.ClassId == 1))
                {
                    context.Classes.Add(new Class
                    {
                        ClassId = 1,
                        ClassName = "Lớp A",
                        Number = 5,
                        SchoolId = trườngId,
                        GradeId = khốiId,
                        SemesterId = họcKỳId,
                        Status = 1
                    });
                }
                // Thêm học sinh nếu chưa có
                if (!await context.Children.AnyAsync(s => s.StudentId == 1))
                {
                    context.Children.Add(new Child
                    {
                        StudentId = 1,
                        Code = "S001",
                        FullName = "Học Sinh A",
                        NickName = "HS A",
                        GradeId = 1 ,
                        Dob = DateTime.Parse("2010-01-01"),
                        Gender = 1 ,
                        Status = 1,
                        EthnicGroups = "Kinh",
                        Nationality = "Việt Nam",
                        Religion = "Không",
                        ParentId = 1,
                        Avatar = "avatar.jpg"
                    });
                }

                // Liên kết học sinh với lớp trong bảng ClassHasChild
                if (!await context.ClassHasChildren.AnyAsync(ch => ch.StudentId == 1 && ch.ClassId == 1))
                {
                    context.ClassHasChildren.Add(new ClassHasChild
                    {
                        ClassId = 1,
                        StudentId = 1,
                        Date = DateTime.Now // Ngày hiện tại khi thêm liên kết
                    });
                }

                await context.SaveChangesAsync();

            }
        }


        // The test method for adding new classes with various scenarios
        [Theory]
        [InlineData(1, "Lớp A", 10, 1, 1, 1, false)] // Trường hợp hợp lệ
        [InlineData(0, "Lớp B", 20, 1, 1, 1, true)]  // Trường không tồn tại
        [InlineData(1, "Lớp C", 0, 1, 1, 1, true)]   // Không có số lượng học sinh hợp lệ
        [InlineData(1, "Lớp D", 10, 0, 1, 1, true)] // Khối không tồn tại
        [InlineData(1, "Lớp E", 10, 1, 0, 1, true)] // Học kỳ không tồn tại

        public async Task ThêmLớpMới_XửLýCácTrườngHợpKhácNhau( int trườngId, string tênLớp, int sốLượng, int khốiId, int họcKỳId, int? trạngThái, bool dựKiếnNgoạiLệ)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                // Sắp xếp: Giả lập dữ liệu
                await GiảLậpDữLiệu(context, tồnTại: true, trườngId: trườngId, khốiId: khốiId, họcKỳId: họcKỳId);

                var classDAO = new ClassDAO(context, _mapper);

                // Dữ liệu lớp mới
                var lớpMới = new ClassMapper
                {
                    SchoolId = trườngId,
                    ClassName = tênLớp,
                    Number = sốLượng,
                    GradeId = khốiId,
                    SemesterId = họcKỳId,
                    Status = trạngThái
                };

                // Thực hiện: Kiểm tra xử lý ngoại lệ hoặc thành công
                if (dựKiếnNgoạiLệ)
                {
                    var ngoạiLệ = await Assert.ThrowsAsync<Exception>(async () => await classDAO.AddNewClass(lớpMới));

                    if (trườngId <= 0)
                    {
                        Assert.Contains("Invalid School. The School does not exist.", ngoạiLệ.Message);
                    }
                    else if (khốiId <= 0)
                    {
                        Assert.Contains("Invalid Grade. The Grade does not exist.", ngoạiLệ.Message);
                    }
                    else if (họcKỳId <= 0)
                    {
                        Assert.Contains("Invalid Semester. The Semester does not exist.", ngoạiLệ.Message);
                    }
                    else if (sốLượng <= 0)
                    {
                        Assert.Contains("Không đủ học sinh chưa có lớp", ngoạiLệ.Message);
                    }
                }
                else
                {
                    // Trường hợp hợp lệ
                    var lớpId = await classDAO.AddNewClass(lớpMới);

                    // Kiểm tra lớp được thêm thành công
                    var lớpĐượcThêm = await context.Classes.FirstOrDefaultAsync(c => c.ClassId == lớpId);
                    Assert.NotNull(lớpĐượcThêm);
                    Assert.Equal(tênLớp, lớpĐượcThêm.ClassName);
                    Assert.Equal(khốiId, lớpĐượcThêm.GradeId);
                    Assert.Equal(trườngId, lớpĐượcThêm.SchoolId);
                    Assert.Equal(trạngThái ?? 1, lớpĐượcThêm.Status); // Mặc định trạng thái là 1 nếu null
                }
            }
        }

        [Theory]
        [InlineData(1, true, false)] // Student ID hợp lệ, có dữ liệu
        [InlineData(999, true, false)] // Student ID không tồn tại, không có dữ liệu
        [InlineData(1, false, false)] // Database trống
        [InlineData(-1, true, false, true)]
        public async Task GetClasssByStudentId_ShouldHandleVariousScenarios(int studentId, bool seedData, bool expectData, bool shouldThrowException = false)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                // Arrange: Seed dữ liệu nếu cần
                if (seedData)
                {
                    await GiảLậpDữLiệu(context);
                }
                else
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                }

                var dao = new ClassDAO(context, _mapper);

                if (shouldThrowException)
                {
                    // Act & Assert: Kiểm tra ngoại lệ
                    var exception = await Assert.ThrowsAsync<Exception>(async () => await dao.GetClasssByStudentId(studentId));
                    Assert.NotNull(exception); // Đảm bảo exception không null
                    Assert.Contains("Invalid student ID", exception.Message); // Kiểm tra message của exception
                }
                else
                {
                    // Act
                    var result = await dao.GetClasssByStudentId(studentId);

                    // Assert
                    Assert.NotNull(result); // Kết quả không null
                    if (expectData)
                    {
                        Assert.NotEmpty(result); // Danh sách lớp học không trống
                        Assert.All(result, r => Assert.IsType<ClassMapper>(r)); 
                    }
                    else
                    {
                        Assert.Empty(result); 

                    }
                }
            }
        }
        [Theory]
        [InlineData(1, 1, true, true, false)]  // Thêm thành công
        [InlineData(1, 2, true, false, false)] // Giáo viên đã tồn tại trong lớp
        [InlineData(1, 3, true, true, true)]  // Giáo viên đã có lớp khác
        [InlineData(2, 1, false, true, true)] // Lớp không tồn tại
        [InlineData(1, 4, false, false, true)] // Giáo viên không tồn tại
        public async Task AddTeacherToClass_ShouldHandleVariousScenarios(
            int classId,
            int teacherId,
            bool seedData,
            bool teacherInSameClass,
            bool expectException)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                // Arrange: Seed dữ liệu nếu cần
                if (seedData)
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    // Tạo lớp học
                    var lop = new Class
                    {
                        ClassId = 1,
                        ClassName = "Lớp A"
                    };
                    context.Classes.Add(lop);

                    // Tạo giáo viên
                    var giaoVien1 = new Teacher { TeacherId = 1, Name = "GV 1" };
                    var giaoVien2 = new Teacher { TeacherId = 2, Name = "GV 2" }; 
                    var giaoVien3 = new Teacher { TeacherId = 3, Name = "GV 3" }; 
                    context.Teachers.AddRange(giaoVien1, giaoVien2, giaoVien3);

                    if (teacherInSameClass)
                    {
                        lop.Teachers.Add(giaoVien2);
                    }

                    if (teacherId == 3)
                    {
                        var lopKhac = new Class
                        {
                            ClassId = 2,
                            ClassName = "Lớp B"
                        };
                        lopKhac.Teachers.Add(giaoVien3);
                        context.Classes.Add(lopKhac);
                    }

                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                }

                var dao = new ClassDAO(context, _mapper);

                // Act
                if (expectException)
                {
                    var exception = await Assert.ThrowsAsync<Exception>(async () =>
                        await dao.AddTeacherToClass(classId, teacherId));
                    Assert.NotNull(exception); // Đảm bảo exception không null

                    if (teacherId == 2)
                    {
                        Assert.Contains("The teacher is already assigned to another class. Cannot add to this class.", exception.Message);
                    }
                    else if (teacherId == 3)
                    {
                        Assert.Contains("The teacher is already assigned to another class. Cannot add to this class.", exception.Message);
                    }
                    else
                    {
                        Assert.Contains("Error adding teacher to class: Class not found", exception.Message);
                    }
                }
                else
                {
                    await dao.AddTeacherToClass(classId, teacherId);

                    // Assert: Kiểm tra kết quả
                    var lop = await context.Classes
                        .Include(c => c.Teachers)
                        .FirstOrDefaultAsync(c => c.ClassId == classId);
                    Assert.NotNull(lop);
                    Assert.Contains(lop.Teachers, t => t.TeacherId == teacherId);
                }
            }
        }

        [Theory]
        [InlineData(1, true, false)] // Teacher ID hợp lệ, có lớp liên kết
        [InlineData(999, true, false)] // Teacher ID không tồn tại, không có lớp liên kết
        [InlineData(1, false, false)] // Cơ sở dữ liệu trống, không có dữ liệu
        [InlineData(-1, true, false, true)] // Teacher ID không hợp lệ, dự kiến ngoại lệ
        public async Task GetClassesByTeacherId_ShouldHandleVariousScenarios(int teacherId, bool seedData, bool expectData, bool shouldThrowException = false)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                // Sắp xếp: Giả lập dữ liệu nếu cần
                if (seedData)
                {
                    await GiảLậpDữLiệu(context);
                }
                else
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                }

                var dao = new ClassDAO(context, _mapper);

                if (shouldThrowException)
                {
                    // Thực hiện & Kiểm tra: Kiểm tra ngoại lệ
                    var exception = await Assert.ThrowsAsync<Exception>(async () => await dao.GetClassesByTeacherId(teacherId));
                    Assert.NotNull(exception); // Đảm bảo exception không null
                    Assert.Contains("Lỗi khi lấy danh sách lớp theo TeacherId", exception.Message); // Kiểm tra message của exception
                }
                else
                {
                    // Thực hiện
                    var result = await dao.GetClassesByTeacherId(teacherId);

                    // Kiểm tra
                    Assert.NotNull(result); // Kết quả không null
                    if (expectData)
                    {
                        Assert.NotEmpty(result); // Danh sách lớp không trống
                        Assert.All(result, r =>
                        {
                            Assert.IsType<ClassMapper2>(r); // Kiểm tra kiểu dữ liệu từng phần tử
                            Assert.NotNull(r.Teachers); // Kiểm tra thuộc tính Teachers không null
                            Assert.All(r.Teachers, teacher =>
                            {
                                Assert.NotNull(teacher.Code); // Đảm bảo Code của mỗi Teacher không null
                            });
                        });
                    }
                    else
                    {
                        Assert.Empty(result); // Danh sách lớp trống
                    }
                }
            }
        }
        [Theory]
        [InlineData(1, 2, true, false)]   // Lớp tồn tại, cập nhật thành công
        [InlineData(1, 0, true, false)]    // Lớp tồn tại, nhưng cập nhật với trạng thái không hợp lệ
        [InlineData(999, 2, false, true)] // Lớp không tồn tại, phải ném ngoại lệ
        public async Task UpdateClassStatus_ShouldHandleVariousScenarios(int classId, int newStatus, bool seedData, bool expectException)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                // Arrange: Seed dữ liệu nếu cần
                if (seedData)
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    // Thêm lớp giả vào cơ sở dữ liệu nếu cần
                    var lop = new Class
                    {
                        ClassId = 1,
                        ClassName = "Lớp A",
                        Status = 1
                    };
                    context.Classes.Add(lop);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                }

                var dao = new DataAccess.DAO.ClassDAO(context, _mapper);

                // Act
                if (expectException)
                {
                    var exception = await Assert.ThrowsAsync<Exception>(async () =>
                        await dao.UpdateClassStatus(classId, newStatus));
                    Assert.NotNull(exception); // Đảm bảo exception không null
                    if (classId == 999)
                    {
                        // Kiểm tra message ngoại lệ khi không tìm thấy lớp
                        Assert.Contains($"ClassId {classId} does not exist.", exception.Message);
                    }
                    else if (newStatus == 0)
                    {
                        // Kiểm tra message ngoại lệ khi trạng thái không hợp lệ (tùy theo logic trong phương thức UpdateClassStatus)
                        Assert.Contains("Invalid status value.", exception.Message); // Giả sử bạn đã thêm logic kiểm tra trạng thái không hợp lệ
                    }
                }
                else
                {
                    await dao.UpdateClassStatus(classId, newStatus);

                    // Assert: Kiểm tra cập nhật thành công
                    var updatedClass = await context.Classes.FirstOrDefaultAsync(c => c.ClassId == classId);
                    Assert.NotNull(updatedClass); // Kiểm tra lớp có tồn tại
                    Assert.Equal(newStatus, updatedClass.Status); // Kiểm tra trạng thái được cập nhật chính xác
                }
            }
        }
        [Theory]
        [InlineData(1, 1, 2, true, true)]  // Giáo viên đã thay thế thành công
        [InlineData(1, 1, 3, true, true)]   // Giáo viên đã có lớp khác
        [InlineData(1, 1, 1, false, true)]  // Giáo viên mới đã được gán vào lớp này
        [InlineData(999, 1, 2, false, true)] // Lớp không tồn tại
        [InlineData(1, 999, 2, true, true)] // Giáo viên hiện tại không tồn tại
        [InlineData(1, 1, 999, true, true)] // Giáo viên mới không tồn tại
        public async Task UpdateTeacherToClass_ShouldHandleVariousScenarios(
        int classId,
        int currentTeacherId,
        int newTeacherId,
        bool seedData,
        bool expectException)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                // Arrange: Seed dữ liệu nếu cần
                if (seedData)
                {
                    await GiảLậpDữLiệu(context);
                    // Tạo giáo viên
                    var teacher1 = new Teacher { TeacherId = 1, Name = "GV 1" };
                    var teacher2 = new Teacher { TeacherId = 2, Name = "GV 2" };
                    var teacher3 = new Teacher { TeacherId = 3, Name = "GV 3" };
                    context.Teachers.AddRange(teacher1, teacher2, teacher3);

                    // Thêm giáo viên vào lớp học
                    var currentClass = await context.Classes.Include(c => c.Teachers).FirstOrDefaultAsync(c => c.ClassId == classId);
                    if (currentClass != null)
                    {
                        currentClass.Teachers.Add(teacher1); // Thêm giáo viên hiện tại
                    }

                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                }

                var dao = new ClassDAO(context, _mapper);

                if (expectException)
                {
                    // Act & Assert: Kiểm tra ngoại lệ
                    var exception = await Assert.ThrowsAsync<Exception>(async () => await dao.UpdateTeacherToClass(classId, currentTeacherId, newTeacherId));
                    Assert.NotNull(exception); // Đảm bảo exception không null
                    Assert.Contains("Error updating teacher in class", exception.Message); // Kiểm tra message của exception
                }
                else
                {
                    // Act: Thực hiện cập nhật
                    await dao.UpdateTeacherToClass(classId, currentTeacherId, newTeacherId);

                    // Assert: Kiểm tra giáo viên trong lớp
                    var updatedClass = await context.Classes.Include(c => c.Teachers)
                        .FirstOrDefaultAsync(c => c.ClassId == classId);

                    var newTeacherAssigned = updatedClass?.Teachers.FirstOrDefault(t => t.TeacherId == newTeacherId);
                    var currentTeacherRemoved = updatedClass?.Teachers.FirstOrDefault(t => t.TeacherId == currentTeacherId);

                    // Kiểm tra: Giáo viên hiện tại đã bị xóa và giáo viên mới đã được thêm vào lớp
                    Assert.Null(currentTeacherRemoved);
                    Assert.NotNull(newTeacherAssigned);
                }
            }
        }
        [Theory]
        [InlineData(1, "Lớp A", 10, 1, 1, 1, false)] // Update valid class
        [InlineData(1, "Lớp C", 0, 1, 1, 1, false)] // Invalid student count
        [InlineData(1, "Lớp D", 10, 0, 1, 1, true)] // Grade does not exist
        [InlineData(1, "Lớp E", 15, 1, 1, 1, false)] // Duplicate class name within the same semester, grade, and school
        public async Task CậpNhậtLớp_XửLýCácTrườngHợpKhácNhau(int classId, string className, int number, int schoolId, int gradeId, int semesterId, bool dựKiếnNgoạiLệ)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                // Prepare the test data
                await GiảLậpDữLiệu(context, tồnTại: true, trườngId: schoolId, khốiId: gradeId, họcKỳId: semesterId);

                var classDAO = new ClassDAO(context, _mapper);

                // Prepare the updated class data
                var updatedClass = new ClassMapper
                {
                    ClassId = classId,
                    ClassName = className,
                    Number = number,
                    SchoolId = schoolId,
                    GradeId = gradeId,
                    SemesterId = semesterId,
                    Status = 1
                };

                if (dựKiếnNgoạiLệ)
                {
                    // Expecting an exception
                    var exception = await Assert.ThrowsAsync<Exception>(async () => await classDAO.UpdateClass(updatedClass));

                    if (schoolId <= 0)
                    {
                        Assert.Contains("Invalid School. The School does not exist.", exception.Message);
                    }
                    else if (gradeId <= 0)
                    {
                        Assert.Contains("Invalid Grade. The Grade does not exist.", exception.Message);
                    }
                    else if (semesterId <= 0)
                    {
                        Assert.Contains("Invalid Semester. The Semester does not exist.", exception.Message);
                    }
                    else if (number <= 0)
                    {
                        Assert.Contains("Không đủ học sinh chưa có lớp", exception.Message);
                    }
                    else
                    {
                        Assert.Contains("A class with the same GradeId, SemesterId, SchoolId, and ClassName already exists in the database.", exception.Message);
                    }
                }
                else
                {
                    // Successful case
                    await classDAO.UpdateClass(updatedClass);
                    var updatedClassFromDb = await context.Classes.FirstOrDefaultAsync(c => c.ClassId == classId);
                    Assert.NotNull(updatedClassFromDb);
                    Assert.Equal(className, updatedClassFromDb.ClassName);
                    Assert.Equal(number, updatedClassFromDb.Number);
                    Assert.Equal(schoolId, updatedClassFromDb.SchoolId);
                    Assert.Equal(gradeId, updatedClassFromDb.GradeId);
                    Assert.Equal(semesterId, updatedClassFromDb.SemesterId);
                }
            }
        }



}
}
