using BusinessObject.DTOS;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Respository.Interfaces
{
    public interface INotificationRepository
    {
        Task AddNotificationByRoleId(string title, string message, int roleId);
        Task AddNotificationByUserId(string title, string message, int userId);
        Task<List<NotificationMapper>> GetNotificationByUserId(int userId);
        Task UpdateNotificationStatus(int userNotificationID);
    }
}
