using BusinessObject.DTOS;
using Microsoft.AspNetCore.Mvc;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SemesterController : ControllerBase
    {
        private readonly ISemesterRepository _semesterRepository;

        public SemesterController(ISemesterRepository semesterRepository)
        {
            _semesterRepository = semesterRepository;
        }

        [HttpGet("GetAllSemester")]
        public async Task<ActionResult<List<SemesterMapper>>> GetAllSemesters()
        {
            try
            {
                var semesters = await _semesterRepository.GetAllSemester();
                return Ok(semesters);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpPost("AddSemester")]
        public async Task<ActionResult<int>> AddSemester([FromBody] SemesterMapper semesterMapper)
        {
            try
            {
                var semesterId = await _semesterRepository.AddSemester(semesterMapper);
                return Ok(new { message = "Add new SchoolYears Successfull." });
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpPut("UpdateSemester")]
        public async Task<IActionResult> UpdateSemester([FromBody] SemesterMapper semesterMapper)
        {
            try
            {
                await _semesterRepository.UpdateSemester(semesterMapper);
                return Ok("School Year Updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("AddSemesterReal/{schoolYearsID}")]
        public async Task<ActionResult<int>> AddSemesterReal(int schoolYearsID, [FromBody] Semesterreal1 semesterMapper)
        {
            try
            {
                var semesterId = await _semesterRepository.AddSemesterReal(schoolYearsID, semesterMapper);
                return Ok(new { message = "Add New Semester Successfull." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("UpdateSemesterReal/{schoolYearsID}")]
        public async Task<IActionResult> UpdateSemesterReal( int schoolYearsID, [FromBody] Semesterreal1 semesterMapper)
        {
            try
            {
                await _semesterRepository.UpdateSemesterReal(schoolYearsID, semesterMapper);
                return Ok("Semester updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("GetListSemesterBySchoolYear/{schoolYearsID}")]
        public async Task<ActionResult<List<Semesterreal1>>> GetListSemesterBySchoolYear(int schoolYearsID)
        {
            try
            {
                var semesters = await _semesterRepository.GetListSemesterBySchoolYear(schoolYearsID);
                return Ok(semesters);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("DeleteSemesterReal/{id}")]
        public async Task<IActionResult> DeleteSemesterReal(int id)
        {
            try
            {
                await _semesterRepository.DeleteSemester(id);
                return Ok("Semester deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
