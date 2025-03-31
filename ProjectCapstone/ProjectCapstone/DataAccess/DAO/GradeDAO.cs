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
    public class GradeDAO
    {
        private readonly kmsContext _context;
        private readonly IMapper _mapper;

        public GradeDAO(kmsContext dbContext, IMapper mapper)
        {
            _context = dbContext;
            _mapper = mapper;
        }

        // Lấy tất cả các khối
        public async Task<List<GradeMapper>> GetAllGrades()
        {
            try
            {
                var allGrades = await _context.Grades.ToListAsync();
                return _mapper.Map<List<GradeMapper>>(allGrades);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving grades: " + ex.Message);
            }
        }
    }
}
