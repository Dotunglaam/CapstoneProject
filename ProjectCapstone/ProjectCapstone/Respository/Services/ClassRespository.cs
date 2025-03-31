using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.AspNetCore.Http;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class ClassRespository : IClassRespository
    {
        private readonly ClassDAO _classDAO;

        public ClassRespository(ClassDAO classDAO)
        {
            _classDAO = classDAO;
        }

        // Cập nhật để trả về ID của lớp mới
        public async Task<int> AddClass(ClassMapper classes)
        {
            try
            {
                return await _classDAO.AddNewClass(classes); // Giả sử bạn đã cập nhật DAO để trả về ID
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<ClassMapper2> GetClassById(int Id)
        {
            try
            {
                return await _classDAO.GetClassById(Id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<ClassMapper2>> GetAllClass()
        {
            try
            {
                return await _classDAO.GetAllClass();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching classes: " + ex.Message);
            }
        }
     
        public async Task UpdateClass(ClassMapper classes)
        {
            try
            {
                await _classDAO.UpdateClass(classes);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task UpdateStatusClass(int classId, int newStatus)
        {
            try
            {
                await _classDAO.UpdateClassStatus(classId, newStatus);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task UpdateTeacherToClass(int classId, int currentTeacherId, int newTeacherId)
        {
            try
            {
                await _classDAO.UpdateTeacherToClass(classId, currentTeacherId,newTeacherId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task<List<ChildrenMapper>> GetChildrenByClassId(int classId)
        {
            try
            {
                return await _classDAO.GetStudentsByClassId(classId);
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
        }

        public async Task<List<ClassMapper2>> GetClasssByStudentId(int studentId)
        {
            try
            {
                return await _classDAO.GetClasssByStudentId(studentId);
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
        }

        public async Task AddTeacherToClass(int classId, int teacherId)
        {
            try
            {
                await _classDAO.AddTeacherToClass(classId, teacherId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task RemoveTeacherFromClass(int classId, int teacherId)
        {
            try
            {
                await _classDAO.RemoveTeacherFromClass(classId, teacherId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task UpdateHomeroomTeacher(int classId, int teacherId)
        {
            try
            {
                await _classDAO.UpdateHomeroomTeacher(classId, teacherId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<ClassMapper2>> GetClassesByTeacherId(int studentId)
        {
            try
            {
                return await _classDAO.GetClassesByTeacherId(studentId);
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
        }
        public async Task SendMailToParentsByClassId(int classId)
        {
            try
            {
                await _classDAO.SendMailToParentsByClassId(classId);
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
        }

        public async Task<List<TeacherMapper>> GetTeachersWithoutClass()
        {
            try
            {
                return await _classDAO.GetTeachersWithoutClass();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching classes: " + ex.Message);
            }
        }

        public async Task<string> ImportClassExcel(IFormFile file)
        {
            try
            {
                return await _classDAO.ImportClassExcel(file);
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
