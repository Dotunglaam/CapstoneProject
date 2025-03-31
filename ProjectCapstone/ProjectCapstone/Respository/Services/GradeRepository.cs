using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class GradeRepository : IGradeRepository
    {
        private readonly GradeDAO _gradeDAO;
        private readonly kmsContext _context;

        public GradeRepository(GradeDAO semesterDAO, kmsContext kms )
        {
            _gradeDAO = semesterDAO;
            _context = kms;
        }

        public async Task<List<GradeMapper>> GetAllGrades()
        {
            try
            {
                return await _gradeDAO.GetAllGrades();
            }
            catch
            {
                throw new NotImplementedException();

            }
        }
        public async Task<Grade> GetGradeByIdAsync(int id)
        {
            return await _context.Grades.FindAsync(id);
        }

        public async Task<Grade> AddGradeAsync(GradeModelDTO gradeDto)
        {
            var grade = new Grade
            {
                Name = gradeDto.Name,
                BaseTuitionFee = gradeDto.BaseTuitionFee,
                Description = gradeDto.Description,
            };

            // Add to the database
            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();

            return grade;
        }

        public async Task<Grade> UpdateGradeAsync(GradeModelDTO grade)
        {

            var existingGrade = await _context.Grades.FindAsync(grade.GradeId);
            if (existingGrade == null)
            {
                return null;
            }

            existingGrade.Name = grade.Name;
            existingGrade.BaseTuitionFee = grade.BaseTuitionFee;
            existingGrade.Description = grade.Description;

            _context.Grades.Update(existingGrade);
            await _context.SaveChangesAsync();
            return existingGrade;
        }

        public async Task<bool> SoftDeleteGradeAsync(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade == null)
            {
                return false;
            }

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
