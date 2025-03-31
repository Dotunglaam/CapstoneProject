using BusinessObject.DTOS;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Respository.Interfaces
{
    public interface ISemesterRepository
    {
        Task<List<SemesterMapper>> GetAllSemester();
        Task<int> AddSemester(SemesterMapper semesterMapper);
        Task UpdateSemester(SemesterMapper semesterMapper);
        Task<int> AddSemesterReal(int schoolYearsID, Semesterreal1 semesterMapper);
        Task UpdateSemesterReal(int schoolYearsID,  Semesterreal1 semesterMapper);
        Task DeleteSemester(int semesterId);
        Task<List<Semesterreal1>> GetListSemesterBySchoolYear(int schoolYearsID);

    }
}
