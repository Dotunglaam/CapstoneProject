using BusinessObject.Models;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Respository.Interfaces;
using Respository.Services;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageRepository _imageRepository;
        private readonly Cloudinary _cloudinary;

        public ImagesController(IImageRepository imageRepository, Cloudinary cloudinary)
        {
            _imageRepository = imageRepository;
            _cloudinary = cloudinary;
        }

        [HttpGet("listAllImageByAlbumId/{Id}")]
        public async Task<IActionResult> ListAllImagesByAlbumId(int Id)
        {
            var images = await _imageRepository.ListAllImagesByAlbumIdAsync(Id);

            if (images == null || !images.Any())
            {
                return Ok(new List<Image>()); // Trả về danh sách rỗng thay vì NotFound
            }

            return Ok(images);
        }
        [HttpPost("CreateImages")]
        public async Task<IActionResult> CreateImagesAsync([FromForm] int albumId, [FromForm] List<IFormFile> images, [FromForm] string? caption)
        {
            if (images == null || !images.Any())
            {
                return BadRequest(new { message = "No images uploaded." });
            }

            var result = await _imageRepository.CreateImagesAsync(albumId, images, caption);

            if (!result)
            {
                return StatusCode(500, new { message = "An error occurred while uploading images." });
            }

            return Ok(new { message = "Images uploaded successfully." });
        }

        [HttpDelete("DeleteImage/{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            // Tìm ảnh theo ID
            var image = _imageRepository.GetImageById(id);

            if (image == null)
            {
                return NotFound(new { message = "Image not found" });
            }

            // Thực hiện xóa ảnh
            var isDeleted = await _imageRepository.DeleteImageAsync(id);

            if (!isDeleted)
            {
                return StatusCode(500, new { message = "Failed to delete the image" });
            }

            return Ok(new { message = "Image deleted successfully", imageId = id });
        }
    }
}
