using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Respository.Interfaces;
using Respository.Services;
using System;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PickupPersonController : ControllerBase
    {
        private readonly IPickupPersonRepository _pickupPersonRepository;
        private readonly kmsContext _context;

        public PickupPersonController(kmsContext dbContext, IPickupPersonRepository pickupPersonRepository)
        {
            _context = dbContext;
            _pickupPersonRepository = pickupPersonRepository;
        }

        [HttpGet("GetPickupPersonInfoByUUI")]
        public async Task<IActionResult> GetPickupPersonInfoByUUIDAsync(string uuid)
        {
            try
            {
                var result = await _pickupPersonRepository.GetPickupPersonInfoByUUIDAsync(uuid);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while get pickup person .", details = ex.Message });
            }
        }

        [HttpGet("GetPickupPersonInfoByParentId")]
        public async Task<IActionResult> GetPickupPersonInfoByParentIdAsync(int parentId)
        {
            try
            {
                var result = await _pickupPersonRepository.GetPickupPersonInfoByParentIdAsync(parentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while get pickup person .", details = ex.Message });
            }
        }

        [HttpGet("GetPickupPersonInfoByStudentId")]
        public async Task<IActionResult> GetPickupPersonInfoByStudentIdAsync(int studentId)
        {
            try
            {
                var result = await _pickupPersonRepository.GetPickupPersonInfoByStudentIdAsync(studentId);

                if (result == null || !result.Any())
                {
                    return NotFound(new { message = "No pickup person found for the given student ID." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while retrieving pickup person information.",
                    details = ex.Message
                });
            }
        }

        [HttpPost("AddPickupPerson")]
        public async Task<IActionResult> AddPickupPersonAsync([FromQuery] string name, [FromQuery] string phoneNumber, [FromQuery] int parentId, IFormFile photo)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                await photo.CopyToAsync(memoryStream);
                var photoData = memoryStream.ToArray();

                await _pickupPersonRepository.AddPickupPersonAsync(name, phoneNumber, parentId, photoData);
                return Ok(new { message = "Add pickup person successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while add pickup person .", details = ex.Message });
            }
        }

        [HttpDelete("DeletePickupPerson/{pickupPersonId}")]
        public async Task<IActionResult> DeletePickupPersonByIdAsync(int pickupPersonId)
        {
            try
            {
                var pickupPerson = await _context.PickupPeople
                .Include(pp => pp.Students)
                .FirstOrDefaultAsync(pp => pp.PickupPersonId == pickupPersonId);

                if (pickupPerson == null)
                {
                    return NotFound(new { Message = "PickupPerson not found." });
                }

                await _pickupPersonRepository.DeletePickupPersonByIdAsync(pickupPersonId);
                return Ok(new { Message = "PickupPerson deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while add pickup person .", details = ex.Message });
            }
        }

    }


}
