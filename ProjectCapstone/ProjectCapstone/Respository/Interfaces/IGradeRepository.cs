using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Interfaces
{
    public interface IGradeRepository
    {
        Task<List<GradeMapper>> GetAllGrades();
        Task<Grade> GetGradeByIdAsync(int id);
        Task<Grade> AddGradeAsync(GradeModelDTO grade);
        Task<Grade> UpdateGradeAsync(GradeModelDTO grade);
        Task<bool> SoftDeleteGradeAsync(int id);
    }
}
