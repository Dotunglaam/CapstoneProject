using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Respository.Interfaces;
using Respository.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ProjectCapstone.Test.Albums;

public class AlbumRepositoryTests
{
    private readonly DbContextOptions<kmsContext> _dbOptions;
    private readonly kmsContext _context;

    public AlbumRepositoryTests()
    {
        var dbName = $"TestDatabase_{Guid.NewGuid()}";
        _dbOptions = new DbContextOptionsBuilder<kmsContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        _context = new kmsContext(_dbOptions);
    }

    [Theory]
    [InlineData(1, 1, "Valid Album", "Valid Description", true, true, true)]  // Valid case
    [InlineData(999, 1, "Invalid Class", "Description", false, true, false)] // Invalid ClassId
    [InlineData(1, 999, "Invalid User", "Description", true, false, false)]  // Invalid CreateBy
    [InlineData(1, 2, "Invalid User", "Description", true, false, false)]
    [InlineData(2, 1, "Invalid User", "Description", true, false, false)]
    [InlineData(2, null, "Invalid User", "Description", true, false, false)]
    [InlineData(null, null, "Invalid User", "Description", true, false, false)]
    [InlineData(null, 1, "Invalid User", "Description", true, false, false)]

    public async Task CreateAlbumAsync_ShouldHandleDifferentScenarios(
        int classId, int createBy, string albumName, string description,
        bool isClassValid, bool isUserValid, bool isExpectedToSucceed)
    {
        // Arrange
        if (isClassValid)
        {
            _context.Classes.Add(new Class { ClassId = classId });
        }

        if (isUserValid)
        {
            _context.Users.Add(new User
            {
                UserId = createBy,
                Mail = "testuser@example.com",
                Password = "hashedpassword", // Provide dummy valid values
                SaltKey = "somesalt"
            });
        }

        await _context.SaveChangesAsync();

        var repository = new AlbumRespository(_context);

        var albumDto = new AlbumCreateDto
        {
            ClassId = classId,
            CreateBy = createBy,
            AlbumName = albumName,
            Description = description
        };

        // Act & Assert
        if (isExpectedToSucceed)
        {
            var album = await repository.CreateAlbumAsync(albumDto);

            // Verify successful album creation
            Assert.NotNull(album);
            Assert.Equal(albumName, album.AlbumName);
            Assert.Equal(description, album.Description);
            Assert.Equal(classId, album.ClassId);
            Assert.Equal(createBy, album.CreateBy);
        }
        else
        {
            // Expect an exception for invalid inputs
            await Assert.ThrowsAsync<Exception>(
                () => repository.CreateAlbumAsync(albumDto));
        }
    }

    [Theory]
    [InlineData(1, 1, "Test Album 1", "Description 1", 1, 1, "No reason", true)]
    [InlineData(2, 2, "Test Album 2", "Description 2", 1, 0, "Reason for Album 2", false)]
    [InlineData(3, 3, "Test Album 3", "Description 3", 0, 1, "Reason for Album 3", false)]
    [InlineData(999, 1, "Invalid Album", "Invalid Description", 1, 0, "Reason for Invalid", false)] // Non-existing Album
    [InlineData(null, 1, "Invalid Album", "Invalid Description", 1, 0, "Reason for Invalid", false)] // Non-existing Album
    [InlineData(null,  null, "Invalid Album", "Invalid Description", 1, 0, "Reason for Invalid", false)] // Non-existing Album
    [InlineData(1, null, "Invalid Album", "Invalid Description", 1, 0, "Reason for Invalid", false)] // Non-existing Album

