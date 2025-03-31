using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Respository.Interfaces;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChildrenController  :  ControllerBase
    {
        public IChildrenRespository _iChildrenRespository;
        public ChildrenController(IChildrenRespository iChildrenRespository)
        {
            _iChildrenRespository = iChildrenRespository;
        }

        [HttpGet("GetAllChildren")]
        public async Task<List<ChildrenClassMapper>> GetAllChildren()
        {
            try
            {
                var result = await _iChildrenRespository.GetAllChildren();
                return result;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("GetChildrenByChildrenId/{id}")]
        public async Task<ChildrenClassMapper> GetChildrenByChildrenId(int id)
        {
            try
            {
                var result = await _iChildrenRespository.GetChildrenByChildrenId(id);
                return result;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("GetChildrensWithoutClassByClassId/{classId}")]
        public async Task<List<ChildrenMapper>> GetChildrensWithoutClass(int classId)
        {
            try
            {
                var result = await _iChildrenRespository.GetChildrensWithoutClass(classId);
                return result;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost("ImportChildrenExcel")]
        public async Task<IActionResult> ImportChildrenExcel(IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                await _iChildrenRespository.ImportChildrenExcel(file);
                return Ok(new { message = "Children added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("AddChildrenToClassesFromExcel")]
        public async Task<IActionResult> AddChildrenToClassesFromExcel(IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                await _iChildrenRespository.AddChildrenToClassesFromExcel(file);
                return Ok(new { message = "Children added to class successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        [HttpPost("AddChildren")]
        public async Task<IActionResult> AddChildren([FromBody] ChildrenMapper childrenMapper)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var studentId = await _iChildrenRespository.AddChildren(childrenMapper);

                return Ok(new { StudentId = studentId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the child.", details = ex.Message });
            }
        }

        [HttpPost("AddChildrenImage")]
        public async Task<IActionResult> AddChildrenImage(int studentId, IFormFile image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                await _iChildrenRespository.AddChildrenImage(studentId, image);
                return Ok(new { message = "Children added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("AddChildToClass")]
        public async Task<IActionResult> AddChildToClass(int classId, int studentId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                await _iChildrenRespository.AddChildToClass(classId, studentId);
                return Ok(new { message = "Children added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("ExportChildrenWithoutClassToExcel")]
        public async Task<IActionResult> ExportChildrenWithoutClassToExcel(int classId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request data." });
            }

            try
            {
                var filePath = await _iChildrenRespository.ExportChildrenWithoutClassToExcel(classId);

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                System.IO.File.Delete(filePath);

                return File(fileBytes,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"ChildrenWithoutClass_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error exporting file: {ex.Message}" });
            }
        }

        [HttpPut("UpdateChildren")]
        public async Task<IActionResult> UpdateChildren([FromBody] ChildrenMapper childrenMapper)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var existingProduct = await _iChildrenRespository.GetChildrenByChildrenId(childrenMapper.StudentId);
                if (existingProduct == null)
                {
                    return NotFound(new { message = "Children not found." });
                }

                await _iChildrenRespository.UpdateChildren(childrenMapper);
                return Ok(new { message = "Children updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("DeleteChildren/{id}")]
        public async Task<IActionResult> DeleteChildren(int id)
        {
            try
            {
                var product = await _iChildrenRespository.GetChildrenByChildrenId(id);
                if (product == null)
                {
                    return NotFound(new { message = "Children not found." });
                }

                await _iChildrenRespository.DeleteChildren(id);
                return Ok(new { message = "Children deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
