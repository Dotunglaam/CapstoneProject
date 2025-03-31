using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using System;
using static BusinessObject.DTOS.DashBoardPrincipleMapper;

namespace BusinessObject.Mapper
{
    public class Applicationmapper : Profile
    {
        private readonly kmsContext _context;
        public Applicationmapper()
        {
            CreateMaps();
        }

        private void CreateMaps()
        {

            CreateMap<Class, ClassMapper>().ReverseMap();
            CreateMap<Child, ChildrenMapper>().ReverseMap();
            CreateMap<Cagetoryservice, CategoryServiceMapper>().ReverseMap();
            CreateMap<Models.Request, RequestMapper>().ReverseMap();
            CreateMap<Service, ServiceMapper>().ReverseMap();
            CreateMap<Service, ServiceMapper1>().ReverseMap();
            CreateMap<Semester, SemesterMapper>().ReverseMap();
            CreateMap<Grade, GradeMapper>().ReverseMap();
            CreateMap<Schedule, ScheduleMapper>().ReverseMap();
            CreateMap<Location, LocationMapper>().ReverseMap();
            CreateMap<Activity, ActivityMapper>().ReverseMap();
            CreateMap<ChildrenHasService, ChildrenHasServicesMapper>().ReverseMap();
            CreateMap<Checkservice, CheckservicesMapper>().ReverseMap();
            CreateMap<Attendance, AttendanceMapper>().ReverseMap();
            CreateMap<AttendanceDetail, AttendanceDetailMapper>().ReverseMap();
            CreateMap<NotificationMapper, Notification>().ReverseMap();
            CreateMap<UsernotificationMapper, Usernotification>().ReverseMap();

        CreateMap<Scheduledetail, ScheduleDetailMapper>()
            .AfterMap((src, dest) =>
            {
                dest.TimeSlotName = _context.TimeSlots
                    .Where(t => t.TimeSlotId == src.TimeSlotId)
                    .Select(t => t.TimeName)
                .FirstOrDefault();

                dest.ActivityName = _context.Activities
                    .Where(a => a.ActivityId == src.ActivityId)
                    .Select(a => a.ActivityName)
                .FirstOrDefault();

                dest.LocationName = _context.Locations
                    .Where(l => l.LocationId == src.LocationId)
                    .Select(l => l.LocationName)
                    .FirstOrDefault();
            })
                .ReverseMap();

            CreateMap<Menu, MenuMapper>()
                .ForMember(dest => dest.MenuID, opt => opt.MapFrom(src => src.MenuId))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();

            CreateMap<Menu, MenuStatusMapper>()
                .ForMember(dest => dest.MenuID, opt => opt.MapFrom(src => src.MenuId))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();

            CreateMap<Menudetail, MenuDetailMapper>()
                .ForMember(dest => dest.MenuDetailId, opt => opt.MapFrom(src => src.MenuDetailId))
                .ForMember(dest => dest.MealCode, opt => opt.MapFrom(src => src.MealCode))
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek))
                .ForMember(dest => dest.FoodName, opt => opt.MapFrom(src => src.FoodName))
                .ReverseMap();

           CreateMap<List<ClassHasChild>, List<EnrollmentStatisticsDTO>>()
                  .ConvertUsing((src, dest, context) =>
                  {
                  var years = src.Where(ch => ch.Date.HasValue)
                                 .Select(ch => ch.Date.Value.Year) // Lấy danh sách các năm
                                 .Distinct()
                                 .OrderBy(year => year); // Loại bỏ trùng lặp và sắp xếp theo năm

                  return years.Select(year => new EnrollmentStatisticsDTO
                  {
                      Year = year, // Gán từng năm
                      MonthlyEnrollments = MapMonthlyEnrollments(src, year) // Ánh xạ theo từng năm
                  }).ToList();
                  });

            CreateMap<List<User>, List<AccountmentStatisticsDTO>>()
                 .ConvertUsing((src, dest, context) =>
                 {
                     var years = src.Where(ch => ch.CreateAt.HasValue)
                                     .Select(ch => ch.CreateAt.Value.Year) // Lấy danh sách các năm
                                     .Distinct()
                                     .OrderBy(year => year); // Loại bỏ trùng lặp và sắp xếp theo năm

                     return years.Select(year => new AccountmentStatisticsDTO
                     {
                         Year = year, // Gán từng năm
                         MonthlyAccountments = MapMonthlyaccountments(src, year) // Ánh xạ theo từng năm
                     }).ToList();
                 });

