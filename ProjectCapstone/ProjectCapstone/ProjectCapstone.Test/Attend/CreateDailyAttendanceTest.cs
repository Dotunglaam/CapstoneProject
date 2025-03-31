using AutoMapper;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.DTOS;
using DataAccess.DAO;

namespace ProjectCapstone.Test.Attend;

public class CreateDailyAttendanceTest
{
    private readonly IMapper _mapper;
    private readonly DbContextOptions<kmsContext> _dbOptions;

    public CreateDailyAttendanceTest()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Attendance, AttendanceMapper>();
            cfg.CreateMap<AttendanceDetail, AttendanceDetailMapper>();
        });
        _mapper = mapperConfig.CreateMapper();

        _dbOptions = new DbContextOptionsBuilder<kmsContext>()
                        .UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}")
                        .Options;
    }
    [Fact]
    public async Task CreateDailyAttendance_ShouldThrowException_WhenNoActiveClassesAvailable()
    {
        // Arrange: Mock dữ liệu khi không có lớp học hoạt động
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var dao = new AttendanceDAO(context, _mapper);

            // Act & Assert: Kiểm tra xem ngoại lệ có được ném ra không khi không có lớp học hoạt động
            var exception = await Assert.ThrowsAsync<Exception>(
                () => dao.CreateDailyAttendance());

            Assert.Contains("No active classes available for attend", exception.Message);
        }
    }

    [Fact]
    public async Task CreateDailyAttendance_ShouldThrowException_WhenAlreadyCheckedIn()
    {
        // Arrange: Mock dữ liệu khi đã có check-in cho ngày hôm nay
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Thêm lớp học và học sinh để có lớp học hoạt động
            var classItem = new Class
            {
                ClassId = 1,
                IsActive = 1,
                ClassHasChildren = new List<ClassHasChild>
            {
                new ClassHasChild
                {
                    Student = new Child
                    {
                        StudentId = 101,
                        Code = "S123",  // Cung cấp giá trị cho Code
                        ParentId = 1
                    }
                }
            }
            };
            context.Classes.Add(classItem);
            await context.SaveChangesAsync();

            // Giả sử đã có attendance cho ngày hôm nay
            context.Attendances.Add(new Attendance
            {
                Type = "Attend",
                CreatedAt = DateTime.Now.Date,
                ClassId = 1
            });
            await context.SaveChangesAsync();

            var dao = new AttendanceDAO(context, _mapper);

            // Act & Assert: Kiểm tra xem ngoại lệ có được ném ra không khi đã có check-in
            var exception = await Assert.ThrowsAsync<Exception>(
                () => dao.CreateDailyAttendance());

            // Kiểm tra thông điệp ngoại lệ
            Assert.Contains("Already Attended", exception.Message); // Cập nhật thông điệp ngoại lệ phù hợp
        }
    }

    [Fact]
    public async Task CreateDailyAttendance_ShouldThrowException_WhenClassHasNoStudents()
    {
        // Arrange: Mock dữ liệu khi một lớp học không có học sinh
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Classes.Add(new Class
            {
                ClassId = 1,
                IsActive = 1,
                ClassHasChildren = new List<ClassHasChild>() // Không có học sinh
            });
            await context.SaveChangesAsync();

            var dao = new AttendanceDAO(context, _mapper);

            // Act & Assert: Kiểm tra xem ngoại lệ có được ném ra không khi lớp học không có học sinh
            var exception = await Assert.ThrowsAsync<Exception>(
                () => dao.CreateDailyAttendance());

            // Chỉnh lại kiểm tra thông điệp ngoại lệ để khớp với thông điệp thực tế
            Assert.Contains("Class 1 has no students for Attend", exception.Message);
        }
    }


    [Fact]
    public async Task CreateDailyAttendance_ShouldCreateAttendance_WhenValidDataProvided()
    {
        // Arrange: Mock dữ liệu khi có lớp học và học sinh
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var classItem = new Class
            {
                ClassId = 1,
                IsActive = 1,
                ClassHasChildren = new List<ClassHasChild>
            {
                new ClassHasChild
                {
                    Student = new Child
                    {
                        StudentId = 101,
                        Code = "S123",  // Cung cấp giá trị cho Code
                        ParentId = 1,  // Cung cấp thông tin còn thiếu nếu cần
                    }
                }
            }
            };
            context.Classes.Add(classItem);
            await context.SaveChangesAsync();

            var dao = new AttendanceDAO(context, _mapper);

            // Act: Gọi phương thức tạo check-in
            await dao.CreateDailyAttendance();

            // Assert: Kiểm tra xem attendance và attendance details có được tạo thành công không
            var attendance = await context.Attendances
                .FirstOrDefaultAsync(a => a.Type == "Attend" && a.CreatedAt.Date == DateTime.Now.Date);
            Assert.NotNull(attendance);
            Assert.Equal("Attend", attendance.Type);

            var attendanceDetails = await context.AttendanceDetails.ToListAsync();
            Assert.NotEmpty(attendanceDetails);
            Assert.All(attendanceDetails, ad => Assert.Equal("Absent", ad.Status)); // Kiểm tra trạng thái là "Absent"
        }
    }


}

