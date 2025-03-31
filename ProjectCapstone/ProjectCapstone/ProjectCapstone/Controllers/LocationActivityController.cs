using BusinessObject.DTOS;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationActivityController : ControllerBase
    {
        private readonly ILocationActivityRepository _locationActivityRepository;

        public LocationActivityController(ILocationActivityRepository locationActivityRepository)
        {
            _locationActivityRepository = locationActivityRepository;
        }

        // Location Endpoints
        [HttpGet("GetAllLocations")]
        public async Task<ActionResult<List<LocationMapper>>> GetAllLocations()
        {
            try
            {
                var locations = await _locationActivityRepository.GetAllLocations();
                return Ok(locations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("GetLocationById/{id}")]
        public async Task<ActionResult<LocationMapper>> GetLocationById(int id)
        {
            try
            {
                var location = await _locationActivityRepository.GetLocationById(id);
                if (location == null) return NotFound(new { message = "Location not found" });
                return Ok(location);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("AddLocation")]
        public async Task<IActionResult> AddLocation([FromBody] LocationMapper locationMapper)
        {
            try
            {
                await _locationActivityRepository.AddLocation(locationMapper);
                return Ok(new { message = "Location added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("UpdateLocation")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationMapper locationMapper)
        {
            try
            {
                await _locationActivityRepository.UpdateLocation(locationMapper);
                return Ok(new { message = "Location updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Activity Endpoints
        [HttpGet("GetAllActivities")]
        public async Task<ActionResult<List<ActivityMapper>>> GetAllActivities()
        {
            try
            {
                var activities = await _locationActivityRepository.GetAllActivities();
                return Ok(activities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("GetActivityById/{id}")]
        public async Task<ActionResult<ActivityMapper>> GetActivityById(int id)
        {
            try
            {
                var activity = await _locationActivityRepository.GetActivityById(id);
                if (activity == null) return NotFound(new { message = "Activity not found" });
                return Ok(activity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("AddActivity")]
        public async Task<IActionResult> AddActivity([FromBody] ActivityMapper activityMapper)
        {
            try
            {
                await _locationActivityRepository.AddActivity(activityMapper);
                return Ok(new { message = "Activity added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("UpdateActivity")]
        public async Task<IActionResult> UpdateActivity([FromBody] ActivityMapper activityMapper)
        {
            try
            {
                await _locationActivityRepository.UpdateActivity(activityMapper);
                return Ok(new { message = "Activity updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
