using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.AspNetCore.Mvc;
using Respository.Interfaces;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradeController : Controller
    {
        private readonly GradeDAO _semesterDAO;
        private readonly IGradeRepository _gradeRepository;

        public GradeController(GradeDAO semesterDAO, IGradeRepository gradeRepository)
        {
            _semesterDAO = semesterDAO;
            _gradeRepository = gradeRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<GradeDAO>>> GetAllGrade()
        {
            try
            {
                var semesters = await _semesterDAO.GetAllGrades();
                return Ok(semesters);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error retrieving semesters: " + ex.Message);
            }
        }

        [HttpGet("GetGradeById/{id}")]
        public async Task<IActionResult> GetGradeById(int id)
        {
            var grade = await _gradeRepository.GetGradeByIdAsync(id);
            if (grade == null)
            {
                return NotFound("No Find grade !");
            }
            return Ok(grade);
        }

        // Add a new grade
        [HttpPost("AddGrade")]
        public async Task<IActionResult> AddGrade([FromBody] GradeModelDTO grade)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var addedGrade = await _gradeRepository.AddGradeAsync(grade);
                if (grade.BaseTuitionFee <= 0)
                {
                    return BadRequest(new { message = "Base Tuition Fee must be greater than 0." });
                }
                return CreatedAtAction(nameof(GetGradeById), new { id = addedGrade.GradeId }, addedGrade);

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Update a grade
        [HttpPut("UpdateGrade")]
        public async Task<IActionResult> UpdateGrade([FromBody] GradeModelDTO grade)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (grade.GradeId != grade.GradeId)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            try
            {
                var updatedGrade = await _gradeRepository.UpdateGradeAsync(grade);
                if (updatedGrade == null)
                {
                    return NotFound(new { message = "Grade not found." });
                }
                return Ok(new { message = "Grade updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("DeleteGrade/{id}")]
        public async Task<IActionResult> SoftDeleteGrade(int id)
        {
            try
            {
                var result = await _gradeRepository.SoftDeleteGradeAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Grade not found." });
                }

                return Ok(new { message = "Grade has been successfully soft-deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

    }
}
