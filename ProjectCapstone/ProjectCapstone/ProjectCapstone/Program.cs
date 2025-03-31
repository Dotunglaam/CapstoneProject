using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Mapper;
using BusinessObject.Models;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using Microsoft.EntityFrameworkCore;
using static BusinessObject.DTOS.Request;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;
using ProjectCapstone.Extensions;
using Quartz;
using Microsoft.Extensions.DependencyInjection;
using Respository.Services;



var builder = WebApplication.CreateBuilder(args);

var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<ScheduleResponse>("Schedule");
builder.Services.AddControllers().AddOData(opt => opt
    .Select()
    .Expand()
    .Filter()
    .OrderBy()
    .Count()
    .SetMaxTop(100)
.AddRouteComponents("odata", modelBuilder.GetEdmModel())
);

builder.Services.AddSwaggerGen();

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new Applicationmapper());
});
builder.Services.AddScoped<IMapper>(sp => mapperConfig.CreateMapper());


builder.Services.AddDbContext<kmsContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("Test"),
        new MySqlServerVersion(new Version(8, 0, 21)));
});

builder.Services.AddQuartz(q =>
{
    // Sử dụng DI Job Factory (Đảm bảo bạn có Quartz Microsoft Dependency Injection)
    q.UseMicrosoftDependencyInjectionJobFactory();

    // Đăng ký job của bạn (ví dụ job GenerateTuitionJob)
    q.AddJob<GenerateTuitionJob>(opts => opts.WithIdentity("GenerateTuitionJob", "group1"));
    // Định nghĩa trigger cron schedule
    q.AddTrigger(opts => opts
        .ForJob("GenerateTuitionJob", "group1")
        .WithIdentity("GenerateTuitionJobTrigger", "group1")
        .WithCronSchedule("0 0 20 L * ?")  
        //.WithCronSchedule("0 30 2 * * ?") 
    );

    q.AddJob<SendTuitionReminderJob>(opts => opts.WithIdentity("SendTuitionReminderJob", "group2"));
    q.AddTrigger(opts => opts
        .ForJob("SendTuitionReminderJob", "group2")
        .WithIdentity("SendTuitionReminderJobTrigger", "group2")
        .WithCronSchedule("0 30 12 * * ?")  
    );

    q.AddJob<UpdateTuitionStatusJob>(opts => opts.WithIdentity("UpdateTuitionStatusJob", "group3"));
    q.AddTrigger(opts => opts
    .ForJob("UpdateTuitionStatusJob", "group3")
    .WithIdentity("UpdateTuitionStatusJobTrigger", "group3")
    .WithCronSchedule("0 30 21 * * ?")  
);
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddRepositories();
builder.Services.AddDAOs();

var configuration = builder.Configuration;
builder.Services.AddSingleton<IConfiguration>(configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", build =>
        build.AllowAnyMethod()
             .AllowAnyHeader()
             .AllowCredentials()
             .SetIsOriginAllowed(hostName => true));
});

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddSingleton(provider =>
{
    var config = provider.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    return new Cloudinary(new Account(config.CloudName, config.ApiKey, config.ApiSecret));
});

builder.Services.AddSingleton<IScheduler>(provider =>
{
    var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
    return schedulerFactory.GetScheduler().Result;
});

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


app.UseCors(build =>
{
    build.AllowAnyOrigin()
         .AllowAnyMethod()
         .AllowAnyHeader();
});


app.MapControllers();

app.Run();

