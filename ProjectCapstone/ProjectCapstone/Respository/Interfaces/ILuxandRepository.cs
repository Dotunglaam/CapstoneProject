using BusinessObject.DTOS;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Interfaces
{
    public interface ILuxandRepository
    {
        Task<List<Person>> ListPersonsAsync();
        Task<string> AddPersonAsync(byte[] photoData, string name, string collections);
        Task<string> RecognizePeopleAsync(byte[] photoData, string collections, string fileExtension);
        Task DeletePersonAsync(string uuid);
        Task<string> VerifyPersonInPhotoAsync(byte[] photoData, string uuid, string fileExtension);
    }
}
