using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Interfaces
{
    public interface IResetPasswordTokenRepository 
    {
        void SaveResetToken(Resetpasswordtoken token);
        Resetpasswordtoken GetResetTokenByUserId(int userId);
        Resetpasswordtoken GetResetTokenByToken(string token);
        void RemoveResetToken(int userId);
        void SendMail(string email, string sub, string body);

    }
}
