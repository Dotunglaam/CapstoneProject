using BusinessObject.DTOS;
using Microsoft.AspNetCore.Mvc;
using Respository.Interfaces;
using Respository.Services;
using System;
using System.Web.Http.ModelBinding;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuRespository _menuRepository;

        public MenuController(IMenuRespository menuRespository)
        {
            _menuRepository = menuRespository;
        }

        [HttpGet("GetMenuByDate")]
        public async Task<IActionResult> GetMenuByDate(DateTime startDate, DateTime endDate)
        {
            try
            {
                var result = await _menuRepository.GetMenuByDate(startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving Menu.", details = ex.Message });
            }
        }

        [HttpGet("GetMenuByMenuId")]
        public async Task<IActionResult> GetMenuByMenuId(int MenuId)
        {
            try
            {
                var result = await _menuRepository.GetMenuByMenuId(MenuId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving Menu.", details = ex.Message });
            }
        }

        [HttpPut("UpdateMenu")]
        public async Task<IActionResult> UpdateMenu([FromBody] MenuMapper menuMapper)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var start = menuMapper.StartDate.ToDateTime(TimeOnly.MinValue);
                var end = menuMapper.EndDate.ToDateTime(TimeOnly.MinValue);

                var existingProduct = await _menuRepository.GetMenuByDate(start, end);
                if (existingProduct == null)
                {
                    return NotFound(new { message = "Menu not found." });
                }

                await _menuRepository.UpdateMenu(menuMapper);
                return Ok(new { message = "Menu updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("UpdateMenuStatus")]
        public async Task<IActionResult> UpdateMenuStatus([FromBody] MenuStatusMapper menuMapper)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var start = menuMapper.StartDate.ToDateTime(TimeOnly.MinValue);
                var end = menuMapper.EndDate.ToDateTime(TimeOnly.MinValue);

                var existingProduct = await _menuRepository.GetMenuByDate(start, end);
                if (existingProduct == null)
                {
                    return NotFound(new { message = "Menu not found." });
                }

                await _menuRepository.UpdateMenuStatus(menuMapper);
                return Ok(new { message = "Menu updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("ImportMenuExcel")]
        public async Task<IActionResult> ImportMenuExcel(IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                await _menuRepository.ImportMenuExcel(file);
                return Ok(new { message = "Menu added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("SendMenuToAllParentsMail")]
        public async Task<IActionResult> SendMenuToAllParentsMail()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _menuRepository.SendMenuToAllParentsMail();
                return Ok(new { message = "Sent mail successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
