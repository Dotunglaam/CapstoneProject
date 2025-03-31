  using BusinessObject.DTOS;
using DataAccess.DAO;
using Microsoft.AspNetCore.Http;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly AttendanceDAO _attendanceDAO;
        public AttendanceRepository(AttendanceDAO attendanceDAO)
        {
            _attendanceDAO = attendanceDAO;
        }

        public async Task CreateDailyCheckin()
        {
            try
            {
                await _attendanceDAO.CreateDailyCheckin();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create daily check-in: " + ex.Message, ex);
            }
        }

        public async Task CreateDailyAttendance()
        {
            try
            {
                await _attendanceDAO.CreateDailyAttendance();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create daily attend: " + ex.Message, ex);
            }
        }

        public async Task CreateDailyCheckout()
        {
            try
            {
                await _attendanceDAO.CreateDailyCheckout();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create daily check-out: " + ex.Message, ex);
            }
        }

        public async Task<List<AttendanceMapper>> GetAttendanceByClassId(int classId, string type)
        {
            try
            {
                return await _attendanceDAO.GetAttendanceByClassId(classId, type); 
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get all attendance: " + ex.Message, ex);
            }
        }

        public async Task<List<AttendanceMapper>> GetAttendanceByDate(int classId, string type, DateTime date)
        {
            try
            {
                return await _attendanceDAO.GetAttendanceByDate(classId, type, date);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get all attendance: " + ex.Message, ex);
            }
        }
        public async Task<List<AttendanceMapper>> GetAttendanceByTypeAndDate(string type, DateTime date)
        {
            try
            {
                return await _attendanceDAO.GetAttendanceByTypeAndDate(type, date);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get all attendance: " + ex.Message, ex);
            }
        }
        public async Task<List<AttendanceMapper>> GetAttendanceByStudentId(int studentId, string type, DateTime date)
        {
            try
            {
                return await _attendanceDAO.GetAttendanceByStudentId(studentId, type, date);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get all attendance: " + ex.Message, ex);
            }
        }
        
        public async Task UpdateAttendance(List<AttendanceMapper> attendanceMappers, int classId, string type)
        {
            try
            {
                await _attendanceDAO.UpdateAttendance( attendanceMappers, classId, type);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update attendance: " + ex.Message, ex);
            }
        }
        public async Task UpdateAttendanceByType(List<AttendanceMapper> attendanceMappers, string type)
        {
            try
            {
                await _attendanceDAO.UpdateAttendanceByType(attendanceMappers, type);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update attendance: " + ex.Message, ex);
            }
        }
        
        public async Task UploadImageAndSaveToDatabaseAsync(int attendanceDetailID, IFormFile image)
        {
            try
            {
                await _attendanceDAO.UploadImageAndSaveToDatabaseAsync(attendanceDetailID, image);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update attendance: " + ex.Message, ex);
            }
        }
    }
}
