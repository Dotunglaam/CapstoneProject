using BusinessObject.DTOS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceRepository _serviceRepository;

        public ServiceController(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        // GET: api/Service/GetAllServices
        [HttpGet("GetAllServices")]
        public async Task<IActionResult> GetAllServices()
        {
            try
            {
                var services = await _serviceRepository.GetAllServices();
                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Error retrieving services: {ex.Message}" });
            }
        }

        // GET: api/Service/GetServiceById/{id}
        [HttpGet("GetServiceById/{id}")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            try
            {
                var service = await _serviceRepository.GetServiceById(id);
                if (service == null)
                {
                    return NotFound(new { message = $"Service with ID {id} not found." });
                }
                return Ok(service);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Error retrieving service: {ex.Message}" });
            }
        }

        // POST: api/Service/AddService
        [HttpPost("AddService")]
        public async Task<IActionResult> AddService([FromBody] ServiceMapper1 serviceMapper)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _serviceRepository.AddService(serviceMapper);
                return CreatedAtAction(nameof(GetServiceById), new { id = serviceMapper.ServiceId }, new { message = "Service added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Error adding service: {ex.Message}" });
            }
        }

        // PUT: api/Service/UpdateService
        [HttpPut("UpdateService")]
        public async Task<IActionResult> UpdateService([FromBody] ServiceMapper1 serviceMapper)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingService = await _serviceRepository.GetServiceById(serviceMapper.ServiceId);
                if (existingService == null)
                {
                    return NotFound(new { message = $"Service with ID {serviceMapper.ServiceId} not found." });
                }

                await _serviceRepository.UpdateService(serviceMapper);
                return Ok(new { message = "Service updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Error updating service: {ex.Message}" });
            }
        }

        [HttpPost("AddChildService")]
        public async Task<IActionResult> AddChildService([FromBody] ChildrenHasServicesMapper childrenServiceMapper)
        {
            if (childrenServiceMapper == null) return BadRequest("Invalid data.");

            try
            {
                await _serviceRepository.AddChildService(childrenServiceMapper);
                return Ok("Child service added successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("UpdateChildService")]
        public async Task<IActionResult> UpdateChildService([FromBody] ChildrenHasServicesMapper childrenServiceMapper)
        {
            if (childrenServiceMapper == null) return BadRequest("Invalid data.");
            try
            {
                await _serviceRepository.UpdateChildService(childrenServiceMapper);
                return Ok("Child service updated successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("AddCheckService")]
        public async Task<IActionResult> AddCheckService([FromBody] CheckservicesMapper checkServiceMapper)
        {
            if (checkServiceMapper == null) return BadRequest("Invalid data.");

            try
            {
                await _serviceRepository.AddCheckService(checkServiceMapper);
                return Ok("Check service added successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPut("UpdateCheckService")]
        public async Task<IActionResult> UpdateCheckService([FromBody] CheckservicesMapper checkServiceMapper)
        {
            if (checkServiceMapper == null) return BadRequest("Invalid data.");

            try
            {
                await _serviceRepository.UpdateCheckService(checkServiceMapper);
                return Ok("Check service updated successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("GetCheckServiceByStudentIdAndDate/{studentId}/{date}")]
        public async Task<IActionResult> GetCheckServiceByStudentIdAndDate(int studentId, string date)
        {
            DateOnly parsedDate;
            if (!DateOnly.TryParse(date, out parsedDate))
            {
                return BadRequest("Invalid date format.");
            }

            var result = await _serviceRepository.GetCheckServiceByStudentIdAndDate(studentId, parsedDate);

            if (result == null)
            {
                return NotFound();
            }
            return Ok(result); 
        }

        [HttpGet("GetCheckServiceByStudentIdAndWeek/{studentId}/{startDate}/{endDate}")]
        public async Task<IActionResult> GetCheckServiceByStudentIdAndWeek(int studentId, string  startDate, string endDate)
        {
            DateOnly parsedstDate, parsedenDate;
            if (!DateOnly.TryParse(startDate, out parsedstDate))
            {
                return BadRequest("Invalid date format.");
            }
            if (!DateOnly.TryParse(endDate, out parsedenDate))
            {
                return BadRequest("Invalid date format.");
            }
            var result = await _serviceRepository.GetCheckServiceByStudentIdAndWeek(studentId, parsedstDate, parsedenDate);

            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("GetChildrenHasServiceByStudentId/{studentId}")]
        public async Task<IActionResult> GetChildrenHasServiceByStudentId(int studentId)
        {
            try
            {
                var childrenServices = await _serviceRepository.GetChildrenHasServiceByStudentId(studentId);
                if (childrenServices == null )
                {
                    return NotFound(new { message = $"No child services found for Student ID {studentId}." });
                }
                return Ok(childrenServices);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}
