using BusinessObject.DTOS;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Interfaces
{
    public interface IChildrenRespository
    {
        Task<List<ChildrenClassMapper>> GetAllChildren();
        Task<ChildrenClassMapper> GetChildrenByChildrenId(int ChildrenId);
        Task ImportChildrenExcel(IFormFile file);
        Task AddChildrenToClassesFromExcel(IFormFile file);
        Task<int> AddChildren(ChildrenMapper childrenMapper);
        Task DeleteChildren(int Id);
        Task UpdateChildren(ChildrenMapper childrenMapper);
        Task AddChildrenImage(int studentId, IFormFile image);
        Task AddChildToClass(int classId, int studentId);
        Task<List<ChildrenMapper>> GetChildrensWithoutClass(int classId);
        Task<string> ExportChildrenWithoutClassToExcel(int classId);
    }
}
