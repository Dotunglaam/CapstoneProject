using BusinessObject.DTOS;
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
    public class RequestController : ControllerBase
    {
        private readonly IRequestRepository _requestRepository;

        public RequestController(IRequestRepository requestRepository)
        {
            _requestRepository = requestRepository;
        }

        // Lấy tất cả các yêu cầu
        [HttpGet("GetAllRequests")]
        public async Task<IActionResult> GetAllRequests()
        {
            try
            {
                var result = await _requestRepository.GetAllRequests();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving requests.", details = ex.Message });
            }
        }

        // Lấy yêu cầu theo ID
        [HttpGet("GetRequestById/{requestId}")]
        public async Task<IActionResult> GetRequestById(int requestId)
        {
            try
            {
                var result = await _requestRepository.GetRequestById(requestId);
                if (result == null)
                {
                    return NotFound(new { message = "Request not found." });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the request.", details = ex.Message });
            }
        }

        // Thêm yêu cầu mới
        [HttpPost("AddRequest")]
        public async Task<IActionResult> AddRequest([FromBody] RequestMapper requestMapper)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                requestMapper.StatusRequest = 1;

                await _requestRepository.AddRequest(requestMapper);
                return Ok(new { message = "Request added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while adding the request.", details = ex.Message });
            }
        }

        // Cập nhật yêu cầu
        [HttpPut("UpdateRequest")]
        public async Task<IActionResult> UpdateRequest([FromBody] RequestMapper requestMapper)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var existingRequest = await _requestRepository.GetRequestById(requestMapper.RequestId);
                if (existingRequest == null)
                {
                    return NotFound(new { message = "Request not found." });
                }

                await _requestRepository.UpdateRequest(requestMapper);
                return Ok(new { message = "Request updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the request.", details = ex.Message });
            }
        }
        [HttpGet("GetStudentsByParentId/{parentId}")]
        public async Task<IActionResult> GetStudentsByParentId(int parentId)
        {
            try
            {
                var childrenList = await _requestRepository.GetStudentsByParentId(parentId);
                if (childrenList == null || !childrenList.Any())
                {
                    return NotFound(new { message = "No children found for parent." });
                }
                return Ok(childrenList);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("UpdateClassIdForStudent/{studentId}/{newClassId}")]
        public async Task<IActionResult> UpdateClassIdForStudent(int studentId, int newClassId)
        {

            try
            {
                await _requestRepository.UpdateStudentClassId(studentId, newClassId);
                return Ok(new { message = "ClassId updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the ClassId.", details = ex.Message });
            }
        }

    }
}
