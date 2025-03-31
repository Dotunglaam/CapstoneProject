using BusinessObject.DTOS;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface ILocationActivityRepository
    {
        // CRUD for Location
        Task<List<LocationMapper>> GetAllLocations();
        Task<LocationMapper> GetLocationById(int id);
        Task AddLocation(LocationMapper locationMapper);
        Task UpdateLocation(LocationMapper locationMapper);

        // CRUD for Activity
        Task<List<ActivityMapper>> GetAllActivities();
        Task<ActivityMapper> GetActivityById(int id);
        Task AddActivity(ActivityMapper activityMapper);
        Task UpdateActivity(ActivityMapper activityMapper);
    }
}
