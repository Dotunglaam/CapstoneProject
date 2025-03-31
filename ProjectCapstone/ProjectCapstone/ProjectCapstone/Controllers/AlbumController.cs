using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Respository.Interfaces;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlbumController : ControllerBase
    {
        private readonly IAlbumRespository _albumRespository;
        
        public AlbumController(IAlbumRespository albumRespository)
        {
            _albumRespository = albumRespository;

        }
        [HttpPost("CreateAlbum")]
        public async Task<IActionResult> CreateAlbum([FromBody] AlbumCreateDto albumDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdAlbum = await _albumRespository.CreateAlbumAsync(albumDto);

            if (createdAlbum == null)
            {
                return StatusCode(500, "Error creating the album.");
            }

            return Ok("Album create successfully.");
        }
       
        [HttpGet("GetAllAlbums")]
        public async Task<IActionResult> GetAllAlbums()
        {
            var albums = await _albumRespository.GetAllAlbumsAsync();

            if (albums == null || !albums.Any())
            {
                return NotFound("No albums found.");
            }

            return Ok(albums);
        }

        [HttpPut("UpdateAlbum")]
        public async Task<IActionResult> UpdateAlbum([FromBody] AlbumUpdateDto album)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isUpdated = await _albumRespository.UpdateAlbumAsync(album);

            if (!isUpdated)
            {
                return NotFound("Album or Class not found.");
            }

            return Ok("Album updated successfully.");
        }

        [HttpDelete("IsActiveAlbum/{Id}")]
        public async Task<IActionResult> DeleteAlbum(int Id)
        {
            var success = await _albumRespository.DeleteAlbumAsync(Id);
            if (!success)
            {
                return NotFound("Album not found.");
            }

            return Ok("Album deleted successfully.");
        }
        [HttpGet("GetAlbumById/{id}")]
        public async Task<IActionResult> GetlAlbumsById(int id)
        {
            var albums = await _albumRespository.GetAlbumByIdAsync(id);

            if (albums == null)
            {
                return NotFound("No albums found.");
            }

            return Ok(albums);
        }
        [HttpPut("UpdateStatusAlbum")]
        public async Task<IActionResult> UpdateStatusAlbum([FromBody] UpdateStatusOfAlbum album)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isUpdated = await _albumRespository.UpdateStatusOfAlbum(album);

            if (!isUpdated)
            {
                return NotFound("Album or Class not found.");
            }

            return Ok("Album updated successfully.");
        }
    }
}
