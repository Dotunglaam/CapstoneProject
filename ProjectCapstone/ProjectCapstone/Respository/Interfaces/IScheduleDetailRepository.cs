using BusinessObject.DTOS;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Respository.Interfaces
{
    public interface IScheduleDetailRepository
    {
        // Lấy tất cả chi tiết lịch theo ScheduleId
        Task<List<ScheduleDetailMapper>> GetAllScheduleDetailsByScheduleId(int scheduleId);

        // Cập nhật ScheduleDetail theo Id
        Task UpdateScheduleDetailById(int id, ScheduleDetailMapper updatedDetail);

        // Thêm mới một ScheduleDetail
        Task AddScheduleDetail(ScheduleDetailMapper newDetail);
        Task AddScheduleDetails(List<ScheduleDetailMapper> newDetails); 
 

    }
}
