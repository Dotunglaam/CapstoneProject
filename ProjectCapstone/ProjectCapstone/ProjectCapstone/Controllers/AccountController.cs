using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Respository.Interfaces;
using Respository.Services;
using static BusinessObject.DTOS.Request;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using Microsoft.AspNetCore.Authorization;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _AccountRepository;
        private readonly IResetPasswordTokenRepository _ResetPasswordToken;

        public AccountController(IAccountRepository accountRepository, IResetPasswordTokenRepository resetPasswordTokenRepository)
        {
            _AccountRepository = accountRepository;
            _ResetPasswordToken = resetPasswordTokenRepository;

        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email and password cannot be empty.");
            }

            var loginResponse = _AccountRepository.Login(request.Email, request.Password);

            if (loginResponse == null)
            {
                return Unauthorized("Login failed, please check your information.");
            }
            // Kiểm tra nếu tài khoản bị khóa
            if (!string.IsNullOrEmpty(loginResponse.Message))
            {
                return Unauthorized(loginResponse.Message); // Trả về thông báo tài khoản bị khóa
            }
            // Tạo đối tượng phản hồi trả về cho người dùng
            var response = new
            {
                User = new
                {
                    loginResponse.User.UserId,
                    loginResponse.User.Firstname,
                    loginResponse.User.LastName,
                    loginResponse.User.Mail,
                    loginResponse.User.PhoneNumber,
                    loginResponse.User.RoleId,
                },
                AccessToken = loginResponse.AccessToken,
                ExpiryDate = loginResponse.ExpiryDate
            };

            return Ok(response); // Trả về đối tượng phản hồi với HTTP 200
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] BusinessObject.DTOS.Request.RegisterViewModel model)
        {
            if (model == null)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                model.Password = _AccountRepository.GeneratePassword();
               
                if (!ModelState.IsValid || string.IsNullOrEmpty(model.Mail))
                {
                    return BadRequest("Email cannot be empty."); // Return error message
                }
                var user = _AccountRepository.Register(model);
                // Kiểm tra xem email có tồn tại trong hệ thống hay không
                var user1 = _AccountRepository.GetUserByEmail(model.Mail);
                if (user1 == null)
                {
                    // Email không tồn tại, trả về thông báo lỗi
                    return NotFound("The email address has not been successfully created. Please try again.");
                }

                var frontendUrl = "http://edunest.io.vn/";

                var emailBody = $"Xin chào bạn,<br/><br/>" +
                        $"Tài khoản của bạn đã được tạo thành công với thông tin sau:<br/>" +
                        $"Email: {user.Mail}<br/>" +
                        $"Mật khẩu: {model.Password}<br/><br/>" +
                        $"Vui lòng đăng nhập link sau  <a href=\"{frontendUrl}\">{frontendUrl}</a>. Để vào hệ thống và 'thay đổi mật khẩu' và 'cập nhật thông tin' của mình ngay sau khi đăng nhập để đảm bảo an toàn cho tài khoản của bạn.";

                // Gọi hàm gửi email thông báo
                _ResetPasswordToken.SendMail(user.Mail, "Account creation successful notification", emailBody);

                return Ok("Account created successfully");
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost("ImportListAccounts")]
        public async Task<IActionResult> ImportAccounts(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is invalid or empty.");
            }

            try
            {
                var accounts = new List<RegisterViewModel>();
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    // Thiết lập LicenseContext cho ExcelPackage
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (var package = new OfficeOpenXml.ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            return BadRequest("Invalid Excel file.");
                        }

                        var rowCount = worksheet.Dimension.Rows;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var email = worksheet.Cells[row, 1]?.Value?.ToString()?.Trim();
                            var roleId = int.TryParse(worksheet.Cells[row, 2]?.Value?.ToString(), out int rId) ? rId : 0;

                            if (string.IsNullOrEmpty(email) || roleId == 0)
                            {
                                Console.WriteLine($"Skipped row {row}: Invalid data.");
                                continue;
                            }

                            Console.WriteLine($"Row {row} - Email: {email}, RoleId: {roleId}");

                            var account = new RegisterViewModel
                            {
                                Mail = email,
                                RoleId = roleId,
                                Password = _AccountRepository.GeneratePassword(),
                            };
                            accounts.Add(account);
                        }
                    }
                }

                try
                {
                    foreach (var account in accounts)
                    {
                        if (_AccountRepository.GetUserByEmail(account.Mail) != null)
                        {
                            Console.WriteLine($"Account with email {account.Mail} already exists.");
                            return BadRequest($"Account with email {account.Mail} already exists.");
                        }

                        var user = _AccountRepository.Register(account);
                        Console.WriteLine($"Account created: {account.Mail}");

                        var emailBody = $"Xin chào bạn,<br/><br/>" +
                                        $"Tài khoản của bạn đã được tạo thành công với thông tin sau:<br/>" +
                                        $"Email: {account.Mail}<br/>" +
                                        $"Mật khẩu: {account.Password}<br/><br/>" +
                                        $"Vui lòng đăng nhập link sau <a href=\"http://edunest.io.vn/\">http://edunest.io.vn/</a>.";

                        _ResetPasswordToken.SendMail(account.Mail, "Account creation successful notification", emailBody);
                        Console.WriteLine($"Email sent to {account.Mail}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing account {ex.Message}");
                }

                return Ok($"{accounts.Count} accounts processed.");
            }
            catch (Exception ex)
            {
                return Conflict($"Error processing file: {ex.Message}");
            }
        }


        [HttpPost("Forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            // Kiểm tra tính hợp lệ của email
            if (!ModelState.IsValid || string.IsNullOrEmpty(model.Mail))
            {
                return BadRequest("Email cannot be empty."); // Return error message
            }

            // Kiểm tra xem email có tồn tại trong hệ thống hay không
            var user = _AccountRepository.GetUserByEmail(model.Mail);
            if (user == null)
            {
                // Email không tồn tại, trả về thông báo lỗi
                return NotFound("The email address does not exist in the system. Please try again.");
            }

            // Tạo reset token
            var resetToken = Guid.NewGuid().ToString();

            var frontendUrl = "http://edunest.io.vn/resetpassword";

            // Tạo liên kết reset với token
            var resetLink = $"{frontendUrl}?token={resetToken}";

            var tokenModel = new Resetpasswordtoken
            {
                UserId = user.UserId,
                Token = resetToken,
                ExpiryTime = DateTime.Now.AddHours(1)
            };

            _ResetPasswordToken.SaveResetToken(tokenModel);

            var emailBody = $"Chúng tôi vừa nhận được yêu cầu đặt lại mật khẩu cho: {model.Mail}. <br/><br/>" +
                    $"Vui lòng nhấp vào đây để đặt lại mật khẩu: <a href=\"{resetLink}\">{resetLink}</a>.<br/><br/> " +
                    "Để bảo mật, liên kết này sẽ hết hạn trong 1 giờ hoặc ngay lập tức sau khi bạn đặt lại mật khẩu.";

            _ResetPasswordToken.SendMail(model.Mail, "Yêu cầu đặt lại mật khẩu", emailBody);
            // Thông báo thành công
            return Ok(tokenModel.Token);
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid input information."); // Return error if the model is invalid
            }

            // Kiểm tra token trong cơ sở dữ liệu
            var resetToken = _ResetPasswordToken.GetResetTokenByToken(model.Token);

            if (resetToken == null || resetToken.ExpiryTime < DateTime.Now)
            {
                // Token không hợp lệ hoặc đã hết hạn
                return BadRequest("The token is invalid or has expired.");
            }

            // Lấy thông tin người dùng dựa trên UserId của token
            var user = _AccountRepository.GetUserById(resetToken.UserId);
            if (user == null)
            {
                // Người dùng không tồn tại
                return NotFound("User not found.");
            }

            // Kiểm tra yêu cầu mật khẩu mới
            var passwordRegex = new Regex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{7,}$");
            if (!passwordRegex.IsMatch(model.Password))
            {
                return BadRequest("Password must contain at least 1 number, 1 lowercase letter, 1 uppercase letter, and be at least 7 characters long.");
            }

            // Hash mật khẩu mới và lưu vào cơ sở dữ liệu
            var saltKey = _AccountRepository.GenerateSaltKey();
            var hashedPassword = AccountRepository.EncryptPassword(model.Password, saltKey);
            user.Password = hashedPassword;
            user.SaltKey = saltKey;

            // Cập nhật thông tin người dùng
            _AccountRepository.Update(user);

            // Xóa token sau khi đã sử dụng
            _ResetPasswordToken.RemoveResetToken(user.UserId);


            // Trả về phản hồi thành công
            return Ok("Your password has been successfully reset.");
        }
    }
}
