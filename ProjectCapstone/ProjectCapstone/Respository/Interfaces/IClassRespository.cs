using BusinessObject.DTOS;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Interfaces
{
    public interface IClassRespository
    {
        Task<List<ClassMapper2>> GetAllClass();
        Task<ClassMapper2> GetClassById(int Id);
        Task<int> AddClass(ClassMapper classes);
        Task UpdateClass(ClassMapper classes );
        Task UpdateStatusClass(int classId, int newStatus);
        Task UpdateTeacherToClass (int classId, int currentTeacherId, int newTeacherId);
        Task<List<ChildrenMapper>> GetChildrenByClassId(int classId);

        Task<List<ClassMapper2>> GetClasssByStudentId(int studentId);

        Task<List<ClassMapper2>> GetClassesByTeacherId(int studentId);
        Task AddTeacherToClass(int classId, int teacherId);

        Task RemoveTeacherFromClass(int classId, int teacherId);

        Task UpdateHomeroomTeacher(int classId, int teacherId);
        Task SendMailToParentsByClassId(int classId);
        Task<List<TeacherMapper>> GetTeachersWithoutClass();

        Task<string> ImportClassExcel(IFormFile file);

    }
}
