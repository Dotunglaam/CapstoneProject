using BusinessObject.DTOS;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Respository.Interfaces;
using Respository.Services;
using Twilio.Http;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LuxandController : ControllerBase
    {
        private readonly ILuxandRepository _luxandRepository;

        public LuxandController(ILuxandRepository luxandRepository)
        {
            _luxandRepository = luxandRepository;
        }

        [HttpGet("ListPersons")]
        public async Task<IActionResult> ListPersons()
        {
            try
            {
                var persons = await _luxandRepository.ListPersonsAsync();

                return Ok(persons);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("AddPerson")]
        public async Task<IActionResult> AddPerson(IFormFile photo,[FromQuery] string name,[FromQuery] string collections)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest("Photo is required.");

            if (string.IsNullOrEmpty(name))
                return BadRequest("Name is required.");

            if (string.IsNullOrEmpty(collections))
                return BadRequest("Collections is required.");

            using var memoryStream = new MemoryStream();
            await photo.CopyToAsync(memoryStream);
            var photoData = memoryStream.ToArray();

            try
            {
                var result = await _luxandRepository.AddPersonAsync(photoData, name, collections);

                var recognitionResults = System.Text.Json.JsonSerializer.Deserialize<object>(result);

                return Ok(recognitionResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("RecognizePerson")]
        public async Task<IActionResult> RecognizePeople(IFormFile photo, [FromQuery] string collections)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest("Photo is required.");

            if (string.IsNullOrEmpty(collections))
                return BadRequest("Collections is required.");

            var extension = Path.GetExtension(photo.FileName).ToLower();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".bmp")
                return BadRequest("Only .jpg, .jpeg, .png, and .bmp files are allowed.");

            using var memoryStream = new MemoryStream();
            await photo.CopyToAsync(memoryStream);
            var photoData = memoryStream.ToArray();

            try
            {
                var result = await _luxandRepository.RecognizePeopleAsync(photoData, collections, extension);

                var recognitionResults = System.Text.Json.JsonSerializer.Deserialize<object>(result);

                return Ok(recognitionResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("VerifyPerson")]
        public async Task<IActionResult> VerifyPersonInPhotoAsync(IFormFile photo, [FromQuery] string uuid)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest("Photo is required.");

            var extension = Path.GetExtension(photo.FileName).ToLower();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".bmp")
                return BadRequest("Only .jpg, .jpeg, .png, and .bmp files are allowed.");

            using var memoryStream = new MemoryStream();
            await photo.CopyToAsync(memoryStream);
            var photoData = memoryStream.ToArray();

            try
            {
                var result = await _luxandRepository.VerifyPersonInPhotoAsync(photoData, uuid, extension);
                var recognitionResults = System.Text.Json.JsonSerializer.Deserialize<object>(result);
                return Ok(recognitionResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpDelete("DeletePerson/{uuid}")]
        public async Task<IActionResult> DeletePerson(string uuid)
        {
            try
            {
                await _luxandRepository.DeletePersonAsync(uuid);

                return Ok(new { message = "Person deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
