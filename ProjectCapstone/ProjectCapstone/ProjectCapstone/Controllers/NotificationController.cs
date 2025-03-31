using BusinessObject.DTOS;
using Microsoft.AspNetCore.Mvc;
using Respository.Interfaces;
using Respository.Services;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        [HttpGet("GetNotificationByUserId")]
        public async Task<IActionResult> GetNotificationByUserId(int userId)
        {
            try
            {
                var result = await _notificationRepository.GetNotificationByUserId(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving Menu.", details = ex.Message });
            }
        }

        [HttpPut("UpdateNotificationStatus")]
        public async Task<IActionResult> UpdateNotificationStatus(int UserNotificationID)
        {
            try
            {
                await _notificationRepository.UpdateNotificationStatus(UserNotificationID);
                return Ok(new { message = "Notification Updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving Menu.", details = ex.Message });
            }
        }

        [HttpPost("AddNotificationByRoleId")]
        public async Task<IActionResult> AddNotificationByRoleId(string title, string message, int roleId)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message) || roleId <= 0)
            {
                return BadRequest(new { message = "Invalid notification request. Please provide a valid title, message, and role ID." });
            }

            try
            {
                await _notificationRepository.AddNotificationByRoleId(title, message, roleId);
                return Ok(new { message = "Notification added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while adding the notification.", details = ex.Message });
            }
        }

        [HttpPost("AddNotificationByUserId")]
        public async Task<IActionResult> AddNotificationByUserId(string title, string message, int userId)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message) || userId <= 0)
            {
                return BadRequest(new { message = "Invalid notification request. Please provide a valid title, message, and user ID." });
            }

            try
            {
                await _notificationRepository.AddNotificationByUserId(title, message, userId);
                return Ok(new { message = "Notification added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while adding the notification.", details = ex.Message });
            }
        }

    }
}
