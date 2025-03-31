using BusinessObject.DTOS;
using System.Threading.Tasks;

namespace Respository.Interfaces
{
    public interface IDashboardRepository
    {
        Task<List<EnrollmentStatisticsDTO>> GetEnrollmentStatisticsAsync();
        Task<TeacherWithTotalDTO> GetTeacherWithTotalAsync();
        Task<FinancialSummaryDTO> GetFinancialSummaryForYearAsync(int year);
        Task<List<AccountmentStatisticsDTO>> GetAccountmentStatisticsAsync();
    }
}
