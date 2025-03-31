using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class CategoryServiceDAO
    {
        private readonly kmsContext _context;
        private readonly IMapper _mapper;

        public CategoryServiceDAO(kmsContext dbContext, IMapper mapper)
        {
            _context = dbContext;
            _mapper = mapper;
        }
        public async Task<List<CategoryServiceMapper>> GetAllCategoryService()
        {
            try
            {
                var getAllCategoryService = await _context.Cagetoryservices.ToListAsync();
                return _mapper.Map<List<CategoryServiceMapper>>(getAllCategoryService);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving CategoryService: " + ex.Message);
            }
        }

        public async Task<CategoryServiceMapper> GetCategoryServiceById(int categoryServiceID)
        {
            try
            {
                var product = await _context.Cagetoryservices
                    .Include(x => x.Services)
                    .FirstOrDefaultAsync(p => p.CategoryServiceId == categoryServiceID);
                if (product == null)
                {
                    throw new Exception("CategoryService not found");
                }
                return _mapper.Map<CategoryServiceMapper>(product);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task AddNewCategoryService(CategoryServiceMapper categoryServiceMapper)
        {
            try
            {

                var CategoryServiceEntity = _mapper.Map<Cagetoryservice>(categoryServiceMapper);


                await _context.Cagetoryservices.AddAsync(CategoryServiceEntity);


                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public async Task DeleteCategoryService(int categoryServiceId)
        {
            try
            {
                var CategoryServiceEntity = await _context.Cagetoryservices.FirstOrDefaultAsync(p => p.CategoryServiceId == categoryServiceId);
                if (CategoryServiceEntity != null)
                {
                    _context.Cagetoryservices.Remove(CategoryServiceEntity);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Product not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task UpdateCategoryService(CategoryServiceMapper categoryServiceMapper)
        {
            try
            {

                var Product = await _context.Cagetoryservices.FirstOrDefaultAsync(p => p.CategoryServiceId == categoryServiceMapper.CategoryServiceId);
                if (Product != null)
                {
                    Product.CategoryName = categoryServiceMapper.CategoryName;

                    _context.Cagetoryservices.Update(Product);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("CategoryService not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
