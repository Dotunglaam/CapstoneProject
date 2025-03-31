using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.DTOS.Request;

namespace Respository.Services
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly kmsContext _context;

        public TeacherRepository(kmsContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TeacherForByAll>> GetAllTeachersAsync()
        {
            return await _context.Teachers
                .Include(t => t.TeacherNavigation)
                .Include(t =>t.Classes)
                .Select(t => new TeacherForByAll
                {
                    TeacherId = t.TeacherId,
                    Name = t.Name,
                    Education = t.Education,
                    Experience = t.Experience,

                    Firstname = t.TeacherNavigation.Firstname,
                    LastName = t.TeacherNavigation.LastName,
                    Address = t.TeacherNavigation.Address,
                    PhoneNumber = t.TeacherNavigation.PhoneNumber,
                    Mail = t.TeacherNavigation.Mail,
                    Status = t.TeacherNavigation.Status,
                    Gender = t.TeacherNavigation.Gender,
                    Dob = t.TeacherNavigation.Dob,
                    Code = t.TeacherNavigation.Code,
                    Avatar = t.TeacherNavigation.Avatar,
                    Role = t.TeacherNavigation.RoleId,
                    HomeroomTeacher = (int)t.HomeroomTeacher,
                    Classes = t.Classes
                    .Where(c => c.IsActive == 1)  // Nếu bạn chỉ muốn lấy các lớp đang hoạt động
                    .Select(c => c.ClassName)
                    .ToList()
                })
                .ToListAsync();
        }

        public async Task<TeacherForByAll> GetTeacherByIdAsync(int id)
        {
            return await _context.Teachers
                .Include(t => t.TeacherNavigation) // Include the User entity (TeacherNavigation)
                .Where(t => t.TeacherId == id)
                .Select(t => new TeacherForByAll
                {
                    TeacherId = t.TeacherId,
                    Name = t.Name,
                    Education = t.Education,
                    Experience = t.Experience,

                    Firstname = t.TeacherNavigation.Firstname,
                    LastName = t.TeacherNavigation.LastName,
                    Address = t.TeacherNavigation.Address,
                    PhoneNumber = t.TeacherNavigation.PhoneNumber,
                    Mail = t.TeacherNavigation.Mail,
                    Status = t.TeacherNavigation.Status,
                    Gender = t.TeacherNavigation.Gender,
                    Dob = t.TeacherNavigation.Dob,
                    Code = t.TeacherNavigation.Code,
                    Avatar = t.TeacherNavigation.Avatar,
                    Role = t.TeacherNavigation.RoleId,
                    HomeroomTeacher = (int)t.HomeroomTeacher,
                    Classes = t.Classes
                    .Where(c => c.IsActive == 1)  // Nếu bạn chỉ muốn lấy các lớp đang hoạt động
                    .Select(c => c.ClassName)
                    .ToList()
                })
                .FirstOrDefaultAsync(); // Fetch the first match or null if not found
        }


        public async Task<bool> SoftDeleteTeacherAsync(int id)
        {
            // Find the teacher by ID
            var teacher = await _context.Teachers.Include(t => t.TeacherNavigation).FirstOrDefaultAsync(t => t.TeacherId == id);

            if (teacher != null)
            {
                // Update the User status to inactive (0)
                teacher.TeacherNavigation.Status = 0; // Assuming 0 means inActive
                _context.Users.Update(teacher.TeacherNavigation);

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<Teacher> UpdateTeacherAsync(ViewProfileTeacherForTableTecher teacherModel)
        {
            var existingTeacher = await _context.Teachers
                .Include(t => t.TeacherNavigation) // Include related User (TeacherNavigation)
                .FirstOrDefaultAsync(t => t.TeacherId == teacherModel.TeacherId);

            if (existingTeacher != null)
            {
                var user = existingTeacher.TeacherNavigation;
                user.Teacher.Name = teacherModel.Name;
                user.Teacher.Education = teacherModel.Education;
                user.Teacher.Experience = teacherModel.Experience;
                // Save changes to the database
                _context.Teachers.Update(existingTeacher);
                await _context.SaveChangesAsync();

                return existingTeacher; 
            }

            return null; 
        }

    }
}
