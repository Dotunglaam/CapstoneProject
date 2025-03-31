using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using System.Web.Http;
using System.Text.RegularExpressions;

namespace DataAccess.DAO
{
    public class ScheduleDetailDAO

    {
        private readonly kmsContext _context;
        private readonly IMapper _mapper;

        public ScheduleDetailDAO(kmsContext dbContext, IMapper mapper)
        {
            _context = dbContext;
            _mapper = mapper;
        }
        public async Task<List<ScheduleDetailMapper>> GetAllScheduleDetailsByScheduleId(int scheduleId)
        {
            try
            {
                var details = await _context.Scheduledetails
                    .Where(sd => sd.ScheduleId == scheduleId)
                    .Select(sd => new ScheduleDetailMapper
                    {
                        ScheduleDetailId = sd.ScheduleDetailId,
                        TimeSlotId = sd.TimeSlotId,
                        ActivityId = sd.ActivityId,
                        LocationId = sd.LocationId,
                        Note = sd.Note,
                        Day = sd.Day,
                        Weekdate    = sd.Weekdate,
                        ScheduleId = sd.ScheduleId,
                        TimeSlotName = _context.TimeSlots
                            .Where(t => t.TimeSlotId == sd.TimeSlotId)
                            .Select(t => t.TimeName)
                            .FirstOrDefault(),
                        ActivityName = _context.Activities
                            .Where(a => a.ActivityId == sd.ActivityId)
                            .Select(a => a.ActivityName)
                            .FirstOrDefault(),
                        LocationName = _context.Locations
                            .Where(l => l.LocationId == sd.LocationId)
                            .Select(l => l.LocationName)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return details;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Schedule Details: " + ex.Message, ex);
            }
        }

        public async Task UpdateScheduleDetailById(int id, ScheduleDetailMapper updatedDetail)
        {
            if (updatedDetail == null)
                throw new ArgumentNullException(nameof(updatedDetail), "Updated detail cannot be null");

            try
            {
                var existingDetail = await _context.Scheduledetails
                    .FirstOrDefaultAsync(sd => sd.ScheduleDetailId == id);

                if (existingDetail == null)
                    throw new InvalidOperationException("Schedule Detail not found");

                // Update properties
                existingDetail.TimeSlotId = updatedDetail.TimeSlotId;
                existingDetail.ActivityId = updatedDetail.ActivityId;
                existingDetail.LocationId = updatedDetail.LocationId;
                existingDetail.Note = updatedDetail.Note;
                existingDetail.Day = updatedDetail.Day;
                existingDetail.ScheduleId = updatedDetail.ScheduleId;
                existingDetail.Weekdate = updatedDetail.Weekdate;

                _context.Scheduledetails.Update(existingDetail);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception("Error updating Schedule Detail: Record has been modified by another user.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Schedule Detail: " + ex.Message, ex);
            }
        }

        public async Task AddScheduleDetail(ScheduleDetailMapper newDetail)
        {
            if (newDetail == null)
                throw new ArgumentNullException(nameof(newDetail), "New detail cannot be null");
            try
            {
                var scheduleDetail = _mapper.Map<Scheduledetail>(newDetail);

                await _context.Scheduledetails.AddAsync(scheduleDetail);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error adding new Schedule Detail: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new Schedule Detail: " + ex.Message, ex);
            }
        }

        public async Task AddScheduleDetails(List<ScheduleDetailMapper> newDetails)
        {
            if (newDetails == null || newDetails.Count == 0)
                throw new ArgumentNullException(nameof(newDetails), "List of new details cannot be null or empty");

            try
            {
                // Lấy danh sách các ScheduleId từ các chi tiết
                var scheduleIds = newDetails.Select(d => d.ScheduleId).Distinct().ToList();

                // Lấy danh sách các Schedule tương ứng
                var schedules = await _context.Schedules
                    .Include(s => s.Class.Semester)
                    .Where(s => scheduleIds.Contains(s.ScheduleId))
                    .ToListAsync();

                var existingWeekdates = await _context.Scheduledetails
                    .Where(sd => scheduleIds.Contains(sd.ScheduleId))
                    .Select(sd => new { sd.ScheduleId, sd.Weekdate })
                    .ToListAsync();

                foreach (var detail in newDetails)
                {
                    // Validate định dạng weekdate
                    if (!Regex.IsMatch(detail.Weekdate ?? "", @"^\d{2}/\d{2}/\d{4}-\d{2}/\d{2}/\d{4}$"))
                    {
                        throw new ArgumentException($"Invalid weekdate format for ScheduleDetailId {detail.ScheduleDetailId}");
                    }

                    var dates = detail.Weekdate.Split('-');
                    var startDate = DateTime.ParseExact(dates[0], "dd/MM/yyyy", null);
                    var endDate = DateTime.ParseExact(dates[1], "dd/MM/yyyy", null);

                    // Lấy Schedule tương ứng
                    var schedule = schedules.FirstOrDefault(s => s.ScheduleId == detail.ScheduleId);
                    if (schedule == null)
                        throw new Exception($"Schedule with ID {detail.ScheduleId} not found.");

                    // Lấy semester của class
                    var semester = schedule.Class.Semester;
                    if (semester == null)
                        throw new Exception($"Semester not found for ScheduleId {detail.ScheduleId}.");

                    // Kiểm tra startDate và endDate nằm trong semester
                    if (startDate < semester.StartDate || endDate > semester.EndDate)
                    {
                        throw new ArgumentException($"Weekdate {detail.Weekdate} is outside the semester range for ScheduleId {detail.ScheduleId}.");
                    }

                    // Kiểm tra weekdate trùng lặp
                    if (existingWeekdates.Any(e => e.ScheduleId == detail.ScheduleId && e.Weekdate == detail.Weekdate))
                    {
                        throw new ArgumentException($"Weekdate {detail.Weekdate} already exists for ScheduleId {detail.ScheduleId}.");
                    }
                }

                // Thêm mới danh sách các chi tiết hợp lệ
                var scheduleDetails = _mapper.Map<List<Scheduledetail>>(newDetails);
                await _context.Scheduledetails.AddRangeAsync(scheduleDetails);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error adding new Schedule Details: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new Schedule Details: " + ex.Message, ex);
            }
        }


    }
}
