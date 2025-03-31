//using AutoMapper;
//using BusinessObject.Models;
//using Microsoft.AspNetCore.Cryptography.KeyDerivation;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using System;
//using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.DTOS;
using System.Net;
using Respository.Interfaces;
using System.Reflection;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using BusinessObject.Models;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Xml.Linq;
namespace Respository.Services
{
    public class AccountRepository : IAccountRepository
    {
        private readonly kmsContext _context;
        private readonly IConfiguration _configuration;
        public AccountRepository(kmsContext context, IConfiguration configuration
)
        {
            _configuration = configuration;
            _context = context;
        }

        public User GetUserByEmail(string email)
        {
            var userInSys = _context.Users.ToList();
            // Tìm người dùng theo email
            return _context.Users.FirstOrDefault(u => u.Mail == email);
        }
        public User GetUserById(int id)
        {
            return _context.Users
                .FirstOrDefault(u => u.UserId == id);
        }
        public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public (string AccessToken, string RefreshToken, DateTime ExpiryDate) GenerateToken(User user)
        {
            // Check if the user object is valid
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            // Lấy key bí mật từ cấu hình
            var secretKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT Secret Key is not configured.");
            }

            var issuer = _configuration["Jwt:Issuer"];
            if (string.IsNullOrEmpty(issuer))
            {
                throw new InvalidOperationException("JWT Issuer is not configured.");
            }

            var audience = _configuration["Jwt:Audience"];
            if (string.IsNullOrEmpty(audience))
            {
                throw new InvalidOperationException("JWT Audience is not configured.");
            }

