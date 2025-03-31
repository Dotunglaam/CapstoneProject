using Moq;
using Xunit;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DataAccess.DAO;
using BusinessObject.DTOS;
using BusinessObject.Models;
using CloudinaryDotNet;

namespace ProjectCapstone.Test.Attend;

public class UpdateAttendanceTest
{
    private readonly IMapper _mapper;
    private readonly DbContextOptions<kmsContext> _dbOptions;

    public UpdateAttendanceTest()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<AttendanceDetail, AttendanceDetailMapper>();
            cfg.CreateMap<Attendance, AttendanceMapper>();
        });
        _mapper = mapperConfig.CreateMapper();

        _dbOptions = new DbContextOptionsBuilder<kmsContext>()
                        .UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}")
                        .Options;
    }

    [Theory]
    [InlineData(false)] // Cập nhật không thành công
    public async Task UpdateAttendance_ShouldReturnExpectedResult(bool expectedResult)
    {
        using (var context = new kmsContext(_dbOptions))
        {
            // Clear database before each test to avoid ID conflict
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Arrange
            var currentDate = DateTime.Now.Date;

            // Tạo dữ liệu cho Attendance với ID duy nhất
            var attendances = new List<Attendance>
        {
            new Attendance
            {
                AttendanceId = 1, // ID duy nhất cho Attendance
                Type = "Checkin",
                CreatedAt = currentDate,
                ClassId = 1
            }
        };

            // Tạo dữ liệu cho AttendanceDetail với ID duy nhất
            var attendanceDetails = new List<AttendanceDetail>
        {
            new AttendanceDetail
            {
                AttendanceDetailId = 1, // ID duy nhất cho AttendanceDetail
                AttendanceId = 1,
                StudentId = 101,
                CreatedAt = currentDate,
                Status = "Absent"  // Trạng thái ban đầu là "Absent"
            }
        };

            // Thêm dữ liệu vào cơ sở dữ liệu
            context.Attendances.AddRange(attendances);
            context.AttendanceDetails.AddRange(attendanceDetails);
            await context.SaveChangesAsync();

            // Dữ liệu mapper
            var attendanceMappers = new List<AttendanceMapper>
        {
            new AttendanceMapper
            {
                AttendanceDetail = _mapper.Map<List<AttendanceDetailMapper>>(attendanceDetails)
            }
        };

            var dao = new AttendanceDAO(context, _mapper);

            // Act
            // Cung cấp đầy đủ ba tham số: attendanceMappers, ClassId, Type
            await dao.UpdateAttendance(attendanceMappers, 1, "Checkin");

            // Assert
            var updatedDetail = context.AttendanceDetails.FirstOrDefault(ad => ad.AttendanceDetailId == 1);

            // In ra trạng thái đã cập nhật để kiểm tra
            Console.WriteLine("Updated Status: " + updatedDetail?.Status);

            // Kiểm tra xem trạng thái có thay đổi thành "Present" không
            var isUpdated = updatedDetail?.Status == "Present";

            Assert.Equal(expectedResult, isUpdated);
        }
    }




    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task UpdateAttendance_ShouldThrowArgumentException_WhenTypeIsNullOrEmpty(string inputType)
    {
        using (var context = new kmsContext(_dbOptions))
        {
            // Clear database before each test to avoid ID conflict
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Arrange
            var attendanceMappers = new List<AttendanceMapper>
            {
                new AttendanceMapper
                {
                    AttendanceDetail = new List<AttendanceDetailMapper>
                    {
                        new AttendanceDetailMapper { AttendanceDetailId = 1, Status = "Present" }
                    }
                }
            };

            var dao = new AttendanceDAO(context, _mapper);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => dao.UpdateAttendance(attendanceMappers, 1, inputType));
        }
    }

    [Fact]
    public async Task UpdateAttendance_ShouldThrowException_WhenNoAttendanceDetailsFound()
    {
        using (var context = new kmsContext(_dbOptions))
        {
            // Clear database before each test to avoid ID conflict
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Arrange: Giả lập không có AttendanceDetails
            var attendanceMappers = new List<AttendanceMapper>
        {
            new AttendanceMapper
            {
                AttendanceDetail = new List<AttendanceDetailMapper>
                {
                    new AttendanceDetailMapper { AttendanceDetailId = 1, Status = "Present" }
                }
            }
        };

            var dao = new AttendanceDAO(context, _mapper);

            // Act & Assert: Kiểm tra xem Exception có được ném ra không khi không có AttendanceDetails
            var exception = await Assert.ThrowsAsync<Exception>(
                () => dao.UpdateAttendance(attendanceMappers, 1, "Checkin"));

            Assert.Contains("Error updating attendance details: No attendance details found for the given ClassId and Type.", exception.Message); // Kiểm tra thông điệp của ngoại lệ
        }
    }



    [Fact]
    public async Task UpdateAttendance_ShouldThrowSystemException_WhenNoStatusIsProvided()
    {
        using (var context = new kmsContext(_dbOptions))
        {
            // Clear database before each test to avoid ID conflict
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Arrange: Set up attendance details with Status là một chuỗi rỗng
            var attendanceMappers = new List<AttendanceMapper>
        {
            new AttendanceMapper
            {
                AttendanceDetail = new List<AttendanceDetailMapper>
                {
                    new AttendanceDetailMapper { AttendanceDetailId = 1, Status = "" } // Status là chuỗi rỗng
                }
            }
        };

            var attendances = new List<Attendance>
        {
            new Attendance
            {
                AttendanceId = 1,
                Type = "Checkin",
                CreatedAt = DateTime.Now,
                ClassId = 1
            }
        };

            var attendanceDetails = new List<AttendanceDetail>
        {
            new AttendanceDetail
            {
                AttendanceDetailId = 1,
                AttendanceId = 1,
                StudentId = 101,
                CreatedAt = DateTime.Now,
                Status = "Absent"
            }
        };

            context.Attendances.AddRange(attendances);
            context.AttendanceDetails.AddRange(attendanceDetails);
            context.SaveChanges();

            var dao = new AttendanceDAO(context, _mapper);

            // Act & Assert: Kiểm tra xem Exception có được ném ra khi Status là chuỗi rỗng hay không
            var exception = await Assert.ThrowsAsync<Exception>(
                () => dao.UpdateAttendance(attendanceMappers, 1, "Checkin"));

            Assert.Contains("Error updating attendance details: Invalid status", exception.Message); // Kiểm tra thông điệp của ngoại lệ
        }
    }


}
