using BusinessObject.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Respository.Interfaces;
using Respository.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ProjectCapstone.Test.Images;

public class ImageTest
    {
        private readonly DbContextOptions<kmsContext> _dbOptions;
        private readonly Cloudinary _cloudinary;
        private readonly Mock<Cloudinary> _mockCloudinary;
        private readonly ImageRepository _imageRepository;

    public ImageTest()
        {
            // Set up the in-memory database for testing
            _dbOptions = new DbContextOptionsBuilder<kmsContext>()
                .UseInMemoryDatabase($"ImageTestDb_{Guid.NewGuid()}")
                .Options;

        _mockCloudinary = new Mock<Cloudinary>(new Account("dj0idln0z", "743697212891634", "SLYsBY5DOLquYBxjl0rUPbOAX1U"));
        _imageRepository = new ImageRepository(new kmsContext(), _mockCloudinary.Object);

        // Mock Cloudinary, you can mock Cloudinary if needed, here we assume it’s set up properly
        _cloudinary = new Cloudinary(new Account("dj0idln0z", "743697212891634", "SLYsBY5DOLquYBxjl0rUPbOAX1U"));
        }

        [Fact]
        public async Task ListAllImagesByAlbumIdAsync_ShouldReturnImages_WhenAlbumExists()
        {
            // Arrange: Seed data
            using (var context = new kmsContext(_dbOptions))
            {
                context.Images.Add(new Image
                {
                    ImageId = 1,
                    AlbumId = 1,
                    ImgUrl = "http://image1.url",
                    Caption = "Image 1 description"
                });
                context.Images.Add(new Image
                {
                    ImageId = 2,
                    AlbumId = 1,
                    ImgUrl = "http://image2.url",
                    Caption = "Image 2 description"
                });
                context.Images.Add(new Image
                {
                    ImageId = 3,
                    AlbumId = 2,
                    ImgUrl = "http://image3.url",
                    Caption = "Image 3 description"
                });
                await context.SaveChangesAsync();
            }

            using (var context = new kmsContext(_dbOptions))
            {
                var imageRepository = new ImageRepository(context, _cloudinary);

                // Act: Call the method to list all images by AlbumId 1
                var images = await imageRepository.ListAllImagesByAlbumIdAsync(1);

                // Assert: Verify that the correct images are returned
                Assert.NotNull(images);
                Assert.Equal(2, images.Count()); // There should be 2 images with AlbumId 1
                Assert.All(images, img => Assert.Equal(1, img.AlbumId)); // All images should have AlbumId = 1
            }
        }

        [Fact]
        public async Task ListAllImagesByAlbumIdAsync_ShouldReturnEmpty_WhenNoImagesForAlbum()
        {
            // Arrange: No images are added for albumId 3
            using (var context = new kmsContext(_dbOptions))
            {
                await context.SaveChangesAsync();
            }

            using (var context = new kmsContext(_dbOptions))
            {
                var imageRepository = new ImageRepository(context, _cloudinary);

                // Act: Call the method to list all images by AlbumId 3 (non-existing album)
                var images = await imageRepository.ListAllImagesByAlbumIdAsync(3);

                // Assert: Verify that no images are returned
                Assert.NotNull(images);
                Assert.Empty(images); // No images should be returned for this albumId
            }
        }
    [Fact]
    public void GetImageById_ShouldReturnImage_WhenImageExists()
    {
        // Arrange: Setup context and repository
        var context = new kmsContext(_dbOptions);
        var imageRepo = new ImageRepository(context, _mockCloudinary.Object);

        // Add test data to in-memory database
        var testImage = new Image
        {
            ImageId = 1,
            ImgUrl = "http://example.com/image.jpg",
            Caption = "Test Image"
        };
        context.Images.Add(testImage);
        context.SaveChanges();

        // Act: Call the method to get the image by ID
        var result = imageRepo.GetImageById(1);

        // Assert: Verify the result
        Assert.NotNull(result);
        Assert.Equal(1, result.ImageId);
        Assert.Equal("http://example.com/image.jpg", result.ImgUrl);
        Assert.Equal("Test Image", result.Caption);
    }
}

