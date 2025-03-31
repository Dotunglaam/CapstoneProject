using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IScheduleRepository
    {
        Task<List<ScheduleMapper>> GetAllScheduleMappers();
        Task<ScheduleMapper> GetScheduleById(int id);
        Task ImportSchedulesAsync(IFormFile file, int ClassID); // Thay đổi tham số cho rõ nghĩa
        Task UpdateSchedule(ScheduleMapper schedule);
        Task AddSchedule(ScheduleMapper schedule); // Thêm hàm thêm mới lịch
        Task<List<ScheduleMapper>> GetSchedulesByClassId(int classId);

        Task<List<LocationMapper>> GetAllLocationMappers();
        Task<List<ActivityMapper>> GetAllActivityMappers();
    }
}
