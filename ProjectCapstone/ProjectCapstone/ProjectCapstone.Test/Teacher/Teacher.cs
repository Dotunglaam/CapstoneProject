using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Repository.Interfaces;
using Repository.Services;
using Respository.Interfaces;
using Respository.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static BusinessObject.DTOS.Request;

namespace ProjectCapstone.Test.Teachers;

public class TeacherRepositoryTests
{
    private readonly DbContextOptions<kmsContext> _dbOptions;
    private readonly kmsContext _context;
    private readonly TeacherRepository _teacherRepository;

    public TeacherRepositoryTests()
    {
        var dbName = $"TestDatabase_{Guid.NewGuid()}";
        _dbOptions = new DbContextOptionsBuilder<kmsContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.DetachedLazyLoadingWarning)) // Bỏ qua cảnh báo LazyLoading
            .Options;

        _context = new kmsContext(_dbOptions);
    }

   [Fact]
    public async Task GetAllTeachersAsync_ShouldReturnAllTeachers()
    {
        // Arrange
        var teacher = new Teacher
        {
            Name = "John Doe",
            Education = "Masters",
            Experience = "5 years",
            TeacherNavigation = new User
            {
                Firstname = "John",
                LastName = "Doe",
                Mail = "john.doe@example.com",
                Status = 1,
                Gender = 1,
                Dob = new DateTime(1985, 10, 5),
                Avatar = "avatar.jpg",
                Password = "hashedPassword123",  // Thêm giá trị cho Password
                SaltKey = "randomSaltKey"       // Thêm giá trị cho SaltKey
            }
        };

        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        var repository = new TeacherRepository(_context);

        // Act
        var teachers = await repository.GetAllTeachersAsync();

        // Assert
        Assert.NotNull(teachers);
        Assert.Contains(teachers, t => t.Name == "John Doe");
    }
    [Fact]
    public async Task GetTeacherByIdAsync_ShouldReturnTeacherDetails_WhenTeacherExists()
    {
        // Arrange
        var teacher = new Teacher
        {
            Name = "John Doe",
            Education = "Masters in Education",
            Experience = "10 years",
            TeacherNavigation = new User
            {
                Firstname = "John",
                LastName = "Doe",
                Address = "123 Main St",
                PhoneNumber = "123-456-7890",
                Mail = "john.doe@example.com",
                Status = 1,
                Gender = 1,
                Dob = new DateTime(1985, 10, 5),
                Code = "T12345",
                Avatar = "avatar.jpg",
                Password = "password123",  // Cung cấp giá trị cho Password
                SaltKey = "saltKey123"    // Cung cấp giá trị cho SaltKey
            }
        };

        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        var repository = new TeacherRepository(_context);
        // Act
        var result = await repository.GetTeacherByIdAsync(teacher.TeacherId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(teacher.TeacherId, result.TeacherId);
        Assert.Equal(teacher.Name, result.Name);
        Assert.Equal(teacher.Education, result.Education);
        Assert.Equal(teacher.Experience, result.Experience);
        Assert.Equal(teacher.TeacherNavigation.Firstname, result.Firstname);
        Assert.Equal(teacher.TeacherNavigation.LastName, result.LastName);
        Assert.Equal(teacher.TeacherNavigation.Address, result.Address);
        Assert.Equal(teacher.TeacherNavigation.PhoneNumber, result.PhoneNumber);
        Assert.Equal(teacher.TeacherNavigation.Mail, result.Mail);
        Assert.Equal(teacher.TeacherNavigation.Status, result.Status);
        Assert.Equal(teacher.TeacherNavigation.Gender, result.Gender);
        Assert.Equal(teacher.TeacherNavigation.Dob, result.Dob);
        Assert.Equal(teacher.TeacherNavigation.Code, result.Code);
        Assert.Equal(teacher.TeacherNavigation.Avatar, result.Avatar);
    }

    [Fact]
    public async Task GetTeacherByIdAsync_ShouldReturnNull_WhenTeacherNotFound()
    {
        var repository = new TeacherRepository(_context);
        // Act
        var result = await repository.GetTeacherByIdAsync(999); // Assuming no teacher has this ID

        // Assert
        Assert.Null(result);
    }
    [Fact]
    public async Task SoftDeleteTeacherAsync_ShouldUpdateStatusToInactive_WhenTeacherExists()
    {
        // Arrange
        var teacher = new Teacher
        {
            Name = "John Doe",
            Education = "Masters",
            Experience = "5 years",
            TeacherNavigation = new User
            {
                Firstname = "John",
                LastName = "Doe",
                Mail = "john.doe@example.com",
                Status = 1, // Active
                Gender = 1,
                Dob = new DateTime(1985, 10, 5),
                Avatar = "avatar.jpg",
                Password = "hashedPassword123",  // Thêm giá trị cho Password
                SaltKey = "randomSaltKey"       // Thêm giá trị cho SaltKey
            }
        };

        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        var repository = new TeacherRepository(_context);

        // Act
        var result = await repository.SoftDeleteTeacherAsync(teacher.TeacherId);

        // Assert
        Assert.True(result);  // Teacher should be soft-deleted successfully
        var updatedTeacher = await _context.Teachers
                                           .Include(t => t.TeacherNavigation)
                                           .FirstOrDefaultAsync(t => t.TeacherId == teacher.TeacherId);

        Assert.NotNull(updatedTeacher);
        Assert.Equal(0, updatedTeacher.TeacherNavigation.Status);  // Status should be updated to 0 (inactive)
    }

    [Fact]
    public async Task SoftDeleteTeacherAsync_ShouldReturnFalse_WhenTeacherNotFound()
    {
        var repository = new TeacherRepository(_context);

        // Act
        var result = await repository.SoftDeleteTeacherAsync(999); // Assuming no teacher has this ID

        // Assert
        Assert.False(result);  // Should return false when teacher is not found
    }
    [Fact]
    public async Task UpdateTeacherAsync_ShouldUpdateTeacher_WhenTeacherExists()
    {
        // Arrange
        var teacher = new Teacher
        {
            Name = "John Doe",
            Education = "Masters",
            Experience = "5 years",
            TeacherNavigation = new User
            {
                Firstname = "John",
                LastName = "Doe",
                Mail = "john.doe@example.com",
                Status = 1,
                Gender = 1,
                Dob = new DateTime(1985, 10, 5),
                Avatar = "avatar.jpg",
                Password = "hashedPassword123",  // Thêm giá trị cho Password
                SaltKey = "randomSaltKey"       // Thêm giá trị cho SaltKey
            }
        };

        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        var repository = new TeacherRepository(_context);

        var teacherModel = new ViewProfileTeacherForTableTecher
        {
            TeacherId = teacher.TeacherId,
            Name = "John Updated",
            Education = "PhD",
            Experience = "10 years"
        };

        // Act
        var result = await repository.UpdateTeacherAsync(teacherModel);

        // Assert
        Assert.NotNull(result);  // Teacher should not be null
        Assert.Equal("John Updated", result.Name);  // Name should be updated
        Assert.Equal("PhD", result.Education);  // Education should be updated
        Assert.Equal("10 years", result.Experience);  // Experience should be updated
    }

    [Fact]
    public async Task UpdateTeacherAsync_ShouldReturnNull_WhenTeacherNotFound()
    {
        var repository = new TeacherRepository(_context);

        var teacherModel = new ViewProfileTeacherForTableTecher
        {
            TeacherId = 999,  // Assuming no teacher has this ID
            Name = "Non Existent Teacher",
            Education = "Unknown",
            Experience = "Unknown"
        };

        // Act
        var result = await repository.UpdateTeacherAsync(teacherModel);

        // Assert
        Assert.Null(result);  // Should return null if teacher is not found
    }
}
