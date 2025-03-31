using BusinessObject.DTOS;
using Microsoft.AspNetCore.Mvc;
using Respository.Interfaces;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryServiceController : ControllerBase
    {
        public ICategoryServiceRespository _iCategoryServiceRespository;
        public CategoryServiceController(ICategoryServiceRespository iCategoryServiceRespository)
        {
            _iCategoryServiceRespository = iCategoryServiceRespository;
        }
        [HttpGet("GetAllCategoryService")]
        public async Task<List<CategoryServiceMapper>> GetAllCategoryService()
        {
            try
            {
                var result = await _iCategoryServiceRespository.GetAllCategoryService();
                return result;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }
        [HttpGet("GetCategoryServiceById/{id}")]
        public async Task<CategoryServiceMapper> GetCategoryServiceById(int id)
        {
            try
            {
                var result = await _iCategoryServiceRespository.GetCategoryServiceById(id);
                return result;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost("AddCategoryService")]
        public async Task<IActionResult> AddCategoryService([FromBody] CategoryServiceMapper categoryServiceMapper)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                await _iCategoryServiceRespository.AddCategoryService(categoryServiceMapper);
                return Ok(new { message = "CategoryService added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        [HttpPut("UpdateCategoryService")]
        public async Task<IActionResult> UpdateCategoryService([FromBody] CategoryServiceMapper categoryServiceMapper)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                var existingProduct = await _iCategoryServiceRespository.GetCategoryServiceById(categoryServiceMapper.CategoryServiceId);
                if (existingProduct == null)
                {
                    return NotFound(new { message = "CategoryService not found." });
                }

                await _iCategoryServiceRespository.UpdateCategoryService(categoryServiceMapper);
                return Ok(new { message = "CategoryService updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpDelete("DeleteCategoryService/{id}")]
        public async Task<IActionResult> DeleteCategoryService(int id)
        {
            try
            {
                var product = await _iCategoryServiceRespository.GetCategoryServiceById(id);
                if (product == null)
                {
                    return NotFound(new { message = "CategoryService not found." });
                }

                await _iCategoryServiceRespository.DeleteCategoryService(id);
                return Ok(new { message = "CategoryService deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}