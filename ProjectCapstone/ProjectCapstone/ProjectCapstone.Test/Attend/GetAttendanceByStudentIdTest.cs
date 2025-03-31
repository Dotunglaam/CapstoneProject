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

namespace ProjectCapstone.Test.Attend
{
    public class GetAttendanceByStudentIdTest
    {
        private readonly IMapper _mapper;
        private readonly DbContextOptions<kmsContext> _dbOptions;

        public GetAttendanceByStudentIdTest()
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
        [InlineData(101, "Checkin", "2024-11-11", false)] // Attendance khớp với studentId và type "Checkin"
        [InlineData(101, "Checkout", "2024-12-01", false)] // Attendance không khớp với type "Checkout"
        [InlineData(101, "Checkin", "2024-12-02", false)] // Attendance không khớp với ngày
        [InlineData(999, "Checkin", "2024-12-01", false)] // StudentId không tồn tại
        [InlineData(101, "InvalidType", "2024-12-01", false)] // Type không hợp lệ
        public async Task GetAttendanceByStudentId_ShouldReturnExpectedResult(
    int studentId, string inputType, string inputStartDate, bool expectedResult)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                // Clear database before each test to avoid ID conflict
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Arrange
                var startOfWeek = DateTime.Parse(inputStartDate).Date;

                // Khởi tạo danh sách Attendance với các AttendanceId duy nhất
                var attendances = new List<Attendance>
        {
            new Attendance { AttendanceId = 1, Type = "Checkin", CreatedAt = startOfWeek, ClassId = 1 },
            new Attendance { AttendanceId = 2, Type = "Checkout", CreatedAt = startOfWeek.AddDays(6), ClassId = 1 }
        };

                // Khởi tạo danh sách AttendanceDetails với các AttendanceDetailId duy nhất
                var attendanceDetails = new List<AttendanceDetail>
        {
            new AttendanceDetail { AttendanceDetailId = 1, AttendanceId = 1, StudentId = 101, CreatedAt = startOfWeek, Status = "Present" },
            new AttendanceDetail { AttendanceDetailId = 2, AttendanceId = 2, StudentId = 101, CreatedAt = startOfWeek.AddDays(6), Status = "Absent" }
        };

                context.Attendances.AddRange(attendances);
                context.AttendanceDetails.AddRange(attendanceDetails);
                await context.SaveChangesAsync();

                var dao = new AttendanceDAO(context, _mapper);

                // Act
                var result = await dao.GetAttendanceByStudentId(studentId, inputType, startOfWeek);

                // Assert
                var expectedAttendance = result.Any(r => r.Type == inputType &&
                                                          r.AttendanceDetail.Any(ad => ad.StudentId == studentId &&
                                                                                         ad.CreatedAt.Date == startOfWeek));
                Assert.Equal(expectedResult, expectedAttendance);
            }
        }


    }
}
