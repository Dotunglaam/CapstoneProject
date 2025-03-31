using BusinessObject.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Interfaces
{
    public interface IRequestRepository
    {
        Task<List<RequestMapper>> GetAllRequests();         
        Task<RequestMapper> GetRequestById(int requestId);   
        Task AddRequest(RequestMapper request);               
        Task UpdateRequest(RequestMapper request);  
        Task<List<ChildrenMapper>> GetStudentsByParentId(int requestId);
        Task UpdateStudentClassId(int studentId, int newClassId);

    }
}
