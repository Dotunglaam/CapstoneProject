using BusinessObject.DTOS;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Respository.Interfaces;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleDetailController : ControllerBase
    {
        private readonly IScheduleDetailRepository _scheduleDetailRepository;

        public ScheduleDetailController(IScheduleDetailRepository scheduleDetailRepository)
        {
            _scheduleDetailRepository = scheduleDetailRepository;
        }

        // Update ScheduleDetail by Id
        [HttpPut("UpdateScheduleDetailById/{id}")]
        public async Task<IActionResult> UpdateScheduleDetailById(int id, [FromBody] ScheduleDetailMapper scheduleDetailMapper)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (scheduleDetailMapper == null)
                {
                    return BadRequest(new { message = "Schedule detail cannot be null." });
                }

                await _scheduleDetailRepository.UpdateScheduleDetailById(id, scheduleDetailMapper);
                return Ok(new { message = "Schedule detail updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while updating the schedule detail by ID.",
                    details = ex.Message
                });
            }
        }

        // Add new ScheduleDetail
        [HttpPost("AddScheduleDetail")]
        public async Task<IActionResult> AddScheduleDetail([FromBody] ScheduleDetailMapper newScheduleDetailMapper)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (newScheduleDetailMapper == null)
                {
                    return BadRequest(new { message = "New schedule detail cannot be null." });
                }

                await _scheduleDetailRepository.AddScheduleDetail(newScheduleDetailMapper);
                return Ok(new { message = "Schedule detail added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while adding the new schedule detail.",
                    details = ex.Message
                });
            }
        }

        // Lấy tất cả chi tiết lịch theo ScheduleId
        [HttpGet("GetAllScheduleDetailsByScheduleId/{scheduleId}")]
        public async Task<IActionResult> GetAllScheduleDetailsByScheduleId(int scheduleId)
        {
            try
            {
                var result = await _scheduleDetailRepository.GetAllScheduleDetailsByScheduleId(scheduleId);
              
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the schedule details.", details = ex.Message });
            }
        }
        // Add list of ScheduleDetails
        [HttpPost("AddListScheduleDetails")]
        public async Task<IActionResult> AddListScheduleDetails([FromBody] List<ScheduleDetailMapper> newScheduleDetailMappers)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (newScheduleDetailMappers == null || newScheduleDetailMappers.Count == 0)
                {
                    return BadRequest(new { message = "List of schedule details cannot be null or empty." });
                }

                await _scheduleDetailRepository.AddScheduleDetails(newScheduleDetailMappers);
                return Ok(new { message = "Schedule details added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while adding the schedule details.",
                    details = ex.Message
                });
            }
        }

    }
}
