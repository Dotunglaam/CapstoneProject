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

namespace ProjectCapstone.Test.Attend
{
    public class GetAttendanceByDateTest
    {
        private readonly IMapper _mapper;
        private readonly DbContextOptions<kmsContext> _dbOptions;

        public GetAttendanceByDateTest()
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
        [InlineData(1, "Checkin", "2024-12-01", true)]  // Kiểm tra nếu Attendance tồn tại với ClassId, Type và Date chính xác
        [InlineData(2, "Checkin", "2024-12-01", false)] // Attendance không khớp với ClassId
        [InlineData(1, "Checkout", "2024-12-01", true)] // Attendance tồn tại với Type "Checkout" và Date "2024-12-01"
        [InlineData(1, "Checkin", "2024-12-02", true)] // Không có Attendance cho ngày này

        // Trường hợp bất thường:
        [InlineData(1, "InvalidType", "2024-12-01", false)]  // Type không hợp lệ
        [InlineData(-1, "Checkin", "2024-12-01", false)]    // ClassId không hợp lệ
        public async Task GetAttendanceByDate_ShouldReturnExpectedResult(
    int inputClassId, string inputType, string inputDate, bool expectedResult)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                // Clear database before each test to avoid ID conflict
                context.Database.EnsureDeleted();  // Đảm bảo dữ liệu cũ được xóa
                context.Database.EnsureCreated();  // Tạo lại cơ sở dữ liệu mới

                // Arrange
                var currentDate = DateTime.Parse(inputDate).Date;

                // Tạo dữ liệu Attendances cho bài kiểm tra
                var attendances = new List<Attendance>
        {
            new Attendance
            {
                AttendanceId = 1,
                Type = "Checkin",
                CreatedAt = currentDate,
                ClassId = 1
            },
            new Attendance
            {
                AttendanceId = 2,
                Type = "Checkout",
                CreatedAt = currentDate,
                ClassId = 1
            },
            new Attendance
            {
                AttendanceId = 3,
                Type = "Checkin",
                CreatedAt = currentDate.AddDays(1),
                ClassId = 1
            }
        };

                var attendanceDetails = new List<AttendanceDetail>
        {
            new AttendanceDetail
            {
                AttendanceDetailId = 1,  // Chắc chắn ID duy nhất cho AttendanceDetail
                AttendanceId = 1,
                StudentId = 101,
                CreatedAt = currentDate,
                Status = "Present"
            }
        };

                context.Attendances.AddRange(attendances);  // Thêm dữ liệu Attendances vào cơ sở dữ liệu
                context.AttendanceDetails.AddRange(attendanceDetails);  // Thêm dữ liệu AttendanceDetails
                await context.SaveChangesAsync();  // Đảm bảo sử dụng SaveChangesAsync để lưu thay đổi

                var dao = new AttendanceDAO(context, _mapper);

                // Act
                var result = await dao.GetAttendanceByDate(inputClassId, inputType, currentDate);

                // Assert
                var expectedAttendance = result.Any(r => r.ClassId == inputClassId && r.Type == inputType && r.CreatedAt.Date == currentDate);
                Assert.Equal(expectedResult, expectedAttendance);
            }
        }



    }
}
