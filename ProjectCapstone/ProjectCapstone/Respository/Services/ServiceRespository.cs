using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Respository.Services
{
	public class ServiceRepository : IServiceRepository
	{
		private readonly ServiceDAO _serviceDAO;

		public ServiceRepository(ServiceDAO serviceDAO)
		{
			_serviceDAO = serviceDAO ?? throw new ArgumentNullException(nameof(serviceDAO));
		}

		public async Task<ServiceMapper> GetServiceById(int id)
		{
			try
			{
				return await _serviceDAO.GetServiceById(id);
			}
			catch (KeyNotFoundException knfEx)
			{
				throw new Exception($"Service with ID {id} not found: {knfEx.Message}", knfEx);
			}
			catch (Exception ex)
			{
				throw new Exception("Error retrieving service: " + ex.Message, ex);
			}
		}

		public async Task AddService(ServiceMapper1 service)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));

			try
			{
				await _serviceDAO.AddNewService(service);
			}
			catch (Exception ex)
			{
				throw new Exception("Error adding new service: " + ex.Message, ex);
			}
		}

		public async Task<List<ServiceMapper>> GetAllServices()
		{
			try
			{
				return await _serviceDAO.GetAllServices();
			}
			catch (Exception ex)
			{
				throw new Exception("Error while fetching services: " + ex.Message, ex);
			}
		}

		public async Task UpdateService(ServiceMapper1 service)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));

			try
			{
				await _serviceDAO.UpdateService(service);
			}
			catch (KeyNotFoundException knfEx)
			{
				throw new Exception($"Service with ID {service.ServiceId} not found: {knfEx.Message}", knfEx);
			}
			catch (Exception ex)
			{
				throw new Exception("Error updating service: " + ex.Message, ex);
			}
		}

        public async Task AddChildService(ChildrenHasServicesMapper childService)
        {
            try
            {
                await _serviceDAO.AddChildService(childService);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new service: " + ex.Message, ex);
            }
        }

        public async Task UpdateChildService(ChildrenHasServicesMapper childService)
        {
            try
            {
                await _serviceDAO.UpdateChildService(childService);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new service: " + ex.Message, ex);
            }
        }

        public async Task AddCheckService(CheckservicesMapper checkservices)
        {
            try
            {
                await _serviceDAO.AddCheckService(checkservices);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new service: " + ex.Message, ex);
            }
        }

        public async Task UpdateCheckService(CheckservicesMapper checkservices)
        {
            try
            {
                await _serviceDAO.UpdateCheckService(checkservices);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new service: " + ex.Message, ex);
            }
        }

        public async Task<List<CheckservicesMapper>> GetCheckServiceByStudentIdAndDate(int studentId, DateOnly? date)
        {
            try
            {
                return await _serviceDAO.GetCheckServiceByStudentIdAndDate(studentId, date);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving check services by Student ID and Date: " + ex.Message, ex);
            }
        }

        public async Task<List<CheckservicesMapper>> GetCheckServiceByStudentIdAndWeek(int studentId, DateOnly? startDate, DateOnly? endDate)
        {
            try
            {
                return await _serviceDAO.GetCheckServiceByStudentIdAndWeek(studentId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving check services by Student ID and Date: " + ex.Message, ex);
            }
        }

        public async Task<List<ChildrenHasServicesMapper>> GetChildrenHasServiceByStudentId(int studentId)
        {
            try
            {
                return await _serviceDAO.GetChildrenHasServiceByStudentId(studentId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving child services by Student ID: " + ex.Message, ex);
            }
        }


    }
}
