using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataAccess.DAO;
using BusinessObject.Models;
using BusinessObject.DTOS;
using AutoMapper;

namespace ProjectCapstone.Test.Children;

public class GetAllChildrenTests
{
    private readonly Mock<DbSet<Child>> _mockChildrenDbSet;
    private readonly Mock<kmsContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ChildrenDAO _childrenDAO;

    public GetAllChildrenTests()
    {
        // Mock DbSet<Child>
        _mockChildrenDbSet = new Mock<DbSet<Child>>();

        // Mock Context
        _mockContext = new Mock<kmsContext>();
        _mockContext.Setup(c => c.Children).Returns(_mockChildrenDbSet.Object);

        // Mock Mapper
        _mockMapper = new Mock<IMapper>();

        // Initialize DAO
        _childrenDAO = new ChildrenDAO(_mockContext.Object, _mockMapper.Object, null);
    }

    [Fact]
    public async Task GetAllChildren_ReturnsChildrenList()
    {
        // Arrange
        var childrenData = new List<Child>
    {
        new Child
        {
            StudentId = 1,
            Code = "ST0001",
            FullName = "John Doe",
            NickName = "Johnny",
            GradeId = 1,
            Dob = new DateTime(2015, 5, 1),
            Gender = 1,
            Status = 1,
            EthnicGroups = "Group1",
            Nationality = "US",
            Religion = "None",
            ParentId = 101,
            Avatar = "avatar1.png"
        }
    };

        var mockDbSet = childrenData.AsQueryableDbSet();
        _mockContext.Setup(c => c.Children).Returns(mockDbSet.Object);

        // Act
        var result = await _childrenDAO.GetAllChildren();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("John Doe", result.First().FullName);
    }

    [Fact]
    public async Task GetAllChildren_ReturnsEmptyList_WhenNoData()
    {
        // Arrange
        var childrenData = new List<Child>(); // Empty list
        var mockDbSet = childrenData.AsQueryableDbSet();
        _mockContext.Setup(c => c.Children).Returns(mockDbSet.Object);

        // Act
        var result = await _childrenDAO.GetAllChildren();

        // Assert
        Assert.NotNull(result); // Ensure result is not null
        Assert.Empty(result);   // Verify that the result is an empty list
    }

}
