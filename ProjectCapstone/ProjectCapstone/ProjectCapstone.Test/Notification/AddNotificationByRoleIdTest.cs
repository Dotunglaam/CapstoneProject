using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ProjectCapstone.Test.Notifications
{
    public class AddNotificationByRoleIdTest
    {
        private readonly DbContextOptions<kmsContext> _dbOptions;
        private readonly IMapper _mapper;

        public AddNotificationByRoleIdTest()
        {
            _dbOptions = new DbContextOptionsBuilder<kmsContext>()
                        .UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}")
                        .Options;

            // Cấu hình AutoMapper
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<NotificationMapper, Notification>();
            });
            _mapper = configuration.CreateMapper();
        }

        // Test trường hợp RoleId không hợp lệ
        [Theory]
        [InlineData(999, "RoleId 999 is invalid.")]
        [InlineData(2, "RoleId 2 is invalid.")]
        [InlineData(-1, "RoleId -1 is invalid.")]
        [InlineData(0, "RoleId 0 is invalid.")]
        public async Task AddNotificationByRoleId_ShouldThrowException_WhenRoleIdIsInvalid(int roleId, string expectedMessage)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Thêm thông tin cần thiết vào cơ sở dữ liệu
                context.Roles.Add(new Role { RoleId = 1, RoleName = "Admin" });
                await context.SaveChangesAsync();

                var dao = new NotificationDAO(context, _mapper);

                // Act
                var exception = await Assert.ThrowsAsync<ArgumentException>(() => dao.AddNotificationByRoleId("Test Title", "Test Message", roleId));

                // Assert
                Assert.Equal(expectedMessage, exception.Message);
            }
        }

        // Test trường hợp thiếu dữ liệu bắt buộc (Title hoặc Message)
        [Theory]
        [InlineData("", "Test Message", "Title is required.")]
        [InlineData("Test Title", "", "Message is required.")]
        [InlineData("Test Title", "Test Message", "RoleId 999 is invalid.")]
        public async Task AddNotificationByRoleId_ShouldThrowException_WhenInvalidData(string title, string message, string expectedMessage)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Thêm role hợp lệ vào cơ sở dữ liệu
                context.Roles.Add(new Role { RoleId = 1, RoleName = "Admin" });
                await context.SaveChangesAsync();

                var dao = new NotificationDAO(context, _mapper);

                // Act & Assert
                var exception = await Assert.ThrowsAsync<ArgumentException>(() => dao.AddNotificationByRoleId(title, message, 999));

                Assert.Equal(expectedMessage, exception.Message);
            }
        }

        // Test trường hợp thêm thông báo thành công
        [Fact]
        public async Task AddNotificationByRoleId_ShouldAddSuccessfully_WhenDataIsValid()
        {
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Arrange: Thêm Role hợp lệ vào cơ sở dữ liệu
                var role = new Role { RoleId = 1, RoleName = "Admin" };
                context.Roles.Add(role);
                await context.SaveChangesAsync();

                var dao = new NotificationDAO(context, _mapper);

                // Act
                await dao.AddNotificationByRoleId("Test Title", "Test Message", 1);

                // Assert: Kiểm tra thông báo có được thêm vào cơ sở dữ liệu không
                var notification = await context.Notifications.FirstOrDefaultAsync(n => n.Title == "Test Title");

                Assert.NotNull(notification);
                Assert.Equal("Test Title", notification.Title);
                Assert.Equal("Test Message", notification.Message);
                Assert.Equal(1, notification.RoleId);
                Assert.NotNull(notification.CreatedAt);
            }
        }
    }
}
