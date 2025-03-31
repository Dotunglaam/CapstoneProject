using BusinessObject.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Interfaces
{
    public interface ICategoryServiceRespository
    {
        Task<List<CategoryServiceMapper>> GetAllCategoryService();
        Task<CategoryServiceMapper> GetCategoryServiceById(int Id);
        Task AddCategoryService(CategoryServiceMapper categoryServiceMapper);
        Task DeleteCategoryService(int Id);
        Task UpdateCategoryService(CategoryServiceMapper categoryServiceMapper);
    }
}
