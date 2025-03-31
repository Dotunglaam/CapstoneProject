using BusinessObject.DTOS;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Respository.Interfaces
{
    public interface IServiceRepository
    {
        Task<List<ServiceMapper>> GetAllServices();
        Task<ServiceMapper> GetServiceById(int id);
        Task AddService(ServiceMapper1 service);
        Task UpdateService(ServiceMapper1 service);
        Task AddChildService(ChildrenHasServicesMapper childService);
        Task UpdateChildService(ChildrenHasServicesMapper childService);
        Task AddCheckService(CheckservicesMapper checkservices);
        Task UpdateCheckService(CheckservicesMapper checkservices);
        Task<List<CheckservicesMapper>> GetCheckServiceByStudentIdAndDate(int studentId, DateOnly? date);
        Task<List<CheckservicesMapper>> GetCheckServiceByStudentIdAndWeek(int studentId, DateOnly? startDate, DateOnly? endDate);
        Task<List<ChildrenHasServicesMapper>> GetChildrenHasServiceByStudentId(int studentId);
    }
}
