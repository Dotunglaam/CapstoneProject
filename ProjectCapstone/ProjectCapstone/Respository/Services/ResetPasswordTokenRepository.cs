using BusinessObject.Models;
using Respository.Interfaces;
using DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.DTOS;
using System.Net.Mail;
using System.Net;

namespace Respository.Services
{
    public class ResetPasswordTokenRepository : IResetPasswordTokenRepository
    {
        private readonly kmsContext _context;

        public ResetPasswordTokenRepository(kmsContext context) 
        {
            _context = context;
        }

        // Lưu token đặt lại mật khẩu
        public void SaveResetToken(Resetpasswordtoken token)
        {
            // Kiểm tra xem người dùng đã có token đặt lại chưa
            var existingToken = _context.Resetpasswordtokens
                .FirstOrDefault(t => t.UserId == token.UserId);

            if (existingToken != null)
            {
                // Cập nhật token và thời gian hết hạn mới
                existingToken.Token = token.Token;
                existingToken.ExpiryTime = token.ExpiryTime;
                _context.Resetpasswordtokens.Update(existingToken);
            }
            else
            {
                // Tạo mới token đặt lại
                _context.Resetpasswordtokens.Add(token);
            }

            _context.SaveChanges();
        }

        // Lấy token đặt lại theo UserId
        public Resetpasswordtoken GetResetTokenByUserId(int userId)
        {
            return _context.Resetpasswordtokens.FirstOrDefault(t => t.UserId == userId);
        }
        // Lấy token đặt lại theo token string
        public Resetpasswordtoken GetResetTokenByToken(string token)
        {
            return _context.Resetpasswordtokens.FirstOrDefault(t => t.Token == token);
        }
        public void RemoveResetToken(int userId)
        {
            var token = _context.Resetpasswordtokens.FirstOrDefault(t => t.UserId == userId);
            if (token != null)
            {
                _context.Resetpasswordtokens.Remove(token);
                _context.SaveChanges();
            }
        }
        public void SendMail(string email, string sub, string body)
        {
            try
            {
                // Cấu hình SmtpClient
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials =
                    new NetworkCredential("lamdthe163085@fpt.edu.vn", "iyni xhmb rtij tnen");
                smtpClient.EnableSsl = true;

                // Tạo đối tượng MailMessage
                MailMessage message = new MailMessage();
                message.From = new MailAddress("lamdthe163085@fpt.edu.vn");
                message.To.Add(email);
                message.Subject = sub;
                message.Body = body;
                message.IsBodyHtml = true; // Đảm bảo nội dung email là HTML
                // Gửi email
                smtpClient.Send(message);

                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
    }
}

