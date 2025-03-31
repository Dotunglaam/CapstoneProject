using BusinessObject.DTOS;
using DataAccess.DAO;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class CategoryServiceRespository : ICategoryServiceRespository
    {
        private readonly CategoryServiceDAO _categoryServiceDAO;
        public CategoryServiceRespository(CategoryServiceDAO categoryServiceDAO)
        {
            _categoryServiceDAO = categoryServiceDAO;
        }


        public async Task<List<CategoryServiceMapper>> GetAllCategoryService()
        {
            try
            {
                return await _categoryServiceDAO.GetAllCategoryService();
            }

            catch (Exception ex)
            {
                throw new Exception("Error while fetching category services: " + ex.Message);

            }
        }

        public async Task<CategoryServiceMapper> GetCategoryServiceById(int Id)
        {
            try
            {
                return await _categoryServiceDAO.GetCategoryServiceById(Id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task AddCategoryService(CategoryServiceMapper categoryServiceMapper)
        {
            try
            {
                await _categoryServiceDAO.AddNewCategoryService(categoryServiceMapper);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task DeleteCategoryService(int Id)
        {
            try
            {
                await _categoryServiceDAO.DeleteCategoryService(Id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task UpdateCategoryService(CategoryServiceMapper categoryServiceMapper)
        {
            try
            {
                await _categoryServiceDAO.UpdateCategoryService(categoryServiceMapper);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
