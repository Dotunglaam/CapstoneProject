using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCapstone.Test.Attend;

public class CreateDailyCheckoutTest
{
    private readonly IMapper _mapper;
    private readonly DbContextOptions<kmsContext> _dbOptions;

    public CreateDailyCheckoutTest()
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
    public async Task CreateDailyCheckout_ShouldThrowException_WhenNoActiveClasses()
    {
        // Arrange: Mock dữ liệu khi không có lớp học nào hoạt động
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var dao = new AttendanceDAO(context, _mapper);

            // Act & Assert: Kiểm tra xem ngoại lệ có được ném ra không khi không có lớp học hoạt động
            var exception = await Assert.ThrowsAsync<Exception>(
                () => dao.CreateDailyCheckout());

            // Kiểm tra thông điệp ngoại lệ
            Assert.Contains("No active classes available for check-out", exception.Message);
        }
    }
    [Fact]
    public async Task CreateDailyCheckout_ShouldThrowException_WhenAlreadyCheckedOut()
    {
        // Arrange: Mock dữ liệu khi đã có check-out cho ngày hôm nay
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
                        Code = "S123",
                        ParentId = 1
                    }
                }
            }
            };
            context.Classes.Add(classItem);
            await context.SaveChangesAsync();

            // Giả sử đã có check-out cho ngày hôm nay
            context.Attendances.Add(new Attendance
            {
                Type = "Checkout",
                CreatedAt = DateTime.Now.Date,
                ClassId = 1
            });
            await context.SaveChangesAsync();

            var dao = new AttendanceDAO(context, _mapper);

            // Act & Assert: Kiểm tra xem ngoại lệ có được ném ra không khi đã có check-out
            var exception = await Assert.ThrowsAsync<Exception>(
                () => dao.CreateDailyCheckout());

            // Kiểm tra thông điệp ngoại lệ
            Assert.Contains("Already checked out", exception.Message);
        }
    }
    [Fact]
    public async Task CreateDailyCheckout_ShouldThrowException_WhenClassHasNoStudents()
    {
        // Arrange: Mock dữ liệu khi một lớp học không có học sinh
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var classItem = new Class
            {
                ClassId = 1,
                IsActive = 1,
                ClassHasChildren = new List<ClassHasChild>() // Không có học sinh
            };
            context.Classes.Add(classItem);
            await context.SaveChangesAsync();

            var dao = new AttendanceDAO(context, _mapper);

            // Act & Assert: Kiểm tra xem ngoại lệ có được ném ra không khi lớp học không có học sinh
            var exception = await Assert.ThrowsAsync<Exception>(
                () => dao.CreateDailyCheckout());

            // Kiểm tra thông điệp ngoại lệ
            Assert.Contains("Class 1 has no students for check-out", exception.Message);
        }
    }
    [Fact]
    public async Task CreateDailyCheckout_ShouldCreateAttendance_WhenValidDataProvided()
    {
        // Arrange: Mock dữ liệu hợp lệ cho lớp học và học sinh
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
                        Code = "S123",
                        ParentId = 1
                    }
                }
            }
            };
            context.Classes.Add(classItem);
            await context.SaveChangesAsync();

            var dao = new AttendanceDAO(context, _mapper);

            // Act: Gọi phương thức CreateDailyCheckout
            await dao.CreateDailyCheckout();

            // Assert: Kiểm tra xem Attendance và AttendanceDetail có được tạo không
            var attendance = await context.Attendances
                .FirstOrDefaultAsync(a => a.ClassId == 1 && a.Type == "Checkout");
            Assert.NotNull(attendance);

            var attendanceDetails = await context.AttendanceDetails
                .Where(ad => ad.AttendanceId == attendance.AttendanceId)
                .ToListAsync();
            Assert.NotEmpty(attendanceDetails);
        }
    }

}
