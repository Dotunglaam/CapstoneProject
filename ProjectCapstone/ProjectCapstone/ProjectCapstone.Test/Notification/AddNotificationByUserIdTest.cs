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
    public class AddNotificationByUserIdTest
    {
        private readonly DbContextOptions<kmsContext> _dbOptions;
        private readonly IMapper _mapper;

        public AddNotificationByUserIdTest()
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

        // Test trường hợp UserId không hợp lệ
        [Theory]
        [InlineData(0, "Invalid UserId.")]
        [InlineData(-1, "Invalid UserId.")]
        public async Task AddNotificationByUserId_ShouldThrowException_WhenUserIdIsInvalid(int userId, string expectedMessage)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Thêm thông tin role hợp lệ vào cơ sở dữ liệu
                context.Roles.Add(new Role { RoleId = 1, RoleName = "Admin" });
                await context.SaveChangesAsync();

                // Thêm user hợp lệ vào cơ sở dữ liệu
                context.Users.Add(new User
                {
                    UserId = 1,
                    Firstname = "Test",
                    LastName = "User",
                    Mail = "test@example.com",
                    Password = "hashed_password",
                    SaltKey = "salt_key"
                });
                await context.SaveChangesAsync();

                var dao = new NotificationDAO(context, _mapper);

                // Act
                var exception = await Assert.ThrowsAsync<ArgumentException>(() => dao.AddNotificationByUserId("Test Title", "Test Message", userId));

                // Assert
                Assert.Equal(expectedMessage, exception.Message);
            }
        }

        // Test trường hợp thiếu dữ liệu bắt buộc (Title hoặc Message)
        [Theory]
        [InlineData("", "Test Message", "Title is required.")]
        [InlineData("Test Title", "", "Message is required.")]
        public async Task AddNotificationByUserId_ShouldThrowException_WhenInvalidData(string title, string message, string expectedMessage)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Thêm role hợp lệ vào cơ sở dữ liệu
                context.Roles.Add(new Role { RoleId = 1, RoleName = "Admin" });
                await context.SaveChangesAsync();

                // Thêm user hợp lệ vào cơ sở dữ liệu
                var user = new User
                {
                    UserId = 1,
                    Firstname = "Test",
                    LastName = "User",
                    Mail = "testuser@example.com",  // Thêm Mail
                    Password = "TestPassword",     // Thêm Password
                    SaltKey = "RandomSaltKey123",  // Thêm SaltKey
                    RoleId = 1                     // Thêm RoleId (nếu cần thiết)
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();

                var dao = new NotificationDAO(context, _mapper);

                // Act & Assert
                var exception = await Assert.ThrowsAsync<ArgumentException>(() => dao.AddNotificationByUserId(title, message, 1));

                Assert.Equal(expectedMessage, exception.Message);
            }
        }

        // Test trường hợp thêm thông báo thành công
        [Fact]
        public async Task AddNotificationByUserId_ShouldAddSuccessfully_WhenDataIsValid()
        {
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Arrange: Thêm Role và User hợp lệ vào cơ sở dữ liệu
                var role = new Role { RoleId = 1, RoleName = "Admin" };
                context.Roles.Add(role);
                await context.SaveChangesAsync();

                var user = new User
                {
                    UserId = 1,
                    Firstname = "Test",
                    LastName = "User",
                    Mail = "testuser@example.com",  
                    Password = "TestPassword",     
                    SaltKey = "RandomSaltKey123",  
                    RoleId = 1                     
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();

                var dao = new NotificationDAO(context, _mapper);

                // Act
                await dao.AddNotificationByUserId("Test Title", "Test Message", 1);

                // Assert: Kiểm tra thông báo có được thêm vào cơ sở dữ liệu không
                var notification = await context.Notifications.FirstOrDefaultAsync(n => n.Title == "Test Title");

                Assert.NotNull(notification);
                Assert.Equal("Test Title", notification.Title);
                Assert.Equal("Test Message", notification.Message);
                Assert.Equal(1, notification.RoleId);
                Assert.NotNull(notification.CreatedAt);

                // Kiểm tra Usernotification có được thêm vào không
                var userNotification = await context.Usernotifications
                    .FirstOrDefaultAsync(un => un.UserId == 1 && un.NotificationId == notification.NotificationId);

                Assert.NotNull(userNotification);
                Assert.Equal("Unread", userNotification.Status);
                Assert.Null(userNotification.ReadAt);
            }
        }
    }
}
