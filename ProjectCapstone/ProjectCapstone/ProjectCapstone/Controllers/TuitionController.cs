using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Respository.Interfaces;
using Respository.Services;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TuitionController : ControllerBase
    {
        private readonly ITuitionService _tuitionService;
        private readonly kmsContext _context;

        public TuitionController(ITuitionService tuitionService, kmsContext kmsContext)
        {
           _tuitionService = tuitionService;
            _context = kmsContext;
        }

        [HttpGet("parent/{parentId}")]
        public async Task<IActionResult> GetTuitionRecordsByParentId(int parentId)
        {
            var result = await _tuitionService.GetTuitionRecordsByParentIdAsync(parentId);

            return Ok(result);
        }

        [HttpPost("generate-tuition")]
        public async Task<IActionResult> GenerateTuitionManual()
        {
            try
            {
                var now = DateTime.Now;
                var nextMonth = now.AddMonths(1);
                // Lấy danh sách tất cả StudentId từ bảng Child
                var allStudentIds = await _context.Children
                    .Where(c => c.Status == 1) 
                    .Select(c => c.StudentId)   
                    .ToListAsync();

                // Lấy danh sách StudentId có bản ghi trong bảng Tuition cho tháng tiếp theo
                var tuitionStudentIds = await _context.Tuitions
                    .Where(t => t.StartDate.Value.Year == nextMonth.Year &&
                                t.StartDate.Value.Month == nextMonth.Month)
                    .Select(t => t.StudentId)
                    .ToListAsync();
                var currentSemester = await _context.Semesters
                    .Where(s => s.StartDate.Value.AddMonths(-1) <= now && s.EndDate >= now)
                    .OrderByDescending(s => s.StartDate)
                    .FirstOrDefaultAsync();
                if (currentSemester == null)
                {
                    return BadRequest(new
                    {
                        message = "No semester available, cannot create tuition fees!"
                    });
                }
                if (nextMonth > currentSemester.EndDate.Value)
                {
                    return BadRequest(new
                    {
                        message = currentSemester.Name + "  ends next month, cannot create tuition fees."
                    });
                }
                // So sánh: Nếu tất cả học sinh đã có học phí, thông báo không tạo lại
                var missingStudentIds = allStudentIds.Except(tuitionStudentIds).ToList();
                if (!missingStudentIds.Any())
                {
                    return BadRequest(new { message = "The tuition fee for the next month already exists. Cannot create again."
                    });
                }

                // Gọi service tạo học phí (bypass kiểm tra ngày cuối tháng)
                var record = await _tuitionService.GenerateMonthlyTuitionRecordsClick(true);

                return Ok(record);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("GetAllTuitionsCheckGene")]
        public async Task<IActionResult> GetAllTuitions()
        {
            var result = await _tuitionService.GetAllTuitionsAsync();

            if (result == null || !result.Any())
            {
                return NotFound("No tuition records found.");
            }

            return Ok(result);
        }

        [HttpPost("approve-and-send-email")]
        public async Task<IActionResult> ApproveAndSendEmail()
        {
            try
            {
                var tuitionRecord = await _context.Tuitions
                .Include(t => t.Student)
                .Include(t => t.Student.Parent)
                .ThenInclude(x => x.ParentNavigation)
                .Where(x => x.IsPaid == 0 && x.SendMailByPr == 0)
                .ToListAsync();

                if (!tuitionRecord.Any())
                {
                    return BadRequest("Not Found Record Tuitions");
                }
                if (tuitionRecord.All(t => t.SendMailByPr == 1))
                {
                    return BadRequest("You realy send mail for all, just one sent mail!");
                }
                await _tuitionService.ApproveAndSendEmailsForAllTuitions();

                return Ok("Email notifications for all students have been sent successfully.");
            }
            catch (Exception ex)
            {
                // Return error response if something goes wrong
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("SendTuitionReminder")]
        public async Task<IActionResult> SendTuitionReminder()
        {
            var result = await _tuitionService.SendTuitionReminderAsync(true);
            return Ok(result);
        }
        [HttpDelete("deleteTuitionFalse")]
        public async Task<IActionResult> DeleteUnpaidTuitions()
        {
            try
            {
                // Gọi service để xóa các bản ghi Tuition có IsPaid = 0
                var deletedCount = await _tuitionService.DeleteUnpaidTuitionsAsync();

                // Nếu không có bản ghi nào được xóa, trả về thông báo thích hợp
                if (deletedCount == 0)
                {
                    return NotFound(new { message = "No unpaid tuition records found to delete." });
                }

                return Ok(new { message = $"{deletedCount} unpaid tuition records have been deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting records.", error = ex.Message });
            }
        }
    }
}
