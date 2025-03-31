using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Interfaces
{
    public interface IImageRepository
    {
        Image GetImageById(int id);
        Task<IEnumerable<Image>> ListAllImagesByAlbumIdAsync(int albumId);
        Task<bool> CreateImagesAsync(int albumId, List<IFormFile> images, string? caption);

        Task<bool> DeleteImageAsync(int imageId);
    }
}
