using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.DTOS.Request;

namespace Respository.Interfaces
{
    public interface ITeacherRepository
    {
        Task<IEnumerable<TeacherForByAll>> GetAllTeachersAsync();
        Task<TeacherForByAll> GetTeacherByIdAsync(int id);
        Task<bool> SoftDeleteTeacherAsync(int id);
        Task<Teacher> UpdateTeacherAsync(ViewProfileTeacherForTableTecher teacherModel);
    }
}
