using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class AlbumRespository : IAlbumRespository
    {
        private readonly kmsContext _context;
        public AlbumRespository(kmsContext context
)
        { 
            _context = context;
        }
        public async Task<Album> CreateAlbumAsync(AlbumCreateDto albumDto)
        {
            // Check if ClassId exists in the database
            var existingClass = await _context.Classes.FindAsync(albumDto.ClassId);
            if (existingClass == null)
            {
                throw new Exception("Class does not exist. Please provide a valid Class.");
            }

            // Validate CreateBy user exists and has permissions
            var creatingUser = await _context.Users.FindAsync(albumDto.CreateBy);
            if (creatingUser == null)
            {
                throw new Exception("CreateBy user does not exist. Please provide a valid user.");
            }

            if (existingClass == null && creatingUser == null)
            {
                throw new Exception("CreateBy user does not exist. Please provide a valid user.");
            }

                // Create the album object
                var album = new Album
                {
                    AlbumName = albumDto.AlbumName,
                    ClassId = albumDto.ClassId,
                    Description = albumDto.Description,
                    CreateBy = albumDto.CreateBy,
                    ModifiBy = albumDto.CreateBy,
                    Status = 0, // Assuming '0' is the default status for a newly created album
                    IsActive = 1 // Assuming '1' means the album is active
                };

                // Add the album to the database
                _context.Albums.Add(album);
                await _context.SaveChangesAsync();
                return album;
        }

        public async Task<AlbumDtoh?> GetAlbumByIdAsync(int albumId)
        {
            var album = await _context.Albums
                .Include(a => a.Images)
                .Include(a => a.Class)
                .FirstOrDefaultAsync(a => a.AlbumId == albumId);

            if (album == null)
            {
                return null;
            }

            // Mapping dữ liệu từ Album sang AlbumDtoh
            var albumDto = new AlbumDtoh
            {
                AlbumId = album.AlbumId,
                ClassId = album.ClassId,
                CreateBy = album.CreateBy,
                ModifiBy = album.ModifiBy,
                AlbumName = album.AlbumName,
                TimePost = album.TimePost,
                Description = album.Description,
                Status = album.Status,
                isActive = album.Status,
                Reason = album.Reason 
            };

            return albumDto;
        }


        // Phương thức để lấy tất cả các album
        public async Task<IEnumerable<AlbumDtoh>> GetAllAlbumsAsync()
        {
            return await _context.Albums.Include(a => a.Images)
                .Include(a => a.Class)
                    .Select(album => new AlbumDtoh
                    {
                        AlbumId = album.AlbumId,
                        ClassId = album.ClassId,
                        CreateBy = album.CreateBy,
                        ModifiBy = album.ModifiBy,
                        AlbumName = album.AlbumName,
                        TimePost = album.TimePost,
                        Description = album.Description,
                        Status = album.Status, 
                        isActive = album.IsActive,
                        Reason = album.Reason,
                    })
                    .ToListAsync();
        }

        // Phương thức để cập nhật album
        public async Task<bool> UpdateAlbumAsync(AlbumUpdateDto albumDto)
        {
            if (albumDto == null)
                throw new ArgumentNullException(nameof(albumDto), "Album update data cannot be null.");

            // Find the album to update
            var album = await _context.Albums.FindAsync(albumDto.AlbumId);
            if (album == null)
                throw new InvalidOperationException($"Album with ID {albumDto.AlbumId} does not exist.");

            // Validate the associated ClassId
            var classExists = await _context.Classes.AnyAsync(c => c.ClassId == albumDto.ClassId);
            if (!classExists)
                throw new InvalidOperationException($"Class with ID {albumDto.ClassId} does not exist.");
            // Cập nhật thông tin album
            album.ClassId = albumDto.ClassId;
            album.AlbumName = albumDto.AlbumName;
            album.Description = albumDto.Description;

            // Lưu thay đổi
            _context.Albums.Update(album);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAlbumAsync(int albumId)
        {
            var album = await _context.Albums.FindAsync(albumId);
            if (album == null) throw new ArgumentNullException(nameof(album), "Album does not exist.");

            if (album.IsActive == 1)
            {
                album.IsActive = 0;
            }else
            {
                album.IsActive = 1;
            }
            // Cập nhật thay vì xóa
            _context.Albums.Update(album);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateStatusOfAlbum(UpdateStatusOfAlbum albumDto)
        {
            // Tìm album cần cập nhật
            var album = await _context.Albums.FindAsync(albumDto.AlbumId);
            if (album == null) throw new ArgumentNullException(nameof(album), "Album does not exist.");

            if (albumDto.Status == 1)
            {
                album.Status = albumDto.Status;
                album.TimePost = DateTime.Now;
            }
            if (albumDto.Status == 2)
            {
                album.Status = albumDto.Status;
                album.Reason = albumDto.Reason;
            }
            // Lưu thay đổi
            _context.Albums.Update(album);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
