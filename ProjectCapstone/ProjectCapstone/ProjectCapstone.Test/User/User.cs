using Moq;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Xunit;
using System;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Respository.Services;

namespace ProjectCapstone.Test.Users;

public class UserRepositoryTests
{
    private readonly Mock<kmsContext> _mockContext;
    private readonly Mock<Cloudinary> _mockCloudinary;
    private readonly Mock<UserDAO> _mockUserDAO;
    private readonly UserRepository _userRepository;

    public UserRepositoryTests()
    {
        // Mock the DbContext
        _mockContext = new Mock<kmsContext>();

        // Mock Cloudinary class
        // Use MockBehavior.Strict to ensure only methods set up will be allowed to be called
        _mockCloudinary = new Mock<Cloudinary>(MockBehavior.Strict, new Account("dj0idln0z", "743697212891634", "SLYsBY5DOLquYBxjl0rUPbOAX1U"));

        // Mock UserDAO
        _mockUserDAO = new Mock<UserDAO>();

        // Initialize the UserRepository with the mocked dependencies
        _userRepository = new UserRepository(_mockContext.Object, _mockCloudinary.Object, null);
    }

    [Fact]
    public void Add_ShouldAddUserToContextAndCommit()
    {
        // Arrange: Set up the mock for DbSet<User> inside the context
        var mockDbSet = new Mock<DbSet<User>>();
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        // Create a new User instance to be added
        var user = new User
        {
            UserId = 1,
            Firstname = "John",
            LastName = "Doe",
            Mail = "john.doe@example.com"
        };

        // Act: Call the Add method on the repository
        _userRepository.Add(user);

        // Assert: Verify the Add method on the DbSet is called once
        mockDbSet.Verify(m => m.Add(It.IsAny<User>()), Times.Once);

        // Verify that SaveChanges (Commit) is called once on the context
        _mockContext.Verify(m => m.SaveChanges(), Times.Once);
    }
    [Fact]
    public void GetAllUsers_ShouldReturnListOfUsers()
    {
        // Arrange: Create a list of mock users to return
        var users = new List<User>
        {
            new User { UserId = 1, Firstname = "John", LastName = "Doe", Mail = "john.doe@example.com" },
            new User { UserId = 2, Firstname = "Jane", LastName = "Smith", Mail = "jane.smith@example.com" }
        };

        // Mock the DbSet<User> to return the users list
        var mockDbSet = new Mock<DbSet<User>>();
        mockDbSet.As<IQueryable<User>>()
                 .Setup(m => m.Provider).Returns(users.AsQueryable().Provider);
        mockDbSet.As<IQueryable<User>>()
                 .Setup(m => m.Expression).Returns(users.AsQueryable().Expression);
        mockDbSet.As<IQueryable<User>>()
                 .Setup(m => m.ElementType).Returns(users.AsQueryable().ElementType);
        mockDbSet.As<IQueryable<User>>()
                 .Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        // Setup the mock context to return the mock DbSet
        _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

        // Act: Call the GetAllUsers method
        var result = _userRepository.GetAllUsers();

        // Assert: Verify that the correct users are returned
        Assert.Equal(2, result.Count());
        Assert.Contains(result, user => user.Firstname == "John" && user.LastName == "Doe");
        Assert.Contains(result, user => user.Firstname == "Jane" && user.LastName == "Smith");

        // Verify the method calls to ensure interaction with the mock context
        _mockContext.Verify(c => c.Users, Times.Once);
    }
    
}
