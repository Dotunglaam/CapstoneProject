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
using Xunit;

namespace ProjectCapstone.Test.Attend
{
    public class GetAttendanceByTypeAndDateTest
    {
        private readonly IMapper _mapper;
        private readonly DbContextOptions<kmsContext> _dbOptions;

        public GetAttendanceByTypeAndDateTest()
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
        [InlineData("Checkin", "2024-12-01", true)]   // Kiểm tra khi có Attendance cho Type "Checkin" vào ngày 2024-12-01
        [InlineData("Checkout", "2024-12-01", true)]  // Kiểm tra khi có Attendance cho Type "Checkout" vào ngày 2024-12-01
        [InlineData("Checkin", "2024-12-02", true)]  // Không có Attendance cho ngày 2024-12-02
        [InlineData("InvalidType", "2024-12-01", false)] // Type không hợp lệ
        [InlineData("Checkin", "2024-12-03", false)]   // Không có Attendance cho ngày 2024-12-03
        [InlineData("Checkout", "2024-12-03", false)]
        public async Task GetAttendanceByTypeAndDate_ShouldReturnExpectedResult(
    string inputType, string inputDate, bool expectedResult)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                // Clear database before each test to avoid ID conflict
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Arrange
                var currentDate = DateTime.Parse(inputDate).Date;

                // Tạo dữ liệu Attendances cho các ngày cần thiết
                var attendances = new List<Attendance>();

                if (inputDate == "2024-12-01" || inputDate == "2024-12-02")
                {
                    attendances.Add(new Attendance
                    {
                        AttendanceId = 1, // ID duy nhất cho Attendance
                        Type = "Checkin",
                        CreatedAt = currentDate,
                        ClassId = 1
                    });
                    attendances.Add(new Attendance
                    {
                        AttendanceId = 2, // ID duy nhất cho Attendance
                        Type = "Checkout",
                        CreatedAt = currentDate,
                        ClassId = 1
                    });
                }

                if (inputDate == "2024-12-01") // Nếu là ngày 2024-12-01, thêm một Attendance vào ngày hôm sau
                {
                    attendances.Add(new Attendance
                    {
                        AttendanceId = 3, // ID duy nhất cho Attendance
                        Type = "Checkin",
                        CreatedAt = currentDate.AddDays(1),
                        ClassId = 1
                    });
                }

                // Tạo dữ liệu AttendanceDetails
                var attendanceDetails = new List<AttendanceDetail>
        {
            new AttendanceDetail
            {
                AttendanceDetailId = 1, // ID duy nhất cho AttendanceDetail
                AttendanceId = 1,
                StudentId = 101,
                CreatedAt = currentDate,
                Status = "Present"
            }
        };

                // Thêm dữ liệu vào cơ sở dữ liệu
                context.Attendances.AddRange(attendances);
                context.AttendanceDetails.AddRange(attendanceDetails);
                await context.SaveChangesAsync();

                var dao = new AttendanceDAO(context, _mapper);

                // Act
                var result = await dao.GetAttendanceByTypeAndDate(inputType, currentDate);

                // Assert
                if (expectedResult)
                {
                    // Kiểm tra nếu có dữ liệu trả về thỏa mãn điều kiện
                    var expectedAttendance = result.Any(r => r.Type == inputType && r.CreatedAt.Date == currentDate);
                    Assert.True(expectedAttendance);
                }
                else
                {
                    // Kiểm tra nếu danh sách kết quả trả về là rỗng khi không có dữ liệu thỏa mãn
                    Assert.Empty(result);
                }
            }
        }

    }
}
