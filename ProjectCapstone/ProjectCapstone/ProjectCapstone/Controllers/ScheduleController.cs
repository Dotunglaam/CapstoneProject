using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static BusinessObject.DTOS.Request;
using Respository.Interfaces;
using Respository.Services;
using Repository.Interfaces;
using Microsoft.AspNetCore.OData.Query;
using BusinessObject.DTOS;
using Microsoft.EntityFrameworkCore;
namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleRepository _scheduleService;

        public ScheduleController(IScheduleRepository scheduleService)
        {
            _scheduleService = scheduleService;
        }
        [HttpPost("AddSchedule")]
        public async Task<IActionResult> AddSchedule([FromBody] ScheduleMapper scheduleMapper)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _scheduleService.AddSchedule(scheduleMapper);
                return Ok(new { message = "Schedule added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while adding the schedule.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("GetAllSchedules")]
        public async Task<IActionResult> GetAllSchedules()
        {
            try
            {
                var result = await _scheduleService.GetAllScheduleMappers();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while retrieving schedules.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("GetScheduleById/{id}")]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            try
            {
                var result = await _scheduleService.GetScheduleById(id);
                if (result == null)
                {
                    return NotFound(new { message = "Schedule not found." });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while retrieving the schedule.",
                    details = ex.Message
                });
            }
        }


        [HttpPut("UpdateSchedule")]
        public async Task<IActionResult> UpdateSchedule([FromBody] ScheduleMapper scheduleMapper)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var existingSchedule = await _scheduleService.GetScheduleById(scheduleMapper.ScheduleId);
                if (existingSchedule == null)
                {
                    return NotFound(new { message = "Schedule not found." });
                }

                await _scheduleService.UpdateSchedule(scheduleMapper);
                return Ok(new { message = "Schedule updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the schedule.", details = ex.Message });
            }
        }
    
        [HttpGet("GetSchedulesByClassId")]
        public async Task<IActionResult> GetSchedulesByClassId(int classId)
        {
            try
            {
                var result = await _scheduleService.GetSchedulesByClassId(classId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving schedules.", details = ex.Message });
            }
        }

        [HttpPost("Import/{ClassID}")]
        public async Task<IActionResult> ImportSchedules(IFormFile file , int ClassID)
        {
            // Validate ModelState to ensure that all required data is correct before processing.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return a 400 Bad Request response with model validation errors.
            }

            // Check if the file is null or empty, indicating that no file was uploaded.
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded."); // Return a 400 Bad Request response with a message indicating the issue.
            }

            try
            {
                // Call the service to process the uploaded Excel file and import schedule details.
                await _scheduleService.ImportSchedulesAsync(file, ClassID);

                // If import is successful, return a 200 OK response with a success message.
                return Ok(new { message = "Schedule added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }




        // Lấy tất cả địa điểm
        [HttpGet("GetAllLocations")]
        public async Task<IActionResult> GetAllLocations()
        {
            try
            {
                var result = await _scheduleService.GetAllLocationMappers();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while retrieving locations.",
                    details = ex.Message
                });
            }
        }

        // Lấy tất cả hoạt động
        [HttpGet("GetAllActivities")]
        public async Task<IActionResult> GetAllActivities()
        {
            try
            {
                var result = await _scheduleService.GetAllActivityMappers();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while retrieving activities.",
                    details = ex.Message
                });
            }
        }


    }
}
