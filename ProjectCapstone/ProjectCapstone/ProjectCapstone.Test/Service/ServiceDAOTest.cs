using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ProjectCapstone.Test.Services
{
    public class ServiceDAOTest
    {
        private readonly IMapper _mapper;
        private readonly DbContextOptions<kmsContext> _dbOptions;

        public ServiceDAOTest()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Service, ServiceMapper>().ReverseMap();
                cfg.CreateMap<ChildrenHasService, ChildrenHasServicesMapper>().ReverseMap();
                cfg.CreateMap<Checkservice, CheckservicesMapper>().ReverseMap();
            });
            _mapper = mapperConfig.CreateMapper();

            _dbOptions = new DbContextOptionsBuilder<kmsContext>()
                .UseInMemoryDatabase("ServiceTestDatabase")
                .EnableSensitiveDataLogging()
                .Options;
        }

        private async Task SeedData(kmsContext context, bool includeService = false)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var category = new Cagetoryservice { CategoryServiceId = 1, CategoryName = "Basic Category" };
            var school = new School { SchoolId = 1, SchoolDes = "Test School" };

            context.Cagetoryservices.Add(category);
            context.Schools.Add(school);

            if (includeService)
            {
                var service = new Service
                {
                    ServiceId = 1,
                    ServiceName = "Test Service",
                    ServicePrice = 1000,
                    ServiceDes = "Test Description",
                    SchoolId = 1,
                    CategoryServiceId = 1,
                    Status = 1
                };
                context.Services.Add(service);
            }

            await context.SaveChangesAsync();
        }

        [Theory]
        [InlineData("Service 1", 500, "Description 1", 1, 1, true)] // Thêm thành công
        [InlineData("Service 2", 700, "Description 2", 1, 999, true)] // CategoryService không tồn tại
        [InlineData("Service 3", 800, "Description 3", 999, 1, true)] // School không tồn tại
        public async Task AddNewService_ShouldHandleVariousScenarios(
            string name,
            decimal price,
            string description,
            int schoolId,
            int categoryId,
            bool expectException)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                await SeedData(context);
                var dao = new ServiceDAO(context, _mapper);

                var serviceMapper = new ServiceMapper1
                {
                    ServiceName = name,
                    ServicePrice = price,
                    ServiceDes = description,
                    SchoolId = schoolId,
                    CategoryServiceId = categoryId
                };

                if (expectException)
                {
                    var exception = await Assert.ThrowsAsync<Exception>(async () => await dao.AddNewService(serviceMapper));
                    if (categoryId == 999)
                        Assert.Contains("Invalid CategoryServiceId", exception.Message);
                    if (schoolId == 999)
                        Assert.Contains("Invalid SchoolId", exception.Message);
                }
                else
                {
                    await dao.AddNewService(serviceMapper);
                    var addedService = await context.Services.FirstOrDefaultAsync(s => s.ServiceName == name);
                    Assert.NotNull(addedService);
                    Assert.Equal(price, addedService.ServicePrice);
                    Assert.Equal(description, addedService.ServiceDes);
                }
            }
        }

        [Theory]
        [InlineData(1, "Updated Service", 1500, "Updated Description", 1, 1, false)] // Cập nhật thành công
        [InlineData(999, "Non-existent Service", 2000, "Description", 1, 1, true)] // Service không tồn tại
        [InlineData(1, "Invalid Category", 2000, "Description", 1, 999, true)] // CategoryService không tồn tại
        public async Task UpdateService_ShouldHandleVariousScenarios(
            int serviceId,
            string name,
            decimal price,
            string description,
            int schoolId,
            int categoryId,
            bool expectException)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                await SeedData(context, includeService: true);
                var dao = new ServiceDAO(context, _mapper);

                var updatedService = new ServiceMapper1
                {
                    ServiceId = serviceId,
                    ServiceName = name,
                    ServicePrice = price,
                    ServiceDes = description,
                    SchoolId = schoolId,
                    CategoryServiceId = categoryId
                };

                if (expectException)
                {
                    var exception = await Assert.ThrowsAsync<Exception>(async () => await dao.UpdateService(updatedService));
                    if (serviceId == 999)
                        Assert.Contains($"Service with ID {serviceId} not found", exception.Message);
                    if (categoryId == 999)
                        Assert.Contains("Invalid CategoryService", exception.Message);
                }
                else
                {
                    await dao.UpdateService(updatedService);
                    var updatedEntity = await context.Services.FindAsync(serviceId);
                    Assert.NotNull(updatedEntity);
                    Assert.Equal(name, updatedEntity.ServiceName);
                    Assert.Equal(price, updatedEntity.ServicePrice);
                    Assert.Equal(description, updatedEntity.ServiceDes);
                }
            }
        }
    }
}
