using BusinessObject.DTOS;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Interfaces
{
    public interface IAttendanceRepository
    {
        Task CreateDailyCheckin();
        Task CreateDailyAttendance();
        Task CreateDailyCheckout();
        Task<List<AttendanceMapper>> GetAttendanceByStudentId(int studentId, string type, DateTime date);
        Task<List<AttendanceMapper>> GetAttendanceByClassId(int classId, string type);
        Task<List<AttendanceMapper>> GetAttendanceByDate(int classId, string type, DateTime date);
        Task<List<AttendanceMapper>> GetAttendanceByTypeAndDate(string type, DateTime date);
        Task UpdateAttendance(List<AttendanceMapper> attendanceMappers, int classId, string type);
        Task UpdateAttendanceByType(List<AttendanceMapper> attendanceMappers, string type);
        Task UploadImageAndSaveToDatabaseAsync(int attendanceDetailID, IFormFile image);
    }
}