    public async Task GetAlbumByIdAsync_ShouldHandleMultipleScenarios(int albumId, int classId, string albumName, string description, int status, int isActive, string reason, bool isExpectedToSucceed)
    {
        // Arrange
        if (isExpectedToSucceed)
        {
            var album = new Album
            {
                AlbumId = albumId,
                ClassId = classId,
                CreateBy = classId,
                ModifiBy = classId,
                AlbumName = albumName,
                TimePost = DateTime.Now,
                Description = description,
                Status = status,
                IsActive = isActive,
                Reason = reason
            };

            var @class = new Class { ClassId = classId };
            _context.Classes.Add(@class);
            _context.Albums.Add(album);
            await _context.SaveChangesAsync();
        }

        var repository = new AlbumRespository(_context);

        // Act
        var result = await repository.GetAlbumByIdAsync(albumId);

        // Assert
        if (isExpectedToSucceed)
        {
            Assert.NotNull(result);
            Assert.Equal(albumId, result.AlbumId);
            Assert.Equal(classId, result.ClassId);
            Assert.Equal(albumName, result.AlbumName);
            Assert.Equal(description, result.Description);
            Assert.Equal(status, result.Status);
            Assert.Equal(isActive, result.isActive);
            Assert.Equal(reason, result.Reason);
        }
        else
        {
            Assert.Null(result);
        }
    }
    [Fact]
    public async Task GetAllAlbumsAsync_ShouldReturnAlbums_WithSpecificFields()
    {
        // Arrange: Manually add test data within the test method
        _context.Classes.AddRange(
            new Class { ClassId = 1, ClassName = "Class A" },
            new Class { ClassId = 2, ClassName = "Class B" }
        );

        _context.Users.AddRange(
            new User { UserId = 1, Mail = "testuser@example.com", Password = "password", SaltKey = "salt" },
            new User { UserId = 2, Mail = "user2@example.com", Password = "password", SaltKey = "salt" }
        );

        _context.Albums.AddRange(
            new Album
            {
                AlbumId = 1,
                ClassId = 1,
                CreateBy = 1,
                ModifiBy = 1,
                AlbumName = "Album 1",
                TimePost = DateTime.Now,
                Description = "Description 1",
                Status = 1,
                IsActive = 1,
                Reason = "Reason 1"
            },
            new Album
            {
                AlbumId = 2,
                ClassId = 2,
                CreateBy = 2,
                ModifiBy = 2,
                AlbumName = "Album 2",
                TimePost = DateTime.Now,
                Description = "Description 2",
                Status = 0,
                IsActive = 0,
                Reason = "Reason 2"
            }
        );

        await _context.SaveChangesAsync();  // Ensure the data is persisted

        var repository = new AlbumRespository(_context);

        // Act: Retrieve all albums
        var albums = await repository.GetAllAlbumsAsync();

        // Assert: Check if the albums are retrieved and the fields are correct
        Assert.NotNull(albums);
        Assert.Equal(2, albums.Count());

        var album1 = albums.FirstOrDefault(a => a.AlbumId == 1);
        var album2 = albums.FirstOrDefault(a => a.AlbumId == 2);

        // Assert for Album 1
        Assert.NotNull(album1);
        Assert.Equal(1, album1.ClassId);
        Assert.Equal(1, album1.CreateBy);
        Assert.Equal(1, album1.ModifiBy);
        Assert.Equal("Album 1", album1.AlbumName);
        Assert.NotNull(album1.TimePost);
        Assert.Equal("Description 1", album1.Description);
        Assert.Equal(1, album1.Status);
        Assert.Equal(1, album1.isActive);
        Assert.Equal("Reason 1", album1.Reason);

        // Assert for Album 2
        Assert.NotNull(album2);
        Assert.Equal(2, album2.ClassId);
        Assert.Equal(2, album2.CreateBy);
        Assert.Equal(2, album2.ModifiBy);
        Assert.Equal("Album 2", album2.AlbumName);
        Assert.NotNull(album2.TimePost);
        Assert.Equal("Description 2", album2.Description);
        Assert.Equal(0, album2.Status);
        Assert.Equal(0, album2.isActive);
        Assert.Equal("Reason 2", album2.Reason);
    }
    public static IEnumerable<object[]> GetUpdateAlbumTestData()
    {
        return new List<object[]>
    {
        // Successful update case
        new object[] { 1, 1, "New Album Name", "New Description", true, true, true },

        // Album does not exist
        new object[] { 99, 1, "New Album Name", "New Description", false, true, false },

        // Class does not exist
        new object[] { 1, 99, "New Album Name", "New Description", true, false, false },

        // Both album and class exist but null fields
        new object[] { 1, 1, null, null, true, true, true },

        // Neither album nor class exists
        new object[] { 99, 99, "New Album Name", "New Description", false, false, false },
        
        new object[] { 1, null, "New Album Name", "New Description", false, false, false },

    };
    }


