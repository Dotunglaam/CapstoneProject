using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ProjectCapstone.Test.Schedules
{
    public class ScheduleDAOTest
    {
        private readonly IMapper _mapper;
        private readonly DbContextOptions<kmsContext> _dbOptions;

        public ScheduleDAOTest()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                // Cấu hình ánh xạ giữa DTO và Entity
                cfg.CreateMap<Schedule, ScheduleMapper>().ReverseMap();
            });
            _mapper = mapperConfig.CreateMapper();

            _dbOptions = new DbContextOptionsBuilder<kmsContext>()
                        .UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}")
                        .Options;
        }

        private async Task SeedDatabase(kmsContext context, bool exists = false)
        {
            // Xóa sạch database trước khi seed
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            if (exists)
            {
                context.Schedules.AddRange(
                    new Schedule
                    {
                        ScheduleId = 1,
                        ClassId = 1,
                        Status = 1,
                        TeacherName = "Teacher A"
                    },
                    new Schedule
                    {
                        ScheduleId = 2,
                        ClassId = 1,
                        Status = 2,
                        TeacherName = "Teacher B"
                    }
                );
                await context.SaveChangesAsync();
            }
        }

        [Theory]
        [InlineData(1, 1, "Teacher A", false)] // Thêm thành công
        [InlineData(1, 2, "Teacher B", true)]  // Trùng lặp ClassId
        [InlineData(0, 1, "Teacher D", true)]  // ClassId không hợp lệ
        public async Task AddSchedule_ShouldHandleVariousScenarios(
            int classId, int? status, string teacherName, bool shouldThrowException)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                await SeedDatabase(context, classId == 1 && teacherName == "Teacher B");

                var dao = new ScheduleDAO(context, _mapper);

                // Chuẩn bị dữ liệu từ ScheduleMapper
                var newSchedule = classId > 0 ? new ScheduleMapper
                {
                    ClassId = classId,
                    Status = status,
                    TeacherName = teacherName
                } : null;

                // Act
                if (shouldThrowException)
                {
                    var exception = await Assert.ThrowsAsync<Exception>(
                        async () => await dao.AddSchedule(newSchedule)
                    );

                    // So khớp thông báo lỗi
                    if (classId == 1 && teacherName == "Teacher B")
                    {
                        Assert.Contains($"Schedule for ClassId {classId} already exists.", exception.Message);
                    }
                    else if (classId <= 0)
                    {
                        Assert.Contains("New schedule cannot be null", exception.Message);
                    }
                }
                else
                {
                    await dao.AddSchedule(newSchedule);

                    // Assert: Xác nhận thêm thành công
                    var addedSchedule = await context.Schedules.FirstOrDefaultAsync(s => s.ClassId == classId);
                    Assert.NotNull(addedSchedule);
                    Assert.Equal(status, addedSchedule.Status);
                    Assert.Equal(teacherName, addedSchedule.TeacherName);
                }
            }
        }

        [Theory]
        [InlineData(1, 1, "Updated Teacher", true)]  // Thành công
        [InlineData(2, 2, "Non-Existent Teacher", false)] // Lỗi: Schedule không tồn tại
        public async Task UpdateSchedule_ShouldUpdateOrThrowException(int scheduleId, int status, string teacherName, bool exists)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                await SeedDatabase(context, exists);

                var dao = new ScheduleDAO(context, _mapper);

                var updatedSchedule = new ScheduleMapper
                {
                    ScheduleId = scheduleId,
                    ClassId = 1,
                    Status = status,
                    TeacherName = teacherName
                };

                // Act & Assert
                if (exists)
                {
                    await dao.UpdateSchedule(updatedSchedule);

                    // Kiểm tra cập nhật thành công
                    var schedule = await context.Schedules.FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);
                    Assert.NotNull(schedule);
                    Assert.Equal(status, schedule.Status);
                    Assert.Equal(teacherName, schedule.TeacherName);
                }
                else
                {
                    // Kiểm tra ngoại lệ nếu không tồn tại
                    var exception = await Assert.ThrowsAsync<Exception>(
                        async () => await dao.UpdateSchedule(updatedSchedule)
                    );
                    Assert.Contains("Schedule not found", exception.Message);
                }
            }
        }

        [Theory]
        [InlineData(1, 2, true)]  // Tìm thấy 2 schedules
        [InlineData(2, 0, false)] // Không tìm thấy schedule nào
        public async Task GetSchedulesByClassId_ShouldReturnSchedules(int classId, int expectedCount, bool seedData)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                await SeedDatabase(context, seedData);

                var dao = new ScheduleDAO(context, _mapper);

                // Act
                var schedules = await dao.GetSchedulesByClassId(classId);

                // Assert
                Assert.NotNull(schedules);
                Assert.Equal(expectedCount, schedules.Count);

                if (expectedCount > 0)
                {
                    Assert.All(schedules, s => Assert.Equal(classId, s.ClassId));
                }
            }
        }
        [Theory]
        [InlineData(1, false)]  // Tìm thấy schedule
        [InlineData(3, false)] // Không tìm thấy schedule
        public async Task GetScheduleById_ShouldReturnScheduleOrThrowException(int scheduleId, bool exists)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                await SeedDatabase(context, exists);

                if (exists)
                {
                    // Seed thêm dữ liệu nếu cần
                    if (!context.Schedules.Any(s => s.ScheduleId == scheduleId))
                    {
                        context.Schedules.Add(new Schedule
                        {
                            ScheduleId = scheduleId,
                            ClassId = 1,
                            Status = 1,
                            TeacherName = "Teacher A"
                        });
                        await context.SaveChangesAsync();
                    }
                }

                var dao = new ScheduleDAO(context, _mapper);

                // Act & Assert
                if (exists)
                {
                    var schedule = await dao.GetScheduleById(scheduleId);

                    // Kiểm tra dữ liệu được trả về
                    Assert.NotNull(schedule);
                    Assert.Equal(scheduleId, schedule.ScheduleId);
                }
                else
                {
                    var exception = await Assert.ThrowsAsync<Exception>(
                        async () => await dao.GetScheduleById(scheduleId)
                    );

                    // Kiểm tra thông báo lỗi
                    Assert.Contains("Schedule not found", exception.Message);
                }
            }
        }
    }
}
