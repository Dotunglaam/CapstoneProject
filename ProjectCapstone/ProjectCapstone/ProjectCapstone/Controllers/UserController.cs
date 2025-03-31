using BusinessObject.DTOS;
using BusinessObject.Models;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Respository.Interfaces;
using Respository.Services;
using System.Text.RegularExpressions;
using static BusinessObject.DTOS.Request;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
    
        private readonly IUserRepository _UserRespository;
        private readonly IAccountRepository _AccountRepository;
        public UserController(IUserRepository userRepository,IAccountRepository accountRepository)
        {
            _UserRespository = userRepository;
            _AccountRepository = accountRepository;   
        }
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _UserRespository.GetAllUsers();
            return Ok(users);
        }

        
        [HttpGet("ProfileById/{id}")]
        public IActionResult GetUserById(int id)
        {
            var user = _UserRespository.GetUserById(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            return Ok(user);
        }

        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateUserAsync([FromForm] ViewProfileModel updatedUser)
        {
            // Validate the model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update user in the database
            var result = await _UserRespository.UpdateAsync(updatedUser);

            if (result == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new { message = "Profile updated successfully." });
        }

        [HttpPut("UpdateUserStatus")]
        public async Task<IActionResult> UpdateUserStatus(int userID)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

             await _UserRespository.UpdateUserStatus(userID);

            return Ok(new { message = "User status updated successfully." });
        }

        [HttpPut("UpdateUserRole")]
        public async Task<IActionResult> UpdateUserRole(int userID)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _UserRespository.UpdateUserRole(userID);

            return Ok(new { message = "User role updated successfully." });
        }

        [HttpPut("ChangePassWord/{userId}")]
        public IActionResult ChangePassword(int userId, [FromBody] ChangePasswordViewModel model)
        {
            
            if (model.NewPassword != model.ConfirmNewPassword)
            {
                return BadRequest("Mật khẩu mới và mật khẩu xác nhận không khớp.");
            }

            
            var user = _UserRespository.GetUserById(userId);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            
            var hashedCurrentPassword = AccountRepository.EncryptPassword(model.CurrentPassword, user.SaltKey);
            if (hashedCurrentPassword != user.Password)
            {
                return BadRequest("Mật khẩu hiện tại không chính xác.");
            }


            // Kiểm tra yêu cầu mật khẩu mới
            var passwordRegex = new Regex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{7,}$");
            if (!passwordRegex.IsMatch(model.NewPassword))
            {
                return BadRequest("Mật khẩu phải có ít nhất 1 số, 1 chữ cái viết thường, 1 chữ cái viết hoa, và dài ít nhất 7 ký tự.");
            }

            // Hash mật khẩu mới và lưu vào cơ sở dữ liệu
            var saltKey = _AccountRepository.GenerateSaltKey();
            var hashedPassword = AccountRepository.EncryptPassword(model.NewPassword, saltKey);
            user.Password = hashedPassword;
            user.SaltKey = saltKey;

            // Cập nhật thông tin người dùng
            _AccountRepository.Update(user);

            // Trả về phản hồi thành công
            return Ok("Mật khẩu của bạn đã được đặt lại thành công.");
        }
    }
}
