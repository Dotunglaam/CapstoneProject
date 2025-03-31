using BusinessObject.DTOS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Respository.Interfaces;
using System;
using System.Threading.Tasks;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardController(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        // Endpoint to get enrollment statistics
        [HttpGet("GetEnrollmentStatistics")]
        public async Task<IActionResult> GetEnrollmentStatistics()
        {
            try
            {
                var enrollmentStatistics = await _dashboardRepository.GetEnrollmentStatisticsAsync();

                if (enrollmentStatistics == null)
                {
                    return NotFound(new { message = "No enrollment statistics found." });
                }

                return Ok(enrollmentStatistics);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving enrollment statistics.", details = ex.Message });
            }
        }

        // Endpoint to get total number of teachers and their details
        [HttpGet("GetTeacherWithTotal")]
        public async Task<IActionResult> GetTeacherWithTotal()
        {
            try
            {
                var teacherWithTotal = await _dashboardRepository.GetTeacherWithTotalAsync();

                if (teacherWithTotal == null)
                {
                    return NotFound(new { message = "No teacher data found." });
                }

                return Ok(teacherWithTotal);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving teacher data.", details = ex.Message });
            }
        }

        [HttpGet("GetFinancialSummaryForYear/{year}")]
        public async Task<IActionResult> GetFinancialSummaryForYear(int year)
        {
            try
            {
                var financialSummary = await _dashboardRepository.GetFinancialSummaryForYearAsync(year);

                if (financialSummary == null)
                {
                    return NotFound(new { message = $"No financial summary found for the year {year}." });
                }

                return Ok(financialSummary);
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi chi tiết để dễ dàng theo dõi
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                }

                // Trả về lỗi chi tiết hơn trong phản hồi
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while retrieving financial summary data.",
                    details = ex.Message,
                    stackTrace = ex.StackTrace, // Thêm stack trace để debug
                    innerException = ex.InnerException?.Message // Thêm thông tin lỗi bên trong (nếu có)
                });
            }
        }
        [HttpGet("GetAccountmentStatistics")]
        public async Task<IActionResult> GetAccountStatistics()
        {
            try
            {
                var enrollmentStatistics = await _dashboardRepository.GetAccountmentStatisticsAsync();

                if (enrollmentStatistics == null)
                {
                    return NotFound(new { message = "No enrollment statistics found." });
                }

                return Ok(enrollmentStatistics);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving enrollment statistics.", details = ex.Message });
            }
        }
    }
}
