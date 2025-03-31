using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using BusinessObject.DTOS;
using Newtonsoft.Json;
using DataAccess.DAO;

namespace Respository.Services
{
    public class LuxandRepository : ILuxandRepository
    {
        private readonly LuxandDAO _luxandDAO;

        public LuxandRepository(LuxandDAO luxandDAO)
        {
            _luxandDAO = luxandDAO;
        }

        public async Task<List<Person>> ListPersonsAsync()
        {
            try
            {
                return await _luxandDAO.ListPersonsAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<string> AddPersonAsync(byte[] photoData, string name, string collections)
        {
            try
            {
                return await _luxandDAO.AddPersonAsync(photoData, name, collections);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<string> RecognizePeopleAsync(byte[] photoData, string collections, string fileExtension)
        {
            try
            {
                return await _luxandDAO.RecognizePeopleAsync(photoData, collections, fileExtension);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task DeletePersonAsync(string uuid)
        {
            try
            {
                await _luxandDAO.DeletePersonAsync(uuid);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<string> VerifyPersonInPhotoAsync(byte[] photoData, string uuid, string fileExtension)
        {
            try
            {
                return await _luxandDAO.VerifyPersonInPhotoAsync(photoData, uuid, fileExtension);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

    }
}
