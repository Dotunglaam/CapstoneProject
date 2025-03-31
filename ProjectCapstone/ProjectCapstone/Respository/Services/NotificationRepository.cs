using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDAO _notificationDAO;

        public NotificationRepository(NotificationDAO notificationDAO)
        {
            _notificationDAO = notificationDAO;
        }
        public async Task AddNotificationByRoleId(string title, string message, int roleId)
        {
            try
            {
                await _notificationDAO.AddNotificationByRoleId(title, message, roleId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task AddNotificationByUserId(string title, string message, int userId)
        {
            try
            {
                await _notificationDAO.AddNotificationByUserId(title, message, userId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task UpdateNotificationStatus(int userNotificationID)
        {
            try
            {
                await _notificationDAO.UpdateNotificationStatus(userNotificationID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<NotificationMapper>> GetNotificationByUserId(int userId)
        {
            try
            {
                var notification = await _notificationDAO.GetNotificationByUserId(userId);
                return notification;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
