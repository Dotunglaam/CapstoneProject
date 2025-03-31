using BusinessObject.DTOS;
using BusinessObject.Models;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Respository.Interfaces;
using System.Collections.Generic;
using System.Linq;
using static BusinessObject.DTOS.Request;
using DataAccess.DAO;

namespace Respository.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDAO _userDAO;
        private readonly kmsContext _context;
        private readonly Cloudinary _cloudinary;
        public UserRepository(kmsContext context, Cloudinary cloudinary, UserDAO userDAO)
        {
            _context = context;
            _cloudinary = cloudinary;
            _userDAO = userDAO;
        }
        // Create a new user
        public void Add(User user)
        {
            _context.Users.Add(user);
            Commit();
        }

        // Get a user by ID
        public User GetUserById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == id);
        }

        // Get all users
        public IEnumerable<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }

        // Update an existing user
        public async Task<User> UpdateAsync(ViewProfileModel updatedUser)
        {
            // Find the existing user by UserId
            var existingUser = await _context.Users.Include(x => x.Parent).Include(x => x.Teacher).FirstAsync(u => u.UserId == updatedUser.UserId);
            if (existingUser != null)
            {
                // Cập nhật các thông tin người dùng từ ViewProfileModel
                existingUser.Firstname = updatedUser.Firstname;
                existingUser.LastName = updatedUser.LastName;
                existingUser.Address = updatedUser.Address;
                existingUser.PhoneNumber = updatedUser.PhoneNumber;
                existingUser.Gender = (sbyte?)updatedUser.Gender;
                existingUser.Dob = updatedUser.Dob;
                if (existingUser.RoleId == 2)
                {
                    existingUser.Parent.Name = updatedUser.Firstname + " " + updatedUser.LastName;
                }
                if (existingUser.RoleId == 5)
                {
                    existingUser.Teacher.Name = updatedUser.Firstname + " " + updatedUser.LastName;
                }
                // Nếu có ảnh đại diện mới được tải lên, thực hiện tải lên Cloudinary
                if (updatedUser.Avatar != null)
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(updatedUser.Avatar.FileName, updatedUser.Avatar.OpenReadStream()),
                        Folder = $"UserProfileImages",
                        PublicId = $"{existingUser.UserId}", // Tên công khai cho ảnh
                        Overwrite = true // Ghi đè nếu ảnh đã tồn tại
                    };

                    // Tải ảnh lên Cloudinary và lưu URL của ảnh
                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    existingUser.Avatar = uploadResult.SecureUri.ToString();
                }
                // Save changes to the database
                await _context.SaveChangesAsync();

                return existingUser; // Return the updated user
            }

            return null; // User not found
        }

        public async Task UpdateUserStatus(int userID)
        {
            try
            {
                await _userDAO.UpdateUserStatus(userID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task UpdateUserRole(int userID)
        {
            try
            {
                await _userDAO.UpdateUserRole(userID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        // Commit changes to the database
        public void Commit()
        {
            _context.SaveChanges();
        }
    }
}
