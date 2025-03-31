using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class ScheduleDetailRepository : IScheduleDetailRepository
    {
        private readonly kmsContext _context;
        private readonly ScheduleDetailDAO _scheduleDetailDAO;

        public ScheduleDetailRepository(kmsContext context, ScheduleDetailDAO scheduleDetailDAO)
        {
            _context = context;
            _scheduleDetailDAO = scheduleDetailDAO;
        }

        // Lấy tất cả chi tiết lịch theo ScheduleId
        public async Task<List<ScheduleDetailMapper>> GetAllScheduleDetailsByScheduleId(int scheduleId)
        {
            try
            {
                return await _scheduleDetailDAO.GetAllScheduleDetailsByScheduleId(scheduleId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        // Cập nhật ScheduleDetail theo Id
        public async Task UpdateScheduleDetailById(int id, ScheduleDetailMapper updatedDetail)
        {
            try
            {
                // Gọi hàm Update từ DAO
                await _scheduleDetailDAO.UpdateScheduleDetailById(id, updatedDetail);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating schedule detail by ID: " + ex.Message, ex);
            }
        }

        // Thêm mới một ScheduleDetail
        public async Task AddScheduleDetail(ScheduleDetailMapper newDetail)
        {
            try
            {
                // Gọi hàm thêm mới từ DAO
                await _scheduleDetailDAO.AddScheduleDetail(newDetail);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new schedule detail: " + ex.Message, ex);
            }
        }
        public async Task AddScheduleDetails(List<ScheduleDetailMapper> newDetails)
        {
            try
            {
                // Gọi hàm thêm danh sách từ DAO
                await _scheduleDetailDAO.AddScheduleDetails(newDetails);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding schedule details: " + ex.Message, ex);
            }
        }
        
    }
}
