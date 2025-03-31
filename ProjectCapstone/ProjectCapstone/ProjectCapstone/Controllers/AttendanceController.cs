using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Respository.Interfaces;
using Respository.Services;
using CloudinaryDotNet;
using System.Collections.Generic;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        public IAttendanceRepository _iAttendanceRepository;
        public AttendanceController(IAttendanceRepository iAttendanceRepository)
        {
            _iAttendanceRepository = iAttendanceRepository;
        }

        [HttpPost("CreateDailyCheckin")]
        public async Task<IActionResult> CreateDailyCheckin()
        {
            try
            {
                await _iAttendanceRepository.CreateDailyCheckin();
                return Ok("Daily check-in created successfully."); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("CreateDailyAttendance")]
        public async Task<IActionResult> CreateDailyAttendance()
        {
            try
            {
                await _iAttendanceRepository.CreateDailyAttendance();
                return Ok("Daily attend created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("CreateDailyCheckout")]
        public async Task<IActionResult> CreateDailyCheckout()
        {
            try
            {
                await _iAttendanceRepository.CreateDailyCheckout();
                return Ok("Daily check-out created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("UpdateAttendance")]
        public async Task<IActionResult> UpdateAttendance([FromBody] List<AttendanceMapper> attendanceMappers, int classId, string type)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var existingProduct = await _iAttendanceRepository.GetAttendanceByClassId(classId, type);
                if (existingProduct == null)
                {
                    return NotFound(new { message = "Attendance not found." });
                }

                await _iAttendanceRepository.UpdateAttendance(attendanceMappers, classId,  type);
                return Ok(new { message = "Attendance updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("UpdateAttendanceByType")]
        public async Task<IActionResult> UpdateAttendanceByType([FromBody] List<AttendanceMapper> attendanceMappers, string type)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                await _iAttendanceRepository.UpdateAttendanceByType(attendanceMappers, type);
                return Ok(new { message = "Attendance updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("UploadAttendanceImages")]
        public async Task<IActionResult> UploadAttendanceImages([FromForm] int attendanceDetailID, [FromForm] List<IFormFile> images)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (images == null || !images.Any())
                {
                    return BadRequest("No images provided.");
                }

                var image = images.First();

                await _iAttendanceRepository.UploadImageAndSaveToDatabaseAsync(attendanceDetailID, image);
                return Ok(new { message = "Attendance images updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetAttendanceByStudentId")]
        public async Task<List<AttendanceMapper>> GetAttendanceByStudentId(int studentId, string type, DateTime date)
        {
            try
            {
                return await _iAttendanceRepository.GetAttendanceByStudentId(studentId, type, date);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        

        [HttpGet("GetAttendanceByDate")]
        public async Task<List<AttendanceMapper>> GetAttendanceByDate(int classId, string type, DateTime date)
        {
            try
            {
                return await _iAttendanceRepository.GetAttendanceByDate(classId, type, date);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("GetAttendanceByTypeAndDate")]
        public async Task<List<AttendanceMapper>> GetAttendanceByTypeAndDate(string type, DateTime date)
        {
            try
            {
                return await _iAttendanceRepository.GetAttendanceByTypeAndDate(type, date);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        [HttpGet("GetAttendanceByClassId")]
        public async Task<List<AttendanceMapper>> GetAttendanceByClassId(int classId, string type)
        {
            try
            {
                return await _iAttendanceRepository.GetAttendanceByClassId(classId, type);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

}
