using BusinessObject.DTOS;
using DataAccess.DAO;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class RequestRepository : IRequestRepository
    {
        private readonly RequestDAO _requestDAO;

        public RequestRepository(RequestDAO requestDAO)
        {
            _requestDAO = requestDAO;
        }

        public async Task<RequestMapper> GetRequestById(int Id)
        {
            try
            {
                return await _requestDAO.GetRequestById(Id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task AddRequest(RequestMapper request)
        {
            try
            {
                await _requestDAO.AddRequest(request);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<RequestMapper>> GetAllRequests()
        {
            try
            {
                return await _requestDAO.GetAllRequests();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching classes: " + ex.Message);
            }
        }

        public async Task UpdateRequest(RequestMapper request)
        {
            try
            {
                await _requestDAO.UpdateRequest(request);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<ChildrenMapper>> GetStudentsByParentId(int requestId)
        {
            try
            {
                return await _requestDAO.GetStudentsByParentId(requestId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task UpdateStudentClassId(int studentId, int newClassId)
        {
            try
            {
                await _requestDAO.UpdateStudentClassId(studentId, newClassId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating student class: " + ex.Message);
            }
        }
    }
}
