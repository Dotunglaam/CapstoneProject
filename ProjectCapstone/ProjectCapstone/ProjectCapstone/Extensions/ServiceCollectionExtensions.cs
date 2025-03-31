using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.OpenApi.Models;
using Repository.Interfaces;
using Repository.Services;
using Respository.Interfaces;
using Respository.Services;

namespace ProjectCapstone.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IClassRespository, ClassRespository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IResetPasswordTokenRepository, ResetPasswordTokenRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITeacherRepository, TeacherRepository>();
            services.AddScoped<IScheduleRepository, ScheduleRepository>();
            services.AddScoped<IChildrenRespository, ChildrenRespository>();
            services.AddScoped<IScheduleDetailRepository, ScheduleDetailRepository>();
            services.AddScoped<ICategoryServiceRespository, CategoryServiceRespository>();
            services.AddScoped<IMenuRespository, MenuRespository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<ISemesterRepository, SemesterRepository>();
            services.AddScoped<IGradeRepository, GradeRepository>();
            services.AddScoped<IAttendanceRepository, AttendanceRepository>();
            services.AddScoped<IAlbumRespository, AlbumRespository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped<ITuitionService, TuitionService>();
            services.AddScoped<GenerateTuitionJob>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<ILocationActivityRepository,LocationActivityRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<ILuxandRepository, LuxandRepository>();
            services.AddScoped<IPickupPersonRepository, PickupPersonRepository>();


            return services;

        }

        public static IServiceCollection AddDAOs(this IServiceCollection services)
        {
            services.AddScoped<ClassDAO>();
            services.AddScoped<ScheduleDAO>();
            services.AddScoped<ChildrenDAO>();
            services.AddScoped<ScheduleDetailDAO>();
            services.AddScoped<CategoryServiceDAO>();
            services.AddScoped<MenuDAO>();
            services.AddScoped<ServiceDAO>();
            services.AddScoped<RequestDAO>();
            services.AddScoped<SemesterDAO>();
            services.AddScoped<GradeDAO>();
            services.AddScoped<AttendanceDAO>();
            services.AddScoped<NotificationDAO>();
            services.AddScoped<UserDAO>();
            services.AddScoped<BusinessObject.DTOS.Request>();
            services.AddScoped<DashBoardPrincipleDAO>();
            services.AddScoped<LocationActivityDAO>();
            services.AddScoped<PickupPersonDAO>();
            services.AddHttpClient<LuxandDAO>();
            return services;
        }
    }
}
