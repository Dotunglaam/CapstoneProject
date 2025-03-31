using BusinessObject.Models;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class ImageRepository : IImageRepository
    {
        private readonly kmsContext _context;
        private readonly Cloudinary _cloudinary;
        public ImageRepository(kmsContext context , Cloudinary cloudinary
)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        public async Task<IEnumerable<Image>> ListAllImagesByAlbumIdAsync(int albumId)
        {
            return await _context.Images
            .Where(img => img.AlbumId == albumId) 
            .ToListAsync();
        }
        public async Task<bool> CreateImagesAsync(int albumId, List<IFormFile> images, string? caption)
        {
            var uploadedImages = new List<Image>();

            foreach (var image in images)
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(image.FileName, image.OpenReadStream()),
                    Folder = $"AlbumImages/{albumId}",
                    PublicId = $"{Guid.NewGuid()}",
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult != null && !string.IsNullOrEmpty(uploadResult.SecureUri.ToString()))
                {
                    uploadedImages.Add(new Image
                    {
                        AlbumId = albumId,
                        ImgUrl = uploadResult.SecureUri.ToString(),
                        PostedAt = DateTime.UtcNow.AddHours(7),
                        Caption = caption,
                        
                    });
                }
            }

            if (uploadedImages.Count > 0)
            {
                _context.Images.AddRange(uploadedImages);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
        public Image GetImageById(int id)
        {
            return _context.Images.FirstOrDefault(u => u.ImageId == id);
        }

        public async Task<bool> DeleteImageAsync(int imageId)
        {
            // Tìm ảnh trong cơ sở dữ liệu
            var image = await _context.Images.FindAsync(imageId);
            if (image == null)
            {
                Console.WriteLine("Image not found in database");
                return false;
            }

            if (!string.IsNullOrEmpty(image.ImgUrl))
            {
                // Lấy publicId từ URL
                var publicId = GetPublicIdFromUrl(image.ImgUrl);
                Console.WriteLine($"Extracted Public ID: {publicId}");

                // Xóa ảnh trên Cloudinary
                var deletionParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image // Đảm bảo đúng loại tài nguyên
                };

                var result = await _cloudinary.DestroyAsync(deletionParams);
                Console.WriteLine($"Cloudinary Delete Result: {result.Result}");


                if (result.Result != "ok" && result.Result != "not found")
                {
                    Console.WriteLine("Failed to delete from Cloudinary");
                    return false;
                }
            }

            // Xóa bản ghi trong cơ sở dữ liệu
            _context.Images.Remove(image);
            await _context.SaveChangesAsync();

            return true;
        }


        private string GetPublicIdFromUrl(string url)
        {
            try
            {
                // Tách URL thành các phần dựa trên "/upload/"
                var parts = url.Split(new[] { "/upload/" }, StringSplitOptions.None);

                if (parts.Length > 1)
                {
                    // Lấy phần sau "/upload/"
                    var publicIdWithVersion = parts[1];

                    // Loại bỏ version nếu có (phần bắt đầu với "v" và tiếp theo là số)
                    var segments = publicIdWithVersion.Split('/');
                    if (segments.Length > 1 && segments[0].StartsWith("v"))
                    {
                        // Bỏ qua "v<number>" để lấy đúng publicId
                        return string.Join("/", segments.Skip(1));
                    }

                    return publicIdWithVersion;
                }

                throw new Exception("Invalid URL format");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting public ID: {ex.Message}");
                return string.Empty;
            }
        }


    }
}
