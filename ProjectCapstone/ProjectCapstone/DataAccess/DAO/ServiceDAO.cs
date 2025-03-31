using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class ServiceDAO
    {
        private readonly kmsContext _context;
        private readonly IMapper _mapper;

        public ServiceDAO(kmsContext dbContext, IMapper mapper)
        {
            _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<List<ServiceMapper>> GetAllServices()
        {
            try
            {
                var allServices = await _context.Services
                    .Include(s => s.CategoryService)
                    .ToListAsync();

                return _mapper.Map<List<ServiceMapper>>(allServices);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving services: " + ex.Message, ex);
            }
        }

        public async Task<ServiceMapper> GetServiceById(int serviceId)
        {
            try
            {

                var service = await _context.Services
                    .Include(s => s.CategoryService)
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

                if (service == null)
                {
                    throw new KeyNotFoundException($"Service with ID {serviceId} not found.");
                }

                return _mapper.Map<ServiceMapper>(service);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving service: " + ex.Message, ex);
            }
        }

        public async Task AddNewService(ServiceMapper1 serviceMapper)
        {
            if (serviceMapper == null) throw new ArgumentNullException(nameof(serviceMapper));

            try
            {
                var categoryExists = await _context.Cagetoryservices.AnyAsync(c => c.CategoryServiceId == serviceMapper.CategoryServiceId);
                if (!categoryExists)
                {
                    throw new Exception("Invalid CategoryServiceId. The CategoryService does not exist.");
                }

                var schoolExists = await _context.Schools.AnyAsync(s => s.SchoolId == serviceMapper.SchoolId);
                if (!schoolExists)
                {
                    throw new Exception("Invalid SchoolId. The School does not exist.");
                }
                // Kiểm tra nếu dịch vụ đã tồn tại
                var isExist = await _context.Services.AnyAsync(a => a.ServiceName.ToLower().Trim() == serviceMapper.ServiceName.ToLower().Trim());
                if (isExist)
                {
                    throw new Exception($"Service with name '{serviceMapper.ServiceName}' already exists.");
                }

                serviceMapper.Status = 0;
                var serviceEntity = _mapper.Map<Service>(serviceMapper);
                await _context.Services.AddAsync(serviceEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new service: " + ex.Message, ex);
            }
        }

        public async Task UpdateService(ServiceMapper1 updatedService)
        {
            if (updatedService == null) throw new ArgumentNullException(nameof(updatedService));

            try
            {
                // Kiểm tra xem CategoryServiceId có tồn tại hay không
                var categoryExists = await _context.Cagetoryservices
                    .AnyAsync(c => c.CategoryServiceId == updatedService.CategoryServiceId);
                if (!categoryExists)
                {
                    throw new ArgumentException("Invalid CategoryService. The CategoryService does not exist.", nameof(updatedService.CategoryServiceId));
                }

                // Tìm dịch vụ hiện tại
                var existingService = await _context.Services.FindAsync(updatedService.ServiceId);
                if (existingService == null)
                {
                    throw new KeyNotFoundException($"Service with ID {updatedService.ServiceId} not found.");
                }

                // Kiểm tra xem tên dịch vụ đã tồn tại ngoại trừ bản ghi hiện tại
                var isNameDuplicate = await _context.Services
                    .AnyAsync(s => s.ServiceName == updatedService.ServiceName && s.ServiceId != updatedService.ServiceId);
                if (isNameDuplicate)
                {
                    throw new InvalidOperationException($"A service with the name '{updatedService.ServiceName}' already exists.");
                }

                // Cập nhật thông tin dịch vụ
                existingService.ServiceName = updatedService.ServiceName;
                existingService.ServicePrice = updatedService.ServicePrice;
                existingService.ServiceDes = updatedService.ServiceDes;
                existingService.SchoolId = updatedService.SchoolId;
                existingService.CategoryServiceId = updatedService.CategoryServiceId;
                existingService.Status = updatedService.Status;

                _context.Services.Update(existingService);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating service: " + ex.Message, ex);
            }
        }


        public async Task AddChildService(ChildrenHasServicesMapper childrenServiceMapper)
        {
            if (childrenServiceMapper == null) throw new ArgumentNullException(nameof(childrenServiceMapper));

            try
            {
                // Check if the student exists
                var studentExists = await _context.Children.AnyAsync(c => c.StudentId == childrenServiceMapper.StudentId);
                if (!studentExists)
                {
                    throw new KeyNotFoundException($"Student with ID {childrenServiceMapper.StudentId} not found.");
                }

                var serviceExists = await _context.Services.AnyAsync(s => s.ServiceId == childrenServiceMapper.ServiceId);
                if (!serviceExists)
                {
                    throw new KeyNotFoundException($"Service with ID {childrenServiceMapper.ServiceId} not found.");
                }

                // Map DTO to entity
                var childServiceEntity = _mapper.Map<ChildrenHasService>(childrenServiceMapper);

                await _context.ChildrenHasServices.AddAsync(childServiceEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding child service: " + ex.Message, ex);
            }
        }
        public async Task UpdateChildService(ChildrenHasServicesMapper childrenServiceMapper)
        {
            if (childrenServiceMapper == null) throw new ArgumentNullException(nameof(childrenServiceMapper));

            try
            {
                // Check if the entry exists
                var existingEntry = await _context.ChildrenHasServices
                    .FirstOrDefaultAsync(cs => cs.StudentId == childrenServiceMapper.StudentId && cs.ServiceId == childrenServiceMapper.ServiceId);

                if (existingEntry == null)
                {
                    throw new KeyNotFoundException($"Child service entry for StudentID {childrenServiceMapper.StudentId} and ServiceID {childrenServiceMapper.ServiceId} not found.");
                }

                // Update fields
                existingEntry.Time = childrenServiceMapper.Time;
                existingEntry.Status = childrenServiceMapper.Status;

                _context.ChildrenHasServices.Update(existingEntry);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating child service: " + ex.Message, ex);
            }
        }

        public async Task AddCheckService(CheckservicesMapper checkServiceMapper)
        {
            if (checkServiceMapper == null) throw new ArgumentNullException(nameof(checkServiceMapper));

            try
            {
                var studentExists = await _context.Children.AnyAsync(c => c.StudentId == checkServiceMapper.StudentId);
                if (!studentExists)
                {
                    throw new KeyNotFoundException($"Student with ID {checkServiceMapper.StudentId} not found.");
                }

                var serviceExists = await _context.Services.AnyAsync(s => s.ServiceId == checkServiceMapper.ServiceId);
                if (!serviceExists)
                {
                    throw new KeyNotFoundException($"Service with ID {checkServiceMapper.ServiceId} not found.");
                }

                checkServiceMapper.Status = 1;
                checkServiceMapper.PayService = 0;
                var checkServiceEntity = _mapper.Map<Checkservice>(checkServiceMapper);
                await _context.Checkservices.AddAsync(checkServiceEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding check service: " + ex.Message, ex);
            }
        }

        public async Task UpdateCheckService(CheckservicesMapper checkServiceMapper)
        {
            if (checkServiceMapper == null) throw new ArgumentNullException(nameof(checkServiceMapper));

            try
            {
                // Find the existing check service record
                var existingEntry = await _context.Checkservices
                    .FirstOrDefaultAsync(cs => cs.CheckServiceId == checkServiceMapper.CheckServiceId);

                if (existingEntry == null)
                {
                    throw new KeyNotFoundException($"Check service with ID {checkServiceMapper.CheckServiceId} not found.");
                }

                // Update fields
                existingEntry.ServiceId = checkServiceMapper.ServiceId;
                existingEntry.StudentId = checkServiceMapper.StudentId;
                existingEntry.Date = checkServiceMapper.Date;
                existingEntry.Status = checkServiceMapper.Status;
                existingEntry.PayService = 0;

                _context.Checkservices.Update(existingEntry);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating check service: " + ex.Message, ex);
            }
        }

        public async Task<List<CheckservicesMapper>> GetCheckServiceByStudentIdAndDate(int studentId, DateOnly? date)
        {
            try
            {
                var checkServices = await _context.Checkservices
                    .Where(cs => cs.StudentId == studentId && cs.Date == date)
                    .ToListAsync();

                return _mapper.Map<List<CheckservicesMapper>>(checkServices);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving check service by Student ID and Date: " + ex.Message, ex);
            }
        }
        public async Task<List<CheckservicesMapper>> GetCheckServiceByStudentIdAndWeek(int studentId, DateOnly? startDate, DateOnly? endDate)
        {
            try
            {
                var checkServices = await _context.Checkservices
                    .Where(cs => cs.StudentId == studentId && cs.Date >= startDate && cs.Date <= endDate)
                    .ToListAsync();

                return _mapper.Map<List<CheckservicesMapper>>(checkServices);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving check services by Student ID and Week: " + ex.Message, ex);
            }
        }

        public async Task<List<ChildrenHasServicesMapper>> GetChildrenHasServiceByStudentId(int studentId)
        {
            try
            {
                var childrenServices = await _context.ChildrenHasServices
                    .Where(cs => cs.StudentId == studentId)
                    .ToListAsync();

                return _mapper.Map<List<ChildrenHasServicesMapper>>(childrenServices);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving children services by Student ID: " + ex.Message, ex);
            }
        }

    }
}
