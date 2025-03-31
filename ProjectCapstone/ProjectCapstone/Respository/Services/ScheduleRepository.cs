
﻿using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static BusinessObject.DTOS.Request;
using Microsoft.AspNetCore.Http;
using BusinessObject.DTOS;

namespace Respository.Services
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly kmsContext _context;
        private readonly ScheduleDAO _scheduleDAO;

        public ScheduleRepository(kmsContext context, ScheduleDAO scheduleDAO)
        {
            _context = context;
            _scheduleDAO = scheduleDAO;
        }
        public async Task AddSchedule(ScheduleMapper schedule)
        {
            try
            {
                await _scheduleDAO.AddSchedule(schedule);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while adding schedule: " + ex.Message);
            }
        }
        public async Task<List<ScheduleMapper>> GetAllScheduleMappers()
        {
            try
            {
                return await _scheduleDAO.GetAllScheduleMappers();
            }

            catch (Exception ex)
            {
                throw new Exception("Error while fetching classes: " + ex.Message);

            }
        }

        public async Task<ScheduleMapper> GetScheduleById(int Id)
        {
            try
            {
                return await _scheduleDAO.GetScheduleById(Id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task ImportSchedulesAsync(IFormFile file, int ClassId)
        {
            try
            {
                await _scheduleDAO.ImportScheduleExcel(file, ClassId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }


        public async Task UpdateSchedule(ScheduleMapper schedule)
        {
            try
            {
                await _scheduleDAO.UpdateSchedule(schedule);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<ScheduleMapper>> GetSchedulesByClassId( int classId)
        {
            try
            {
                return await _scheduleDAO.GetSchedulesByClassId( classId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<LocationMapper>> GetAllLocationMappers()
        {
            try
            {
                return await _scheduleDAO.GetAllLocationByStatus();
            }

            catch (Exception ex)
            {
                throw new Exception("Error while fetching classes: " + ex.Message);

            }
        }

        public async Task<List<ActivityMapper>> GetAllActivityMappers()
        {
            try
            {
                return await _scheduleDAO.GetAllActivityByStatus();
            }

            catch (Exception ex)
            {
                throw new Exception("Error while fetching classes: " + ex.Message);

            }
        }

       
    }
}
