using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class PickupPersonDAO
    {
        private readonly kmsContext _context;
        private readonly IMapper _mapper;
        private readonly LuxandDAO _luxandDAO;
        public PickupPersonDAO(kmsContext dbContext, IMapper mapper)
        {
            _context = dbContext;
            _mapper = mapper;
        }

        public PickupPersonDAO(kmsContext dbContext, IMapper mapper, LuxandDAO luxandDAO)
        {
            _context = dbContext;
            _mapper = mapper;
            _luxandDAO= luxandDAO;
        }

        public async Task<PickupPersonInfoDto> GetPickupPersonInfoByUUIDAsync(string uuid)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(uuid))
            {
                throw new ArgumentException("UUID cannot be null or empty.", nameof(uuid));
            }

            if (uuid.Length != 36) 
            {
                throw new ArgumentException("Invalid UUID format.", nameof(uuid));
            }

            var pickupPerson = await _context.PickupPeople
                .Where(pp => pp.Uuid == uuid)
                .FirstOrDefaultAsync();

            if (pickupPerson == null)
            {
                return null;
            }

            var students = await _context.Children
                .Where(c => c.PickupPeople.Any(pp => pp.PickupPersonId == pickupPerson.PickupPersonId))
                .Select(c => new StudentInfoDto
                {
                    StudentID = c.StudentId,
                    FullName = c.FullName,
                    Code = c.Code,
                    Avatar = c.Avatar
                })
                .ToListAsync();

            var result = new PickupPersonInfoDto
            {
                PickupPersonID = pickupPerson.PickupPersonId,
                Name = pickupPerson.Name,
                PhoneNumber = pickupPerson.PhoneNumber,
                UUID = pickupPerson.Uuid,
                ImageUrl = pickupPerson.ImageUrl,
                Students = students
            };

            return result;
        }

        public async Task<PickupPersonInfoDto> GetPickupPersonInfoByParentIdAsync(int parentId)
        {
            var pickupPerson = await _context.PickupPeople
                .Where(pp => pp.ParentId == parentId)
                .FirstOrDefaultAsync();

            if (pickupPerson == null)
            {
                return null;
            }

            var students = await _context.Children
                .Where(c => c.PickupPeople.Any(pp => pp.PickupPersonId == pickupPerson.PickupPersonId))
                .Select(c => new StudentInfoDto
                {
                    StudentID = c.StudentId,
                    FullName = c.FullName,
                    Code = c.Code,
                    Avatar = c.Avatar
                })
                .ToListAsync();

            var result = new PickupPersonInfoDto
            {
                PickupPersonID = pickupPerson.PickupPersonId,
                Name = pickupPerson.Name,
                PhoneNumber = pickupPerson.PhoneNumber,
                UUID = pickupPerson.Uuid,
                ImageUrl = pickupPerson.ImageUrl,
                Students = students
            };

            return result;
        }
        public async Task<List<PickupPersonInfoDto>> GetPickupPersonInfoByStudentIdAsync(int studentId)
        {
            var pickupPeople = await _context.PickupPeople
                .Include(p => p.Students) 
                .Where(p => p.Students.Any(s => s.StudentId == studentId))
                .ToListAsync();

            if (!pickupPeople.Any())
            {
                return new List<PickupPersonInfoDto>(); 
            }

            var result = pickupPeople.Select(p => new PickupPersonInfoDto
            {
                PickupPersonID = p.PickupPersonId,
                Name = p.Name,
                PhoneNumber = p.PhoneNumber,
                UUID = p.Uuid,
                ImageUrl = p.ImageUrl,
                Students = p.Students.Select(s => new StudentInfoDto
                {
                    StudentID = s.StudentId,
                    FullName = s.FullName,
                    Code = s.Code,
                    Avatar = s.Avatar
                }).ToList()
            }).ToList();

            return result;
        }

        public async Task AddPickupPersonAsync(string name, string phoneNumber, int parentId, byte[] photoData)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("Phone number cannot be null or empty.", nameof(phoneNumber));
            }

            if (!Regex.IsMatch(phoneNumber, @"^\+?[0-9]{7,15}$"))
            {
                throw new ArgumentException("Invalid phone number format. It should contain 7 to 15 digits, optionally starting with '+'.", nameof(phoneNumber));
            }

            var pickupPerson = new PickupPerson
            {
                Name = name,
                PhoneNumber = phoneNumber,
                ParentId = parentId
            };

            var collections = "PickupPerson"; 

            var response = await _luxandDAO.AddPersonAsync(photoData, name, collections);

            var responseObject = JsonConvert.DeserializeObject<LuxandResponse>(response);

            if (responseObject?.Faces != null && responseObject.Faces.Any())
            {
                pickupPerson.Uuid = responseObject.Uuid;
                pickupPerson.ImageUrl = responseObject.Faces.First().Url; 
            }
            else
            {
                throw new Exception("Failed to retrieve face information from Luxand response.");
            }

            _context.PickupPeople.Add(pickupPerson);
            await _context.SaveChangesAsync();

            var studentIds = await GetStudentIdsByParentIdAsync(parentId);

            if (studentIds != null && studentIds.Any())
            {
                var students = await _context.Children
                    .Where(s => studentIds.Contains(s.StudentId))
                    .ToListAsync();

                foreach (var student in students)
                {
                    student.PickupPeople.Add(pickupPerson);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeletePickupPersonByIdAsync(int pickupPersonId)
        {
            var pickupPerson = await _context.PickupPeople
                .Include(pp => pp.Students)
                .FirstOrDefaultAsync(pp => pp.PickupPersonId == pickupPersonId);

            foreach (var child in pickupPerson.Students)
            {
                child.PickupPeople.Remove(pickupPerson);
            }

            if (!string.IsNullOrEmpty(pickupPerson.Uuid))
            {
                await _luxandDAO.DeletePersonAsync(pickupPerson.Uuid);
            }

            _context.PickupPeople.Remove(pickupPerson);

            await _context.SaveChangesAsync();

        }

        public async Task<List<int>> GetStudentIdsByParentIdAsync(int parentId)
        {
            var studentIds = await _context.Children
                .Where(c => c.ParentId == parentId) 
                .Select(c => c.StudentId) 
                .ToListAsync();

            return studentIds;
        }
    }
}
