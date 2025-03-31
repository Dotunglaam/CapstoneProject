using BusinessObject.DTOS;
using DataAccess.DAO;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Services
{
    public class LocationActivityRepository : ILocationActivityRepository
    {
        private readonly LocationActivityDAO _locationActivityDAO;

        public LocationActivityRepository(LocationActivityDAO locationActivityDAO)
        {
            _locationActivityDAO = locationActivityDAO;
        }

        // Location Methods
        public async Task<List<LocationMapper>> GetAllLocations()
        {
            try
            {
                return await _locationActivityDAO.GetAllLocations();
            }
            catch (Exception ex)
            {
                throw new Exception( ex.Message);
            }
        }

        public async Task<LocationMapper> GetLocationById(int id)
        {
            try
            {
                return await _locationActivityDAO.GetLocationById(id);
            }
            catch (Exception ex)
            {
                throw new Exception( ex.Message);
            }
        }

        public async Task AddLocation(LocationMapper locationMapper)
        {
            try
            {
                await _locationActivityDAO.AddLocation(locationMapper);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateLocation(LocationMapper locationMapper)
        {
            try
            {
                await _locationActivityDAO.UpdateLocation(locationMapper);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

      

        // Activity Methods
        public async Task<List<ActivityMapper>> GetAllActivities()
        {
            try
            {
                return await _locationActivityDAO.GetAllActivities();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ActivityMapper> GetActivityById(int id)
        {
            try
            {
                return await _locationActivityDAO.GetActivityById(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task AddActivity(ActivityMapper activityMapper)
        {
            try
            {
                await _locationActivityDAO.AddActivity(activityMapper);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateActivity(ActivityMapper activityMapper)
        {
            try
            {
                await _locationActivityDAO.UpdateActivity(activityMapper);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
