using BusinessObject.DTOS;
using DataAccess.DAO;
using Microsoft.AspNetCore.Http;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class ChildrenRespository : IChildrenRespository
    {
        private readonly ChildrenDAO _childrenDAO;

        public ChildrenRespository(ChildrenDAO childrenDAO)
        {
            _childrenDAO = childrenDAO;
        }
        public async Task<ChildrenClassMapper> GetChildrenByChildrenId(int ChildrenId)
        {
            try
            {
                return await _childrenDAO.GetChildrenByChildrenId(ChildrenId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task ImportChildrenExcel(IFormFile file)
        {
            try
            {
                await _childrenDAO.ImportChildrenExcel(file);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task AddChildrenToClassesFromExcel(IFormFile file)
        {
            try
            {
                await _childrenDAO.AddChildrenToClassesFromExcel(file);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<int> AddChildren(ChildrenMapper childrenMapper)
        {
            try
            {
                var studentId = await _childrenDAO.AddChildren(childrenMapper);
                return studentId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task DeleteChildren(int Id)
        {
            try
            {
                await _childrenDAO.DeleteChildren(Id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<ChildrenClassMapper>> GetAllChildren()
        {
            try
            {
                return await _childrenDAO.GetAllChildren();
            }

            catch (Exception ex)
            {
                throw new Exception("Error while fetching children: " + ex.Message);

            }
        }

        public async Task UpdateChildren(ChildrenMapper childrenMapper)
        {
            try
            {
                await _childrenDAO.UpdateChildren(childrenMapper);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<ChildrenMapper>> GetChildrensWithoutClass(int classId)
        {
            try
            {
                var studentsWithoutClass = await _childrenDAO.GetChildrensWithoutClass(classId);
                return studentsWithoutClass;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task<string> ExportChildrenWithoutClassToExcel(int classId)
        {
            try
            {
                var filePath = await _childrenDAO.ExportChildrenWithoutClassToExcel(classId);
                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while exporting children without class to Excel: {ex.Message}", ex);
            }
        }


        public async Task AddChildrenImage(int studentId, IFormFile image)
        {
            try
            {
                await _childrenDAO.UploadImageAndSaveToDatabaseAsync(studentId, image);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task AddChildToClass(int classId, int studentId)
        {
            try
            {
                await _childrenDAO.AddChildToClass(classId, studentId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