            CreateMap<Teacher, TeacherDTO>()
               .ForMember(dest => dest.TeacherId, opt => opt.MapFrom(src => src.TeacherId))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Classes, opt => opt.MapFrom(src => src.Classes.Select(c => new ClassDTO
           {
               ClassId = c.ClassId,
               ClassName = c.ClassName,
               GradeName = c.Grade.Name
           }).ToList()));

            CreateMap<Class, ClassDTO>()
                .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.ClassId))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassName))
                .ForMember(dest => dest.GradeName, opt => opt.MapFrom(src => src.Grade.Name));

            // Ánh xạ từ Payment sang FinancialSummaryDTO
            CreateMap<List<Payment>, FinancialSummaryDTO>()
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => DateTime.Now.Year))  // Gán năm hiện tại
                .ForMember(dest => dest.FinancialSummaryByMonth, opt => opt.MapFrom(src => MapFinancialSummaryByMonth(src)));  // Ánh xạ qua hàm MapFinancialSummaryByMonth

            // Ánh xạ từ Payment sang FinancialSummaryByMonthDTO
            CreateMap<List<Payment>, FinancialSummaryByMonthDTO>()
            .ForMember(dest => dest.Month, opt => opt.MapFrom(src =>
                src.FirstOrDefault().PaymentDate.HasValue ?
                src.FirstOrDefault().PaymentDate.Value.ToString("MMMM") : "Unknown"))
            .ForMember(dest => dest.TuitionRevenue, opt => opt.MapFrom(src =>
                src.Where(p => p.TuitionId.HasValue).Sum(p => p.TotalAmount ?? 0)))
            .ForMember(dest => dest.ServiceRevenue, opt => opt.MapFrom(src =>
                src.Where(p => p.ServiceId.HasValue).Sum(p => p.TotalAmount ?? 0)))
            .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src =>
                src.Sum(p => p.TotalAmount ?? 0)));

            CreateMap<Semester, SemesterRealMapper>().ReverseMap();
            CreateMap<Semesterreal, Semesterreal1>().ReverseMap()
                .ForMember(dest => dest.SemesterrealId, opt => opt.MapFrom(src => src.SemesterrealId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ReverseMap();

            CreateMap<Class, ClassMapper2>().ReverseMap();

            CreateMap<Teacher, TeacherMapper>()
                .ForMember(dest => dest.TeacherId, opt => opt.MapFrom(src => src.TeacherId))
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.HomeroomTeacher, opt => opt.MapFrom(src => src.HomeroomTeacher))
                .ForMember(dest => dest.Mail, opt => opt.MapFrom(src => src.TeacherNavigation.Mail))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.TeacherNavigation.Avatar))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.TeacherNavigation.Code))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.TeacherNavigation.Status))
                .ReverseMap();
                 }

        // Hàm để ánh xạ Payment sang FinancialSummaryByMonthDTO
        private List<FinancialSummaryByMonthDTO> MapFinancialSummaryByMonth(List<Payment> payments)
        {
            return payments
            .GroupBy(p => p.PaymentDate?.ToString("MMMM") ?? "Unknown") // Nhóm theo tháng hoặc "Unknown"
                .Select(g => new FinancialSummaryByMonthDTO
                {
                    Month = g.Key,  // Lấy tên tháng
                    TuitionRevenue = g.Where(p => p.TuitionId.HasValue).Sum(p => p.TotalAmount ?? 0),  // Tổng doanh thu học phí
                    ServiceRevenue = g.Where(p => p.ServiceId.HasValue).Sum(p => p.TotalAmount ?? 0),  // Tổng doanh thu dịch vụ
                    TotalRevenue = g.Sum(p => p.TotalAmount ?? 0)  // Tổng doanh thu (học phí + dịch vụ)
                })
                .ToList();
        }

        // Hàm để ánh xạ ClassHasChild sang MonthlyEnrollmentDTO
        private List<MonthlyEnrollmentDTO> MapMonthlyEnrollments(List<ClassHasChild> classHasChildren, int year)
        {
            return classHasChildren
                .Where(ch => ch.Date.HasValue && ch.Date.Value.Year == year) // Lọc học sinh theo năm
                .GroupBy(ch => ch.Date.Value.Month) // Nhóm theo tháng
                .Select(g => new MonthlyEnrollmentDTO
                {
                    Month = g.Key,            // Lấy tháng
                    NewStudents = g.Count()   // Đếm số lượng học sinh gia nhập trong tháng đó
                })
                .OrderBy(me => me.Month)      // Sắp xếp theo tháng
                .ToList();
        }
        private List<AccountmentDTO> MapMonthlyaccountments(List<User> classHasChildren , int year)
        {
            return classHasChildren
                .Where(ch => ch.CreateAt.HasValue && ch.CreateAt.Value.Year == year) // Lọc học sinh theo năm
                .GroupBy(ch => ch.CreateAt.Value.Month) // Nhóm theo tháng
                .Select(g => new AccountmentDTO
                {
                    Month = g.Key,  // Lấy tháng
                    NewAccount = g.Count()  // Đếm số lượng học sinh gia nhập trong tháng đó
                })
                .OrderBy(me => me.Month)  // Sắp xếp theo tháng
                .ToList();
        }

    }

}

