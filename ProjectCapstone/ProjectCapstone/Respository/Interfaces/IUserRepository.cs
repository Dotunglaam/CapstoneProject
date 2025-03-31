using BusinessObject.Models;
using System.Collections.Generic;
using static BusinessObject.DTOS.Request;

namespace Respository.Interfaces
{
    public interface IUserRepository
    {
        void Add(User user);
        User GetUserById(int id);
        IEnumerable<User> GetAllUsers();
        Task<User> UpdateAsync(ViewProfileModel updatedUser);
        Task UpdateUserStatus(int userID);
        Task UpdateUserRole(int userID);
        void Commit();
    }
}
