using BusinessObject.DTOS;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.DTOS.Request;

namespace Respository.Interfaces
{
    public interface IAccountRepository
    {
        public User GetUserByEmail(string email);
        User GetUserById(int id);
        bool IsValidEmail(string email);
        (string AccessToken, string RefreshToken, DateTime ExpiryDate) GenerateToken(User user);
        LoginResponse Login(string email, string password);
        Token SaveToken(User user, string accessToken, string refreshToken, DateTime expiryDate);
        User Register(BusinessObject.DTOS.Request.RegisterViewModel model);
        string GenerateSaltKey();
        User Update(User user);
        string GeneratePassword();
    }
}
