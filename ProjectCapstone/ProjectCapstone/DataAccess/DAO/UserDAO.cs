using AutoMapper;
using BusinessObject.Models;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class UserDAO
    {
        private readonly kmsContext _context;
        private readonly IMapper _mapper;

        public UserDAO(kmsContext dbContext, IMapper mapper)
        {
            _context = dbContext;
            _mapper = mapper;
        }
        public async Task UpdateUserStatus(int userID)
        {
            var user = await _context.Users
                                      .FirstOrDefaultAsync(u => u.UserId == userID);

            if (user == null)
            {
                throw new Exception("User not found");
            }
            if (user.Status == 1)
            {
                user.Status = (sbyte)0;
            }
            else if (user.Status == 0)
            {
                user.Status = (sbyte)1;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserRole(int userID)
        {
            try
            {
                var user = await _context.Users
                                          .FirstOrDefaultAsync(u => u.UserId == userID);

                if (user == null)
                {
                    throw new Exception("User not found");
                }

                if (user.RoleId == 5)
                {
                    user.RoleId = 6;
                }
                else if (user.RoleId == 6)
                {
                    user.RoleId = 5;
                }
                else
                {
                    throw new Exception("Invalid RoleID. Only RoleID 5 or 6 can be updated.");
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the user role.");
            }
        }


    }
}