    [Theory]
    [MemberData(nameof(GetUpdateAlbumTestData))]
    public async Task UpdateAlbumAsync_ShouldHandleVariousScenarios(
    int albumId,
    int classId,
    string albumName,
    string description,
    bool doesAlbumExist,
    bool doesClassExist,
    bool isExpectedToSucceed)
    {
        // Arrange
        var dbOptions = new DbContextOptionsBuilder<kmsContext>()
            .UseInMemoryDatabase($"UpdateAlbumTestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new kmsContext(dbOptions);

        // Seed data if necessary
        if (doesAlbumExist)
        {
            context.Albums.Add(new Album
            {
                AlbumId = albumId,
                ClassId = 1, // Default valid class ID
                AlbumName = "Old Album Name",
                Description = "Old Description"
            });
        }

        if (doesClassExist)
        {
            context.Classes.Add(new Class
            {
                ClassId = classId,
                ClassName = "Class A"
            });
        }

        await context.SaveChangesAsync();

        var repository = new AlbumRespository(context);

        var updateDto = new AlbumUpdateDto
        {
            AlbumId = albumId,
            ClassId = classId,
            AlbumName = albumName,
            Description = description
        };

        // Act
        if (isExpectedToSucceed)
        {
            var result = await repository.UpdateAlbumAsync(updateDto);

            // Assert
            Assert.True(result);

            var updatedAlbum = await context.Albums.FindAsync(albumId);
            Assert.NotNull(updatedAlbum);
            Assert.Equal(albumName, updatedAlbum.AlbumName);
            Assert.Equal(description, updatedAlbum.Description);
        }
        else
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpdateAlbumAsync(updateDto));
        }
    }
    [Theory]
    [InlineData(1, 1, 0, true)] // Toggle from active (1) to inactive (0)
    [InlineData(2, 0, 1, true)] // Toggle from inactive (0) to active (1)
    public async Task DeleteAlbumAsync_ShouldToggleIsActive(int albumId, int initialIsActive, int expectedIsActive, bool shouldSucceed)
    {
        // Arrange
        var options = new DbContextOptionsBuilder<kmsContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new kmsContext(options);

        // Seed data if the album should exist
        if (shouldSucceed)
        {
            var album = new Album
            {
                AlbumId = albumId,
                IsActive = initialIsActive,
                AlbumName = "Test Album",
                ClassId = 1,
                CreateBy = 1,
                ModifiBy = 1,
                Status = 1,
                Description = "Test Description"
            };
            context.Albums.Add(album);
            await context.SaveChangesAsync();
        }

        var repository = new AlbumRespository(context);

        // Act
        var result = await repository.DeleteAlbumAsync(albumId);

        // Assert
        if (shouldSucceed)
        {
            Assert.True(result);

            var updatedAlbum = await context.Albums.FindAsync(albumId);
            Assert.NotNull(updatedAlbum);
            Assert.Equal(expectedIsActive, updatedAlbum.IsActive);
        }
        else
        {
            Assert.False(result);
        }
    }
    [Theory]
    [InlineData(1, 1, null, true)] // Update status to 1 and set TimePost
    [InlineData(2, 2, "Rejected for testing purposes", true)] // Update status to 2 and set Reason
    [InlineData(3, 2, null, true)] // Invalid status, album should not update
    [InlineData(4, 1, null, false)] // Album does not exist
    [InlineData(null, 1, null, false)] 
    [InlineData(4, null, null, false)] 
    [InlineData(null, null, null, false)]
    public async Task UpdateStatusOfAlbum_ShouldHandleVariousScenarios(int albumId, int? status, string? reason, bool isExpectedToSucceed)
    {
        // Arrange
        var options = new DbContextOptionsBuilder<kmsContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        using var context = new kmsContext(options);

        // Seed data if album exists
        if (albumId < 4)
        {
            var album = new Album
            {
                AlbumId = albumId,
                Status = 0,
                AlbumName = "Test Album",
                ClassId = 1,
                CreateBy = 1,
                ModifiBy = 1,
                Description = "Test Description"
            };
            context.Albums.Add(album);
            await context.SaveChangesAsync();
        }

        var repository = new AlbumRespository(context);

        var updateDto = new UpdateStatusOfAlbum
        {
            AlbumId = albumId,
            Status = status,
            Reason = reason
        };

        // Act
        if (isExpectedToSucceed)
        {
            var result = await repository.UpdateStatusOfAlbum(updateDto);

            // Assert
            Assert.True(result);

            var updatedAlbum = await context.Albums.FindAsync(albumId);
            Assert.NotNull(updatedAlbum);
            Assert.Equal(status, updatedAlbum.Status);

            if (status == 1)
            {
                Assert.NotNull(updatedAlbum.TimePost);
            }
            if (status == 2)
            {
                Assert.Equal(reason, updatedAlbum.Reason);
            }
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateStatusOfAlbum(updateDto));
        }
    }

}
