using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace DataAccess.DAO
{
    public class ScheduleDAO
    {
        private readonly kmsContext _context;
        private readonly IMapper _mapper;

        public ScheduleDAO(kmsContext dbContext, IMapper mapper)
        {
            _context = dbContext;
            _mapper = mapper;
        }
        public async Task AddSchedule(ScheduleMapper newSchedule)
        {
            try
            {

                if (newSchedule == null)
                    throw new ArgumentNullException(nameof(newSchedule), "New schedule cannot be null");

                // Kiểm tra nếu đã tồn tại Schedule cho ClassId này
                bool classHasSchedule = await _context.Schedules.AnyAsync(s => s.ClassId == newSchedule.ClassId);
                if (classHasSchedule)
                    throw new InvalidOperationException($"Schedule for ClassId {newSchedule.ClassId} already exists.");

                // Ánh xạ từ DTO sang thực thể Schedule
                var scheduleEntity = _mapper.Map<Schedule>(newSchedule);

                // Thêm schedule vào cơ sở dữ liệu
                _context.Schedules.Add(scheduleEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ScheduleMapper>> GetAllScheduleMappers()
        {
            try
            {
                var allSchedules = await _context.Schedules
                    .Include(s => s.Class) // Nếu cần thiết, có thể bao gồm các thực thể liên quan
                    .ToListAsync();

                return _mapper.Map<List<ScheduleMapper>>(allSchedules);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving schedules: " + ex.Message);
            }
        }

        public async Task UpdateSchedule(ScheduleMapper updatedSchedule)
        {
            try
            {
                // Kiểm tra sự tồn tại của lịch
                var existingSchedule = await _context.Schedules.FirstOrDefaultAsync(s => s.ScheduleId == updatedSchedule.ScheduleId);
                if (existingSchedule != null)
                {

                    existingSchedule.Status = updatedSchedule.Status;
                    existingSchedule.ClassId = updatedSchedule.ClassId;
                    existingSchedule.TeacherName = updatedSchedule.TeacherName;

                    _context.Schedules.Update(existingSchedule);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Schedule not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating schedule: " + ex.Message);
            }
        }
        public async Task<List<ScheduleMapper>> GetSchedulesByClassId(int classId)
        {
            try
            {
                // Fetch schedules that match the criteria
                var schedules = await _context.Schedules
                    .Where(s => s.ClassId == classId)
                    .ToListAsync();

                // Check if the result is empty

                // Map the schedules to ScheduleMapper DTOs
                var scheduleDtos = _mapper.Map<List<ScheduleMapper>>(schedules);

                // Return the mapped list
                return scheduleDtos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving schedules by ClassId: " + ex.Message);
            }
        }

        public async Task<ScheduleMapper> GetScheduleById(int scheduleId)
        {
            try
            {
                // Fetch the schedule by ID
                var scheduleEntity = await _context.Schedules
                    .Include(s => s.Class) // Include related entities if needed
                    .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);

                // Check if the schedule is null

                // Map the schedule to ScheduleMapper DTO
                return _mapper.Map<ScheduleMapper>(scheduleEntity);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving schedule: " + ex.Message);
            }
        }

        public async Task ImportScheduleExcel(IFormFile file, int classId)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        for (int sheetIndex = 1; sheetIndex < package.Workbook.Worksheets.Count; sheetIndex++)
                        {
                            var worksheet = package.Workbook.Worksheets[sheetIndex];

                            var startDateText = worksheet.Cells[2, 1].Text;
                            var endDateText = worksheet.Cells[2, 2].Text;

                            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-GB");
                            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-GB");

                            // Validate Start Date Format
                            if (!DateTime.TryParseExact(startDateText, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                            {
                                throw new Exception($"The Start Date in cell A2 is not in the correct format 'dd/MM/yyyy' on sheet {sheetIndex + 1}.");
                            }

                            // Validate End Date Format
                            if (!DateTime.TryParseExact(endDateText, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                            {
                                throw new Exception($"The End Date in cell B2 is not in the correct format 'dd/MM/yyyy' on sheet {sheetIndex + 1}.");
                            }

                            var weekDate = $"{startDate:dd/MM/yyyy}-{endDate:dd/MM/yyyy}";

                            var classEntity = await _context.Classes
                                .Include(c => c.Semester)
                                .FirstOrDefaultAsync(c => c.ClassId == classId);

                            if (classEntity == null)
                            {
                                throw new Exception($"Class with ID {classId} could not be found.");
                            }

                            // Validate Semester Existence
                            if (classEntity.Semester == null)
                            {
                                throw new Exception($"Semester for the Class with ID {classId} could not be found.");
                            }

                            // Validate Start and End Dates within Semester Range
                            if (startDate < classEntity.Semester.StartDate || endDate > classEntity.Semester.EndDate)
                            {
                                throw new Exception($"The Start Date ({startDate:dd/MM/yyyy}) or End Date ({endDate:dd/MM/yyyy}) is not within the semester period from {classEntity.Semester.StartDate:dd/MM/yyyy} to {classEntity.Semester.EndDate:dd/MM/yyyy}.");
                            }

                            // Validate Schedule Existence
                            var schedule = await _context.Schedules
                                .FirstOrDefaultAsync(s => s.ClassId == classId);

                            if (schedule == null)
                            {
                                throw new Exception($"Schedule for ClassId {classId} could not be found.");
                            }

                            // Validate Duplicate Weekdate
                            var existingScheduleDetails = await _context.Scheduledetails
                                .Where(sd => sd.ScheduleId == schedule.ScheduleId && sd.Weekdate.Trim() == weekDate.Trim())
                                .ToListAsync();

                            if (existingScheduleDetails.Any())
                            {
                                throw new Exception($"Weekdate {weekDate} already exists in ScheduleDetail for ScheduleId {schedule.ScheduleId}");
                            }

                            var rowCount = worksheet.Dimension.Rows;

                            for (int row = 4; row <= rowCount; row += 2)
                            {
                                var timeSlotText = worksheet.Cells[row, 1].Text;
                                var timeSlotId = ExtractIdFromFormattedString(timeSlotText);

                                // Validate TimeSlotId
                                if (timeSlotId <= 0)
                                {
                                    throw new Exception($"Invalid TimeSlotId in row {row}, column A on sheet {sheetIndex + 1}.");
                                }

                                for (int dayColumn = 2; dayColumn <= 7; dayColumn++)
                                {
                                    var activityText = worksheet.Cells[row, dayColumn].Text;
                                    var activityId = ExtractIdFromFormattedString(activityText);

                                    var locationText = worksheet.Cells[row + 1, dayColumn].Text;
                                    var locationId = ExtractIdFromFormattedString(locationText);

                                    // Validate ActivityId and LocationId
                                    if (activityId <= 0 || locationId <= 0)
                                    {
                                        throw new Exception($"Invalid ActivityId or LocationId in row {row}, column {dayColumn} on sheet {sheetIndex + 1}.");
                                    }

                                    // Check Activity and Location status
                                    var activity = await _context.Activities.FirstOrDefaultAsync(a => a.ActivityId == activityId);
                                    if (activity == null || activity.Status != 1)
                                    {
                                        throw new Exception($"Activity '{activityText}' (ID {activityId}) is not active or does not exist.");
                                    }

                                    var location = await _context.Locations.FirstOrDefaultAsync(l => l.LocationId == locationId);
                                    if (location == null || location.Status != 1)
                                    {
                                        throw new Exception($"Location '{locationText}' (ID {locationId}) is not active or does not exist.");
                                    }

                                    var scheduleDetail = new Scheduledetail
                                    {
                                        TimeSlotId = timeSlotId,
                                        ActivityId = activityId,
                                        LocationId = locationId,
                                        Day = GetDayOfWeek(dayColumn),
                                        ScheduleId = schedule.ScheduleId,
                                        Weekdate = weekDate
                                    };

                                    _context.Scheduledetails.Add(scheduleDetail);
                                }
                            }

                            await _context.SaveChangesAsync();
                            Console.WriteLine($"Successfully imported schedule details for ClassId {classId} and Weekdate {weekDate} from sheet {sheetIndex + 1}.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        // Helper function to extract the ID from the formatted string like "1 - Đón trẻ"
        private int ExtractIdFromFormattedString(string formattedString)
        {
            if (string.IsNullOrWhiteSpace(formattedString))
                return 0;

            var parts = formattedString.Split('-');
            if (parts.Length > 0 && int.TryParse(parts[0].Trim(), out int id))
            {
                return id;
            }

            return 0; // Return 0 if parsing fails
        }
        private string GetDayOfWeek(int column)
        {
            return column switch
            {
                2 => "Monday",
                3 => "Tuesday",
                4 => "Wednesday",
                5 => "Thursday",
                6 => "Friday",
                7 => "Saturday",
                _ => throw new ArgumentOutOfRangeException(nameof(column), "Invalid column number")
            };
        }

        public async Task<List<LocationMapper>> GetAllLocationByStatus()
        {
            try
            {
                var locations = await _context.Locations.Where(x=>x.Status ==1 )
                    .Select(l => new LocationMapper
                    {
                        LocationId = l.LocationId,
                        LocationName = l.LocationName
                    })
                    .ToListAsync();

                return locations;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving locations: " + ex.Message);
            }
        }

        public async Task<List<ActivityMapper>> GetAllActivityByStatus()
        {
            try
            {
                var activities = await _context.Activities.Where(x => x.Status == 1)
                    .Select(a => new ActivityMapper
                    {
                        ActivityId = a.ActivityId,
                        ActivityName = a.ActivityName
                    })
                    .ToListAsync();

                return activities;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving activities: " + ex.Message);
            }
        }

    }
}
