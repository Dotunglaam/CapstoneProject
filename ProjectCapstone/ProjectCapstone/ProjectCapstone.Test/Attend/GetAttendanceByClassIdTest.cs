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

public class GetAttendanceByClassIdTest
{
    private readonly IMapper _mapper;
    private readonly DbContextOptions<kmsContext> _dbOptions;

    public GetAttendanceByClassIdTest()
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

    [Theory]
    [InlineData(1, "Checkin", true)]  // Attendance khớp với ClassId và Type = "Checkin"
    [InlineData(2, "Checkin", true)]  // Attendance không khớp với ClassId
    [InlineData(1, "Checkout", true)] // Attendance không khớp với Type = "Checkout"
    [InlineData(1, "Checkin", true)]  // Attendance khớp với ClassId và Type = "Checkin"

    // Trường hợp bất thường:
    [InlineData(1, "InvalidType", false)]  // Type không hợp lệ
    [InlineData(-1, "Checkin", false)]    // ClassId không hợp lệ
    [InlineData(1, null, false)]          // Type null
    public async Task GetAttendanceByClassId_ShouldReturnExpectedResult(
    int inputClassId, string inputType, bool expectedResult)
    {
        using (var context = new kmsContext(_dbOptions))
        {
            // Clear database before each test to avoid ID conflict
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Arrange
            var currentDate = DateTime.Now.Date;

            // Sửa đổi các AttendanceId và AttendanceDetailId để tránh xung đột
            var attendances = new List<Attendance>
        {
            new Attendance
            {
                AttendanceId = 1, // Đảm bảo ID duy nhất cho mỗi bản ghi
                Type = "Checkin",
                CreatedAt = currentDate,
                ClassId = 1
            },
            new Attendance
            {
                AttendanceId = 2,
                Type = "Checkin",
                CreatedAt = currentDate,
                ClassId = 2
            },
            new Attendance
            {
                AttendanceId = 3,
                Type = "Checkout",
                CreatedAt = currentDate,
                ClassId = 1
            }
        };

            var attendanceDetails = new List<AttendanceDetail>
        {
            new AttendanceDetail
            {
                AttendanceDetailId = 1,  // Đảm bảo AttendanceDetailId duy nhất
                AttendanceId = 1, // Đảm bảo không trùng với AttendanceId
                StudentId = 101,
                CreatedAt = currentDate,
                Status = "Present"
            }
        };

            context.Attendances.AddRange(attendances);
            context.AttendanceDetails.AddRange(attendanceDetails);
            await context.SaveChangesAsync(); // Đảm bảo dùng SaveChangesAsync để lưu các thay đổi

            var dao = new AttendanceDAO(context, _mapper);

            // Act
            var result = await dao.GetAttendanceByClassId(inputClassId, inputType);

            // Assert
            var expectedAttendance = result.Any(r => r.ClassId == inputClassId && r.Type == inputType);
            Assert.Equal(expectedResult, expectedAttendance);
        }
    }


}
