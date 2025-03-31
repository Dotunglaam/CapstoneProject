using BusinessObject.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Interfaces
{
    public interface IPickupPersonRepository
    {
        Task<PickupPersonInfoDto> GetPickupPersonInfoByUUIDAsync(string uuid);
        Task<PickupPersonInfoDto> GetPickupPersonInfoByParentIdAsync(int parentId);
        Task<List<PickupPersonInfoDto>> GetPickupPersonInfoByStudentIdAsync(int parentId);
        Task AddPickupPersonAsync(string name, string phoneNumber, int parentId, byte[] photoData);
        Task DeletePickupPersonByIdAsync(int pickupPersonId);
    }
}
