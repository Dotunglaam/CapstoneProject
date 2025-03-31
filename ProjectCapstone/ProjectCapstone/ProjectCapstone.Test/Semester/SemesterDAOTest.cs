using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ProjectCapstone.Test.Semesters
{
    public class SemesterDAOTest
    {
        private readonly IMapper _mapper;
        private readonly DbContextOptions<kmsContext> _dbOptions;

        public SemesterDAOTest()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Semester, SemesterMapper>().ReverseMap();
            });
            _mapper = mapperConfig.CreateMapper();

            _dbOptions = new DbContextOptionsBuilder<kmsContext>()
                .UseInMemoryDatabase("TestDatabase")
                .EnableSensitiveDataLogging()
                .Options;
        }

        // Seed data
        private async Task SeedData(kmsContext context, bool existingData = false)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            if (existingData)
            {
                context.Semesters.AddRange(
                    new Semester
                    {
                        SemesterId = 1,
                        Name = "Học Kỳ 1",
                        StartDate = DateTime.Parse("2025-01-01"),
                        EndDate = DateTime.Parse("2025-05-31"),
                        Status = 1
                    },
                    new Semester
                    {
                        SemesterId = 2,
                        Name = "Học Kỳ 2",
                        StartDate = DateTime.Parse("2025-06-01"),
                        EndDate = DateTime.Parse("2025-12-31"),
                        Status = 1
                    }
                );
                await context.SaveChangesAsync();
            }
        }

        [Theory]
        [InlineData("Học Kỳ 3", "2026-01-01", "2026-05-31", false, false)] 
        [InlineData("Học Kỳ 4", "2025-03-01", "2025-06-15", true, true)]  
        [InlineData("Học Kỳ 5", "2026-06-01", "2026-06-01", false, true)] 
        [InlineData("Học Kỳ 6", null, "2026-06-30", false, true)]         
        [InlineData("Học Kỳ 7", "2026-06-01", null, false, true)]         
        public async Task AddSemester_ShouldHandleVariousScenarios(string name, string startDateStr, string endDateStr, bool seedData, bool expectException)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                if (seedData)
                {
                    await SeedData(context, true);
                }

                var dao = new SemesterDAO(context, _mapper);

                var semesterMapper = new SemesterMapper
                {
                    Name = name,
                    StartDate = startDateStr != null ? DateTime.Parse(startDateStr) : null,
                    EndDate = endDateStr != null ? DateTime.Parse(endDateStr) : null,
                };

                if (expectException)
                {
                    var exception = await Assert.ThrowsAsync<Exception>(async () =>
                        await dao.AddSemester(semesterMapper));

                    Assert.NotNull(exception);
                    if (startDateStr == null || endDateStr == null)
                    {
                        Assert.Contains("Start date and end date cannot be null.", exception.Message);
                    }
                    else if (DateTime.Parse(startDateStr) >= DateTime.Parse(endDateStr))
                    {
                        Assert.Contains("Start date must be before end date.", exception.Message);
                    }
                    else
                    {
                        Assert.Contains("overlap with an existing semester", exception.Message);
                    }
                }
                else
                {
                    var newSemesterId = await dao.AddSemester(semesterMapper);

                    var addedSemester = await context.Semesters.FirstOrDefaultAsync(s => s.SemesterId == newSemesterId);
                    Assert.NotNull(addedSemester);
                    Assert.Equal(name, addedSemester.Name);
                    Assert.Equal(DateTime.Parse(startDateStr), addedSemester.StartDate);
                    Assert.Equal(DateTime.Parse(endDateStr), addedSemester.EndDate);
                }
            }
        }
        [Theory]
        [InlineData(1, "Updated Semester", "2025-06-01", "2025-12-31", true, false)] // Cập nhật thành công
        [InlineData(999, "Non-existent Semester", "2025-06-01", "2025-12-31", false, true)] // Kỳ học không tồn tại
        [InlineData(1, "Invalid Date Range", "2025-06-01", "2025-05-01", true, true)] // Ngày bắt đầu lớn hơn ngày kết thúc
        [InlineData(1, "Overlapping Semester", "2025-06-01", "2025-08-31", true, true)] // Trùng lặp với kỳ học khác
        public async Task UpdateSemester_ShouldHandleVariousScenarios(
            int semesterId,
            string newName,
            string startDate,
            string endDate,
            bool seedData,
            bool expectException)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                // Sắp xếp: Seed dữ liệu nếu cần
                if (seedData)
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    // Tạo kỳ học hiện tại
                    var existingSemester = new Semester
                    {
                        SemesterId = 1,
                        Name = "Existing Semester",
                        StartDate = DateTime.Parse("2025-01-01"),
                        EndDate = DateTime.Parse("2025-05-31"),
                        Status = 1
                    };
                    context.Semesters.Add(existingSemester);

                    // Tạo kỳ học trùng lặp nếu cần
                    if (expectException && semesterId == 1 && newName == "Overlapping Semester")
                    {
                        var overlappingSemester = new Semester
                        {
                            SemesterId = 2,
                            Name = "Overlapping Semester",
                            StartDate = DateTime.Parse("2025-06-01"),
                            EndDate = DateTime.Parse("2025-08-31"),
                            Status = 1
                        };
                        context.Semesters.Add(overlappingSemester);
                    }

                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                }

                var dao = new SemesterDAO(context, _mapper);

                // Thực hiện
                var semesterMapper = new SemesterMapper
                {
                    SemesterId = semesterId,
                    Name = newName,
                    StartDate = DateTime.Parse(startDate),
                    EndDate = DateTime.Parse(endDate),
                    Status = 1
                };

                if (expectException)
                {
                    // Kiểm tra ngoại lệ
                    var exception = await Assert.ThrowsAsync<Exception>(async () =>
                        await dao.UpdateSemester(semesterMapper));
                    Assert.NotNull(exception); // Đảm bảo exception không null

                    // Kiểm tra thông báo ngoại lệ
                    if (semesterId == 999)
                    {
                        Assert.Contains("Semester not found", exception.Message);
                    }
                    else if (startDate.CompareTo(endDate) >= 0)
                    {
                        Assert.Contains("Start date must be before end date", exception.Message);
                    }
                    else if (newName == "Overlapping Semester")
                    {
                        Assert.Contains("overlap with an existing semester", exception.Message);
                    }
                }
                else
                {
                    // Kiểm tra cập nhật thành công
                    await dao.UpdateSemester(semesterMapper);

                    var updatedSemester = await context.Semesters.FindAsync(semesterId);
                    Assert.NotNull(updatedSemester); // Kỳ học đã được cập nhật
                    Assert.Equal(newName, updatedSemester.Name); // Kiểm tra tên mới
                    Assert.Equal(DateTime.Parse(startDate), updatedSemester.StartDate); // Kiểm tra ngày bắt đầu mới
                    Assert.Equal(DateTime.Parse(endDate), updatedSemester.EndDate); // Kiểm tra ngày kết thúc mới
                }
            }
        }

    }
}
