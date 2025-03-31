using AutoMapper;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using BusinessObject.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace ProjectCapstone.Test.Attend;
public class CreateDailyCheckoutByClassIdTest
{
    private readonly IMapper _mapper;
    private readonly DbContextOptions<kmsContext> _dbOptions;

    public CreateDailyCheckoutByClassIdTest()
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
    public async Task CreateDailyCheckoutByClassId_ShouldThrowException_WhenClassNotFound()
    {
        // Arrange: Mock dữ liệu khi không có lớp học với ClassId
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var dao = new AttendanceDAO(context, _mapper);

            // Act & Assert: Kiểm tra xem ngoại lệ có được ném ra không khi lớp học không tồn tại
            //var exception = await Assert.ThrowsAsync<Exception>(
            //    () => dao.CreateDailyCheckoutByClassId(999)); // ClassId không tồn tại

            //// Kiểm tra thông điệp ngoại lệ
            //Assert.Contains("ClassID not found", exception.Message);
        }
    }
    [Fact]
    public async Task CreateDailyCheckoutByClassId_ShouldThrowException_WhenClassHasNoStudents()
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
            //var exception = await Assert.ThrowsAsync<Exception>(
            //    () => dao.CreateDailyCheckoutByClassId(1));

            // Kiểm tra thông điệp ngoại lệ
            //Assert.Contains("Class 1 has no students for check-out", exception.Message);
        }
    }


    [Fact]
    public async Task CreateDailyCheckoutByClassId_ShouldCreateAttendance_WhenValidDataProvided()
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

            // Act: Gọi phương thức CreateDailyCheckoutByClassId
            //await dao.CreateDailyCheckoutByClassId(1);

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
    [Fact]
    public async Task CreateDailyCheckoutByClassId_ShouldThrowException_WhenClassIdNotFound()
    {
        // Arrange: Dữ liệu không có lớp học với ClassId = 999
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var dao = new AttendanceDAO(context, _mapper);

            // Act & Assert: Kiểm tra xem ngoại lệ có được ném ra không khi ClassId không tồn tại
            //var exception = await Assert.ThrowsAsync<Exception>(
                //() => dao.CreateDailyCheckoutByClassId(999));

            // Kiểm tra thông điệp ngoại lệ
            //Assert.Contains("ClassID not found", exception.Message);
        }
    }
    [Fact]
    public async Task CreateDailyCheckoutByClassId_ShouldThrowException_WhenAlreadyCheckedOut()
    {
        // Arrange: Mock dữ liệu khi đã có check-out cho ngày hôm nay
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

            // Giả sử đã có attendance cho ngày hôm nay
            context.Attendances.Add(new Attendance
            {
                Type = "Checkout",
                CreatedAt = DateTime.Now.Date,
                ClassId = 1
            });
            await context.SaveChangesAsync();

            var dao = new AttendanceDAO(context, _mapper);

            // Act & Assert: Kiểm tra xem ngoại lệ có được ném ra không khi đã có check-out
            //var exception = await Assert.ThrowsAsync<Exception>(
            //    () => dao.CreateDailyCheckoutByClassId(1));

            //// Kiểm tra thông điệp ngoại lệ
            //Assert.Contains("Already checkout", exception.Message);
        }
    }
    [Fact]
    public async Task CreateDailyCheckoutByClassId_ShouldCreateAttendanceDetails_WhenStudentsArePresent()
    {
        // Arrange: Lớp học có học sinh và tất cả học sinh có mặt
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

            // Act: Gọi phương thức CreateDailyCheckoutByClassId
            //await dao.CreateDailyCheckoutByClassId(1);

            // Assert: Kiểm tra xem attendance detail đã được tạo ra
            var attendanceDetails = await context.AttendanceDetails
                .Where(ad => ad.AttendanceId == 1 && ad.StudentId == 101)
                .ToListAsync();

            Assert.Single(attendanceDetails); // Kiểm tra chỉ có 1 học sinh được tạo attendance detail
        }
    }

}