            // Create key for signing the token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // Tạo thông tin xác thực (claims) chứa thông tin người dùng
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString() ?? string.Empty),
        new Claim(JwtRegisteredClaimNames.Email, user.Mail ?? string.Empty),
        new Claim(ClaimTypes.Role, user.RoleId.ToString() ?? string.Empty)
    };

            // Tạo ký số và mã hóa token
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Tạo Access Token với thời gian sống ngắn
            var expiryDate = DateTime.UtcNow.AddHours(1); // Token hết hạn sau 1 giờ
            var accessToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiryDate,
                signingCredentials: creds
            );

            // Tạo Refresh Token (chỉ là một chuỗi ngẫu nhiên, có thể phức tạp hơn)
            var refreshToken = Guid.NewGuid().ToString();

            // Trả về Access Token, Refresh Token, và Expiry Date
            return (
                AccessToken: new JwtSecurityTokenHandler().WriteToken(accessToken),
                RefreshToken: refreshToken,
                ExpiryDate: expiryDate
            );
        }
        public static string EncryptPassword(string? originPassword, string salt)
        {
            if (string.IsNullOrWhiteSpace(originPassword) || string.IsNullOrWhiteSpace(salt))
            {
                throw new ArgumentNullException(nameof(originPassword), " is null or blank!");
            }
            byte[] saltkey = Encoding.UTF8.GetBytes(salt);
            var encryptPassword = KeyDerivation.Pbkdf2(
                password: originPassword,
                salt: saltkey,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8);

            string result = Convert.ToHexString(encryptPassword).ToLower();
            return result;
        }
        public static bool VerifyPassword(string originPassword, string storedPassword, string salt)
        {
            try
            {
                string encryptPassword = EncryptPassword(originPassword, salt);
                return storedPassword.Equals(encryptPassword);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public BusinessObject.DTOS.Request.LoginResponse Login(string email, string password)
        {
            // Tìm người dùng theo email
            var user = GetUserByEmail(email);

            if (user == null)
            {
                return new BusinessObject.DTOS.Request.LoginResponse
                {
                    Message = "Your account not exsit."
                }; 
            }

            if (!VerifyPassword(password, user.Password, user.SaltKey))
            {
                return new BusinessObject.DTOS.Request.LoginResponse
                {
                    Message = "Incorrect password, please try again."
                };

            }

            if (user.Status == 0)
            {
                return new BusinessObject.DTOS.Request.LoginResponse
                {
                    Message = "Your account has been locked."
                };
            }
            // Đăng nhập thành công, tạo và lưu token
            var (accessToken, refreshToken, expiryDate) = GenerateToken(user);
            SaveToken(user, accessToken, refreshToken, expiryDate);

            // Trả về đối tượng chứa thông tin người dùng và token
            return new BusinessObject.DTOS.Request.LoginResponse
            {
                User = user,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiryDate = expiryDate
            };
        }

        public Token SaveToken(User user, string accessToken, string refreshToken, DateTime expiryDate)
        {
            // Chuyển đổi TokenAddModel thành đối tượng Token trước khi lưu
            var tokenEntity = new Token
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiryDate = expiryDate,
                UserId = user.UserId
            };

            _context.Tokens.Add(tokenEntity);
            _context.SaveChanges();

            // Trả về đối tượng Token đã được thêm
            return tokenEntity;
        }
        public User Register(BusinessObject.DTOS.Request.RegisterViewModel model)
        {
            // Kiểm tra email có trùng không
            var existingUser = _context.Users.FirstOrDefault(x => x.Mail == model.Mail);
            if (existingUser != null)
            {
                throw new Exception("Email đã được sử dụng.");
            }

            // Kiểm tra RoleId có hợp lệ không
            var validRole = _context.Roles.Any(r => r.RoleId == model.RoleId);
            if (!validRole)
            {
                throw new Exception("Invalid RoleId. Please check the role again.");
            }

            // Băm mật khẩu và tạo user
            var saltKey = GenerateSaltKey();
            var hashedPassword = EncryptPassword(model.Password, saltKey);

            // Khởi tạo user
            var user = new User
            {
                Mail = model.Mail,
                RoleId = model.RoleId,
                Password = hashedPassword,
                Status = 1,
                SaltKey = saltKey,
                CreateAt = DateTime.Now,
            };

            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();

                // Cập nhật mã Code dựa trên RoleId và UserId
                switch (model.RoleId)
                {
                    case 2: // Parent
                        user.Code = "PA" + user.UserId;
                        break;
                    case 5: // Teacher
                        user.Code = "TA" + user.UserId;
                        break;
                    case 4: // Principal
                        user.Code = "PR" + user.UserId;
                        break;
                    case 3: // Staff
                        user.Code = "SF" + user.UserId;
                        break;
                    case 1: // Admin
                        user.Code = "AD" + user.UserId;
                        break;
                    default:
                        throw new Exception("Invalid Role. Cannot generate code.");
                }

                _context.Users.Update(user);
                _context.SaveChanges();

                // Kiểm tra RoleId và tạo Parent hoặc Teacher tương ứng
                if (model.RoleId == 5) 
                {
                    var teacher = new Teacher
                    {
                        TeacherNavigation = user, 
                        HomeroomTeacher = 0,
                    };
                    _context.Teachers.Add(teacher);
                }
                else if (model.RoleId == 2) 
                {
                    var parent = new Parent
                    {
                        ParentNavigation = user, 
                    };
                    _context.Parents.Add(parent);
                }
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while registering the user:" + ex.Message);
            }

            return user;
        }

        public string GenerateSaltKey()
        {
            // Cách tạo salt key (thay đổi theo yêu cầu)
            using (var rng = new RNGCryptoServiceProvider())
            {
                var saltBytes = new byte[32];
                rng.GetBytes(saltBytes);
                return Convert.ToBase64String(saltBytes);
            }
        }

        public virtual User Update(User user)
        {
            _context.Update(user);
            _context.SaveChanges();
            return user;
        }
        public string GeneratePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var password = new StringBuilder(6);

            for (int i = 0; i < 6; i++)
            {
                int index = random.Next(chars.Length);
                password.Append(chars[index]);
            }

            return password.ToString();
        }
    }
}
