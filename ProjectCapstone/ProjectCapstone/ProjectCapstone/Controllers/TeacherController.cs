using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Respository.Interfaces;
using Respository.Services;
using static BusinessObject.DTOS.Request;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherRepository _StaffRepository;

        public TeacherController(ITeacherRepository staffRepository)
        {
            _StaffRepository = staffRepository;
        }

        // Get all teachers
        [HttpGet("GetAllTeachers")]
        public async Task<IActionResult> GetAllTeachers()
        {
            var teachers = await _StaffRepository.GetAllTeachersAsync();
            return Ok(teachers);
        }

        // Get teacher by ID
        [HttpGet("GetTeacherById/{id}")]
        public async Task<IActionResult> GetTeacherById(int id)
        {
            var teacher = await _StaffRepository.GetTeacherByIdAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(teacher);
        }
        // Soft delete a teacher (mark inactive)

        [HttpDelete("SoftDeleteTeacher/{id}")]
        public async Task<IActionResult> SoftDeleteTeacher(int id)
        {
            var result = await _StaffRepository.SoftDeleteTeacherAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent(); 
        }

        [HttpPut("StaffUpdateProfileForTeacher")]
        public async Task<IActionResult> UpdateTeacher([FromBody] ViewProfileTeacherForTableTecher teacher)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Check if the teacher exists before updating
                var existingTeacher = await _StaffRepository.GetTeacherByIdAsync(teacher.TeacherId);
                if (existingTeacher == null)
                {
                    return NotFound(new { message = "Teacher not found." });
                }

                // Update teacher
                var updatedTeacher = await _StaffRepository.UpdateTeacherAsync(teacher);
                if (updatedTeacher != null)
                {
                    return Ok(new { message = "Teacher updated successfully." });
                }

                return NotFound(new { message = "Teacher update failed." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
