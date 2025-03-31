using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Respository.Interfaces;
using Respository.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IClassRespository _classRepository;

        public ClassController(IClassRespository classRepository)
        {
            _classRepository = classRepository;
        }

        // Lấy tất cả các lớp
        [HttpGet("GetAllClass")]
        public async Task<IActionResult> GetAllClass()
        {
            try
            {
                var result = await _classRepository.GetAllClass();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving classes.", details = ex.Message });
            }
        }

        [HttpGet("GetClassById/{id}")]
        public async Task<IActionResult> GetClassById(int id)
        {
            try
            {
                var result = await _classRepository.GetClassById(id);
                if (result == null)
                {
                    return NotFound(new { message = "Class not found." });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the class.", details = ex.Message });
            }
        }
        [HttpPost("AddClass")]
        public async Task<IActionResult> AddClass([FromBody] ClassMapper classMapper)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                classMapper.Status = 0;

                // Lưu lớp mới và nhận lại ID
                int newClassId = await _classRepository.AddClass(classMapper);
                return Ok(new { message = "Thêm lớp thành công.", classId = newClassId });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while adding the class.", details = ex.Message });
            }
        }
        [HttpPut("UpdateClass")]
        public async Task<IActionResult> UpdateClass([FromBody] ClassMapper classMapper  )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var existingClass = await _classRepository.GetClassById(classMapper.ClassId);
                if (existingClass == null)
                {
                    return NotFound(new { message = "Class not found." });
                }

                await _classRepository.UpdateClass(classMapper);
                return Ok(new { message = "Class updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the class.", details = ex.Message });
            }
        }
        [HttpPut("UpdateStatusClass/{classId}/{newStatus}")]
        public async Task<IActionResult> UpdateStatusClass(int classId, int newStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                await _classRepository.UpdateStatusClass(classId, newStatus);
                return Ok(new { message = "Class updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the class.", details = ex.Message });
            }
        }
        [HttpPut("UpdateTeacherToClass/{classId}/{currentTeacherId}/{newTeacherId}")]
        public async Task<IActionResult> UpdateTeacherToClass(int classId, int currentTeacherId, int newTeacherId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                await _classRepository.UpdateTeacherToClass(classId, currentTeacherId, newTeacherId);
                return Ok(new { message = "Class updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the class.", details = ex.Message });
            }
        }

        [HttpGet("GetChildrenByClassId/{classId}")]
        public async Task<IActionResult> GetChildrenByClassId(int classId)
        {
            try
            {
                var childrenList = await _classRepository.GetChildrenByClassId(classId);
                if (childrenList == null || !childrenList.Any())
                {
                    return NotFound(new { message = "No children found for this class." });
                }
                return Ok(childrenList);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetClassesByStudentId/{studentId}")]
        public async Task<IActionResult> GetClassesByStudentId(int studentId)
        {
            try
            {
                var classList = await _classRepository.GetClasssByStudentId(studentId);
                if (classList == null || !classList.Any())
                {
                    return NotFound(new { message = "No classes found for this student." });
                }
                return Ok(classList);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving classes.", details = ex.Message });
            }
        }

        [HttpPost("AddTeacherToClass")]
        public async Task<IActionResult> AddTeacherToClass(int classId, int teacherId)
        {
            try
            {
                await _classRepository.AddTeacherToClass(classId, teacherId);
                return Ok(new { message = "Add Teacher To Class success ." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Lỗi khi thêm giáo viên vào lớp.", details = ex.Message });
            }
        }
        [HttpPost("RemoveTeacherFromClass")]
        public async Task<IActionResult> RemoveTeacherFromClass(int classId, int teacherId)
        {
            try
            {
                await _classRepository.RemoveTeacherFromClass(classId, teacherId);
                return Ok(new { message = "Teacher removed from class successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error removing teacher from class.", details = ex.Message });
            }
        }

        [HttpGet("GetClassesByTeacherId/{teacherId}")]
        public async Task<IActionResult> GetClassesByTeacherId(int teacherId)
        {
            try
            {
                var classes = await _classRepository.GetClassesByTeacherId(teacherId);
                if (classes == null || !classes.Any())
                {
                    return NotFound(new { message = "No classes found for this teacher." });
                }
                return Ok(classes);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving classes.", details = ex.Message });
            }
        }
        // Endpoint to send mail to parents by class ID  
        [HttpPost("SendMailToParentsByClassId/{classId}")]
        public async Task<IActionResult> SendMailToParentsByClassId(int classId)
        {
            try
            {
                // Call the service to send emails  
                await _classRepository.SendMailToParentsByClassId(classId);
                return Ok(new { message = "Email đã được gửi tới phụ huynh." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  new { message = "Lỗi khi gửi email: " + ex.Message });
            }
        }
        [HttpGet("GetTeachersWithoutClass")]
        public async Task<IActionResult> GetTeachersWithoutClass()
        {
            try
            {
                var teachersWithoutClass = await _classRepository.GetTeachersWithoutClass();
                if (teachersWithoutClass == null || !teachersWithoutClass.Any())
                {
                    return NotFound(new { message = "No teachers found without a class." });
                }
                return Ok(teachersWithoutClass);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving teachers without class.", details = ex.Message });
            }
        }

        [HttpPost("ImportClassExcel")]
        public async Task<IActionResult> ImportClassExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File không hợp lệ.");
            }

            try
            {
                var result = await _classRepository.ImportClassExcel(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi import file Excel: {ex.Message}");
            }
        }
        [HttpPut("UpdateHomeroomTeacher/{classId}/{teacherId}")]
        public async Task<IActionResult> UpdateHomeroomTeacher(int classId, int teacherId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                await _classRepository.UpdateHomeroomTeacher(classId, teacherId);
                return Ok(new { message = "HomeroomTeacher updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the class.", details = ex.Message });
            }
        }

    }
}
