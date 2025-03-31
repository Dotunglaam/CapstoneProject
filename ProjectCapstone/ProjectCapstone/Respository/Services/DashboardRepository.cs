using BusinessObject.DTOS;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Respository.Interfaces;
using System;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DashBoardPrincipleDAO _classDAO;

        public DashboardRepository(DashBoardPrincipleDAO classDAO)
        {
            _classDAO = classDAO;
        }

        // Get the enrollment statistics
        public async Task<List<EnrollmentStatisticsDTO>> GetEnrollmentStatisticsAsync()
        {
            try
            {
                return await _classDAO.GetEnrollmentStatisticsAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the enrollment statistics.", ex);
            }
        }

        // Get total number of teachers and their details
        public async Task<TeacherWithTotalDTO> GetTeacherWithTotalAsync()
        {
            try
            {
                return await _classDAO.GetTeacherWithTotalAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the teacher data.", ex);
            }
        }

        // Get financial summary for the specified year
        public async Task<FinancialSummaryDTO> GetFinancialSummaryForYearAsync(int year)
        {
            try
            {
                return await _classDAO.GetFinancialSummaryForYearAsync(year);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while retrieving financial summary: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw new Exception("An error occurred while retrieving the financial summary.", ex);
            }
        }
        public async Task<List<AccountmentStatisticsDTO>> GetAccountmentStatisticsAsync()
        {
            try
            {
                return await _classDAO.GetAccountmentStatisticsAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the enrollment statistics.", ex);
            }
        }
    }
}
