using BusinessObject.DTOS;
using DataAccess.DAO;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class PickupPersonRepository : IPickupPersonRepository
    {
        private readonly PickupPersonDAO _pickupPersonDAO;

        public PickupPersonRepository(PickupPersonDAO pickupPersonDAO)
        {
            _pickupPersonDAO = pickupPersonDAO;
        }

        public async Task<PickupPersonInfoDto> GetPickupPersonInfoByUUIDAsync(string uuid)
        {
            try
            {
                return await _pickupPersonDAO.GetPickupPersonInfoByUUIDAsync(uuid);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<PickupPersonInfoDto> GetPickupPersonInfoByParentIdAsync(int parentId)
        {
            try
            {
                return await _pickupPersonDAO.GetPickupPersonInfoByParentIdAsync(parentId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task<List<PickupPersonInfoDto>> GetPickupPersonInfoByStudentIdAsync(int studentId)
        {
            try
            {
                return await _pickupPersonDAO.GetPickupPersonInfoByStudentIdAsync(studentId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task AddPickupPersonAsync(string name, string phoneNumber, int parentId, byte[] photoData)
        {
            try
            {
                await _pickupPersonDAO.AddPickupPersonAsync(name, phoneNumber, parentId, photoData);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task DeletePickupPersonByIdAsync(int pickupPersonId)
        {
            try
            {
                await _pickupPersonDAO.DeletePickupPersonByIdAsync(pickupPersonId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        
    }
}
