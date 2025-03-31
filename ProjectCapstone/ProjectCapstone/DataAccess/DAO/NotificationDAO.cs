using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class NotificationDAO
    {
        private readonly kmsContext _context;
        private readonly IMapper _mapper;

        public NotificationDAO(kmsContext dbContext, IMapper mapper)
        {
            _context = dbContext;
            _mapper = mapper;
        }

        public async Task AddNotificationByRoleId(string title, string message, int roleId)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title is required.");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message is required.");
            }

            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
            {
                throw new ArgumentException($"RoleId {roleId} is invalid.");
            }
            try
            {
                var notification = new Notification
                {
                    Title = title,
                    Message = message,
                    RoleId = roleId,
                    CreatedAt = DateTime.Now
                };

                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();

                var userIds = await GetUserIdsByRoleId(roleId);

                var userNotifications = userIds.Select(userId => new Usernotification
                {
                    UserId = userId,
                    NotificationId = notification.NotificationId,
                    Status = "Unread",
                    ReadAt = null
                }).ToList();

                await _context.Usernotifications.AddRangeAsync(userNotifications);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding the notification and user notifications.", ex);
            }
        }

        public async Task AddNotificationByUserId(string title, string message, int userId)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title is required.");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message is required.");
            }

            if (userId <= 0)
            {
                throw new ArgumentException("Invalid UserId.");
            }
            try
            {
                var roleId = await GetRoleIdByUserId(userId);

                if (roleId == null)
                {
                    throw new Exception("Invalid RoleId for the specified UserId.");
                }

                var notification = new Notification
                {
                    Title = title,
                    Message = message,
                    RoleId = roleId.Value,
                    CreatedAt = DateTime.Now
                };

                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();

                var userNotification = new Usernotification
                {
                    UserId = userId, 
                    NotificationId = notification.NotificationId,
                    Status = "Unread",
                    ReadAt = null
                };

                await _context.Usernotifications.AddRangeAsync(userNotification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding the notification and user notifications.", ex);
            }
        }

        public async Task<List<NotificationMapper>> GetNotificationByUserId(int userId)
        {
            try
            {
                var userNotifications = await _context.Usernotifications
                                                      .Where(un => un.UserId == userId)
                                                      .ToListAsync();

                if (!userNotifications.Any())
                {
                    return new List<NotificationMapper>();
                }

                var notificationIds = userNotifications.Select(un => un.NotificationId).ToList();
                var notifications = await _context.Notifications
                                                  .Where(n => notificationIds.Contains(n.NotificationId))
                                                  .ToListAsync();

                var notificationMappers = notifications.Select(notification => new NotificationMapper
                {
                    NotificationId = notification.NotificationId,
                    Title = notification.Title,
                    Message = notification.Message,
                    CreatedAt = notification.CreatedAt,
                    RoleId = notification.RoleId,
                    Usernotifications = userNotifications
                        .Where(un => un.NotificationId == notification.NotificationId)
                        .Select(un => new UsernotificationMapper
                        {
                            UserNotificationId = un.UserNotificationId,
                            UserId = un.UserId,
                            NotificationId = un.NotificationId,
                            Status = un.Status,
                            ReadAt = un.ReadAt
                        }).ToList()
                }).ToList();

                return notificationMappers;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching notifications and user notifications.", ex);
            }
        }

        public async Task UpdateNotificationStatus(int userNotificationID)
        {
            try
            {
                var userNotification = await _context.Usernotifications
                                                      .FirstOrDefaultAsync(un => un.UserNotificationId == userNotificationID);

                if (userNotification == null)
                {
                    throw new Exception("UserNotification not found.");
                }

                userNotification.Status = "Readed";
                userNotification.ReadAt = DateTime.Now;

                _context.Usernotifications.Update(userNotification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the notification status.", ex);
            }
        }

        public async Task<IEnumerable<int>> GetUserIdsByRoleId(int roleId)
        {
            try
            {
                return await _context.Users
                                     .Where(u => u.RoleId == roleId)
                                     .Select(u => u.UserId)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving UserIDs by role ID.", ex);
            }
        }

        public async Task<int?> GetRoleIdByUserId(int userId)
        {
            try
            {
                var user = await _context.Users
                                         .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return null;
                }

                return user.RoleId;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the RoleId.", ex);
            }
        }
    }
}
