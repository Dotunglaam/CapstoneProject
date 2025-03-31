using BusinessObject.DTOS;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Interfaces
{
    public interface IAlbumRespository
    {
        Task<Album> CreateAlbumAsync(AlbumCreateDto album);

        // Lấy album theo Id
        Task<AlbumDtoh> GetAlbumByIdAsync(int albumId);

        // Lấy tất cả album
        Task<IEnumerable<AlbumDtoh>> GetAllAlbumsAsync();

        // Cập nhật album
        Task<bool> UpdateAlbumAsync(AlbumUpdateDto album);

        // Xóa album
        Task<bool> DeleteAlbumAsync(int albumId);
        Task<bool> UpdateStatusOfAlbum(UpdateStatusOfAlbum albumDto);
    }
}
