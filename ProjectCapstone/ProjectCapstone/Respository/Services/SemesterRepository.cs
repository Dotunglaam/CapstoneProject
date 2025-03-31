using BusinessObject.DTOS;
using DataAccess.DAO;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class SemesterRepository : ISemesterRepository
    {
        private readonly SemesterDAO _semesterDAO;

        public SemesterRepository(SemesterDAO semesterDAO)
        {
            _semesterDAO = semesterDAO;
        }

        // Lấy tất cả kỳ học
        public async Task<List<SemesterMapper>> GetAllSemester()
        {
            try
            {
                return await _semesterDAO.GetAllSemesters();
            }
            catch (Exception ex)
            {
                throw new Exception( ex.Message);
            }
        }

        // Thêm kỳ học mới
        public async Task<int> AddSemester(SemesterMapper semesterMapper)
        {
            try
            {
                return await _semesterDAO.AddSemester(semesterMapper);
            }
            catch (Exception ex)
            {
                throw new Exception( ex.Message);
            }
        }

        // Sửa kỳ học
        public async Task UpdateSemester(SemesterMapper semesterMapper)
        {
            try
            {
                await _semesterDAO.UpdateSemester(semesterMapper);
            }
            catch (Exception ex)
            {
                throw new Exception( ex.Message);
            }
        }

        public async Task<int> AddSemesterReal(int schoolYearsID,Semesterreal1 semesterMapper)
        {
            try
            {
                return await _semesterDAO.AddSemesterReal(schoolYearsID,semesterMapper);
            }
            catch (Exception ex)
            {
                throw new Exception( ex.Message);
            }
        }

        // Sửa kỳ học
        public async Task UpdateSemesterReal(int schoolYearsID ,Semesterreal1 semesterMapper)
        {
            try
            {
                await _semesterDAO.UpdateSemesterReal(schoolYearsID, semesterMapper);
            }
            catch (Exception ex)
            {
                throw new Exception( ex.Message);
            }
        }
        public async Task DeleteSemester(int semesterId)
        {
            try
            {
                await _semesterDAO.DeleteSemesterReal(semesterId);
            }
            catch (Exception ex)
            {
                throw new Exception( ex.Message);
            }
        }
        public async Task<List<Semesterreal1>> GetListSemesterBySchoolYear(int schoolYearsID)
        {
            try
            {
                return await _semesterDAO.GetAllSemesterRealBySchoolYearID(schoolYearsID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
