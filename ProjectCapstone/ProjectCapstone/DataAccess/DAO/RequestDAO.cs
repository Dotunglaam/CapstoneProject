using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class RequestDAO
    {
        private readonly kmsContext _context;
        private readonly IMapper _mapper;

        public RequestDAO(kmsContext dbContext, IMapper mapper)
        {
            _context = dbContext;
            _mapper = mapper;
        }

        // Lấy tất cả các yêu cầu
        public async Task<List<RequestMapper>> GetAllRequests()
        {
            try
            {
                var requests = await _context.Requests.ToListAsync();
                return _mapper.Map<List<RequestMapper>>(requests);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving requests: " + ex.Message);
            }
        }

        // Lấy yêu cầu theo ID
        public async Task<RequestMapper> GetRequestById(int requestId)
        {
            try
            {
                var requestEntity = await _context.Requests
                    .FirstOrDefaultAsync(r => r.RequestId == requestId);
                if (requestEntity == null)
                {
                    throw new Exception("Request not found");
                }
                return _mapper.Map<RequestMapper>(requestEntity);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving request: " + ex.Message);
            }
        }

        // Thêm yêu cầu mới
        public async Task AddRequest(RequestMapper requestMapper)
        {
            try
            {
                await ValidateForeignKeysAsync(requestMapper); 

                var requestEntity = _mapper.Map<BusinessObject.Models.Request>(requestMapper);
                await _context.Requests.AddAsync(requestEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding request: " + ex.Message);
            }
        }

        // Cập nhật yêu cầu
        public async Task UpdateRequest(RequestMapper updatedRequest)
        {
            try
            {
                var existingRequest = await _context.Requests.FirstOrDefaultAsync(r => r.RequestId == updatedRequest.RequestId);
                if (existingRequest != null)
                {
                    existingRequest.Title = updatedRequest.Title;
                    existingRequest.Description = updatedRequest.Description;
                    existingRequest.StatusRequest = updatedRequest.StatusRequest;
                    existingRequest.CreateAt = updatedRequest.CreateAt;
                    existingRequest.CreateBy = updatedRequest.CreateBy;
                    existingRequest.StudentId = updatedRequest.StudentId;
                    existingRequest.ProcessNote = updatedRequest.ProcessNote;
                    await ValidateForeignKeysAsync(updatedRequest);

                    _context.Requests.Update(existingRequest);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Request not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating request: " + ex.Message);
            }
        }

        private async Task ValidateForeignKeysAsync(RequestMapper requestMapper)
        {
           

            // Kiểm tra StudentId
            if (requestMapper.StudentId.HasValue)
            {
                var studentExists = await _context.Children.AnyAsync(s => s.StudentId == requestMapper.StudentId);
                if (!studentExists)
                {
                    throw new Exception("Invalid StudentId. The Student does not exist.");
                }
            }
        }

        public async Task<List<ChildrenMapper>> GetStudentsByParentId(int parentId)
        {
            try
            {
                var students = await _context.Children
                    .Where(c => c.ParentId == parentId)
                    .ToListAsync();

                return _mapper.Map<List<ChildrenMapper>>(students);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving students by ParentId: " + ex.Message);
            }
        }
        public async Task UpdateStudentClassId(int studentId, int newclassId)
        {
            try
            {
                var student = await _context.Children
                    .Include(s => s.ClassHasChildren)  // Load the junction table 'ClassHasChild'
                        .ThenInclude(ch => ch.Class)  // Load the associated Class
                    .FirstOrDefaultAsync(s => s.StudentId == studentId);

                if (student == null)
                {
                    throw new Exception("Student not found");
                }

                // Kiểm tra nếu lớp có tồn tại
                var classExists = await _context.Classes.AnyAsync(c => c.ClassId == newclassId);
                if (!classExists)
                {
                    throw new Exception("Invalid Class. The Class does not exist.");
                }

                // Find if the student is already assigned to the class
                var classAlreadyAssigned = student.ClassHasChildren
                    .Any(ch => ch.ClassId == newclassId);

                student.ClassHasChildren.Clear(); // Clear existing classes

                if (!classAlreadyAssigned)
                {
                    // If the student is not already assigned, add the new class
                    student.ClassHasChildren.Add(new ClassHasChild
                    {
                        StudentId = studentId,
                        ClassId = newclassId,
                        Date = DateTime.Now // Or another value as needed
                    });
                }
                else
                {
                    throw new Exception("The student is already assigned to this class.");
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating class ID for student: " + ex.Message);
            }
        }


    }
}
