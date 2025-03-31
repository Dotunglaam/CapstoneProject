using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class LocationActivityDAO
    {
        private readonly kmsContext _context;
        private readonly IMapper _mapper;

        public LocationActivityDAO(kmsContext dbContext, IMapper mapper)
        {
            _context = dbContext;
            _mapper = mapper;
        }

        // CRUD for Activity

        // Get all activities
        public async Task<List<ActivityMapper>> GetAllActivities()
        {
            try
            {
                var activities = await _context.Activities.ToListAsync();
                return _mapper.Map<List<ActivityMapper>>(activities);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // Get activity by ID
        public async Task<ActivityMapper> GetActivityById(int id)
        {
            try
            {
                var activity = await _context.Activities.FindAsync(id);
                return _mapper.Map<ActivityMapper>(activity);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // Add a new activity
        public async Task AddActivity(ActivityMapper activityMapper)
        {
            try
            {
                var isExist = await _context.Activities.AnyAsync(a => a.ActivityName.ToLower().Trim() == activityMapper.ActivityName.ToLower().Trim()
                                           && a.ActivityId != activityMapper.ActivityId);
                if (isExist)
                {
                    throw new Exception($"Activity with name '{activityMapper.ActivityName}' already exists.");
                }

                var activity = _mapper.Map<Activity>(activityMapper);
                await _context.Activities.AddAsync(activity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        // Update an existing activity
        public async Task UpdateActivity(ActivityMapper activityMapper)
        {
            try
            {
                var isExist = await _context.Activities
                    .AnyAsync(a => a.ActivityName.ToLower().Trim() == activityMapper.ActivityName.ToLower().Trim() && a.ActivityId != activityMapper.ActivityId);

                if (isExist)
                {
                    throw new Exception($"Activity with name '{activityMapper.ActivityName}' already exists.");
                }

                var activity = await _context.Activities.FindAsync(activityMapper.ActivityId);
                if (activity != null)
                {
                    _mapper.Map(activityMapper, activity);
                    _context.Activities.Update(activity);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Activity not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        // Get all locations
        public async Task<List<LocationMapper>> GetAllLocations()
        {
            try
            {
                var locations = await _context.Locations.ToListAsync();
                return _mapper.Map<List<LocationMapper>>(locations);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // Get location by ID
        public async Task<LocationMapper> GetLocationById(int id)
        {
            try
            {
                var location = await _context.Locations.FindAsync(id);
                return _mapper.Map<LocationMapper>(location);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // Add a new location
        public async Task AddLocation(LocationMapper locationMapper)
        {
            try
            {
                var isExist = await _context.Locations.AnyAsync(l => l.LocationName.ToLower().Trim() == locationMapper.LocationName.ToLower().Trim());
                if (isExist)
                {
                    throw new Exception($"Location with name '{locationMapper.LocationName}' already exists.");
                }

                var location = _mapper.Map<Location>(locationMapper);
                await _context.Locations.AddAsync(location);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        // Update an existing location
        public async Task UpdateLocation(LocationMapper locationMapper)
        {
            try
            {
                var isExist = await _context.Locations
                    .AnyAsync(l => l.LocationName.ToLower().Trim() == locationMapper.LocationName.ToLower().Trim() && l.LocationId != locationMapper.LocationId);

                if (isExist)
                {
                    throw new Exception($"Location with name '{locationMapper.LocationName}' already exists.");
                }

                var location = await _context.Locations.FindAsync(locationMapper.LocationId);
                if (location != null)
                {
                    _mapper.Map(locationMapper, location);
                    _context.Locations.Update(location);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Location not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
