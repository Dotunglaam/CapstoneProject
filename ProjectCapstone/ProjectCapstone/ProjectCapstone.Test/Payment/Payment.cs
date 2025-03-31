using Moq;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ProjectCapstone.Controllers;  // Adjust to your project's namespace
using Microsoft.EntityFrameworkCore;
using BusinessObject.Models;
using BusinessObject.DTOS;
using System.Linq.Expressions;
using Moq.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;
using System.Web.Http.Results;

namespace ProjectCapstone.Test.Payments;

public class PaymentControllerTests
{
    private readonly Mock<DbSet<Payment>> _mockPaymentDbSet;
    private readonly Mock<DbSet<Child>> _mockChildDbSet;
    private readonly Mock<kmsContext> _mockDbContext;
    private readonly PaymentController _controller;

    public PaymentControllerTests()
    {
        // Create a mock of DbContext
        _mockDbContext = new Mock<kmsContext>();

        // Create some mock data
        var mockPayments = new List<Payment>
            {
                new Payment
                {
                    StudentId = 1,
                    PaymentDate = DateOnly.FromDateTime(DateTime.Now),
                    TotalAmount = 2000000,
                    PaymentName = "Tuition",
                    Tuition = new Tuition
                    {
                        TuitionId = 1,
                        TuitionFee = 2000000,
                        Discount = new Discount
                        {
                            DiscountId = 1,
                            Number = 1,
                            Discount1 = 10
                        },
                        Semester = new Semester
                        {
                            SemesterId = 1,
                            Name = "Fall 2024"
                        }
                    },
                    Student = new Child
                    {
                        StudentId = 1,
                        FullName = "John Doe",
                        ParentId = 1,
                        Parent = new Parent { Name = "Jane Doe" }
                    }
                }
            }.AsQueryable();

        _mockPaymentDbSet = new Mock<DbSet<Payment>>();
        _mockPaymentDbSet.As<IQueryable<Payment>>().Setup(m => m.Provider).Returns(mockPayments.Provider);
        _mockPaymentDbSet.As<IQueryable<Payment>>().Setup(m => m.Expression).Returns(mockPayments.Expression);
        _mockPaymentDbSet.As<IQueryable<Payment>>().Setup(m => m.ElementType).Returns(mockPayments.ElementType);
        _mockPaymentDbSet.As<IQueryable<Payment>>().Setup(m => m.GetEnumerator()).Returns(mockPayments.GetEnumerator());

        // Mocking Children DbSet
        _mockChildDbSet = new Mock<DbSet<Child>>();
        var mockChildren = new List<Child>
            {
                new Child
                {
                    StudentId = 1,
                    FullName = "John Doe"
                }
            }.AsQueryable();
        _mockChildDbSet.As<IQueryable<Child>>().Setup(m => m.Provider).Returns(mockChildren.Provider);
        _mockChildDbSet.As<IQueryable<Child>>().Setup(m => m.Expression).Returns(mockChildren.Expression);
        _mockChildDbSet.As<IQueryable<Child>>().Setup(m => m.ElementType).Returns(mockChildren.ElementType);
        _mockChildDbSet.As<IQueryable<Child>>().Setup(m => m.GetEnumerator()).Returns(mockChildren.GetEnumerator());

        // Setup DbContext to return the mock DbSets
        _mockDbContext.Setup(c => c.Payments).Returns(_mockPaymentDbSet.Object);
        _mockDbContext.Setup(c => c.Children).Returns(_mockChildDbSet.Object);

        // Initialize the controller
        _controller = new PaymentController(null ,_mockDbContext.Object);
    }
    [Fact]
    public async Task GetDiscount_ShouldReturnDiscount_WhenDiscountExists()
    {
        // Arrange
        var mockContext = new Mock<kmsContext>();
        var mockConfig = new Mock<IConfiguration>();

        var discountId = 1;
        var discount = new Discount
        {
            DiscountId = discountId,
            Number = 10,
            Discount1 = 5
        };

        // Mock the DbSet<Discount> and setup FindAsync to return the mock discount object
        mockContext.Setup(m => m.Discounts.FindAsync(discountId))
                   .ReturnsAsync(discount);

        var controller = new PaymentController(mockConfig.Object, mockContext.Object);

        // Act
        var result = await controller.GetDiscount(discountId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Discount>>(result);
        var returnValue = Assert.IsType<Discount>(actionResult.Value); // Ensure the result is a Discount object
        Assert.Equal(discountId, returnValue.DiscountId); // Check that the returned discount has the correct ID
    }

    [Fact]
    public async Task GetAllDiscount_ShouldReturnAllDiscounts_WhenDiscountsExist()
    {
        // Arrange
        var mockContext = new Mock<kmsContext>();
        var mockConfig = new Mock<IConfiguration>();

        var discounts = new List<Discount>
        {
            new Discount { DiscountId = 1, Number = 10, Discount1 = 5 },
            new Discount { DiscountId = 2, Number = 15, Discount1 = 7 },
            new Discount { DiscountId = 3, Number = 20, Discount1 = 10 }
        };

        // Mock the DbSet<Discount> to return the list of discounts
        var mockDbSet = new Mock<DbSet<Discount>>();
        mockDbSet.As<IQueryable<Discount>>().Setup(m => m.Provider).Returns(discounts.AsQueryable().Provider);
        mockDbSet.As<IQueryable<Discount>>().Setup(m => m.Expression).Returns(discounts.AsQueryable().Expression);
        mockDbSet.As<IQueryable<Discount>>().Setup(m => m.ElementType).Returns(discounts.AsQueryable().ElementType);
        mockDbSet.As<IQueryable<Discount>>().Setup(m => m.GetEnumerator()).Returns(discounts.GetEnumerator());

        mockContext.Setup(m => m.Discounts).Returns(mockDbSet.Object);

        var controller = new PaymentController(mockConfig.Object, mockContext.Object);

        // Act
        var result = await controller.GetAllDiscount();

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result); // Ensure we got an Ok response
        var returnValue = Assert.IsAssignableFrom<IEnumerable<Discount>>(actionResult.Value); // Ensure the value is a list of discounts
        Assert.Equal(discounts.Count, returnValue.Count()); // Verify that the count of returned discounts matches
    }

    [Fact]
    public async Task GetAllDiscount_ShouldReturnEmpty_WhenNoDiscountsExist()
    {
        // Arrange
        var mockContext = new Mock<kmsContext>();
        var mockConfig = new Mock<IConfiguration>();

        // Mock an empty list of discounts
        var discounts = new List<Discount>();

        var mockDbSet = new Mock<DbSet<Discount>>();
        mockDbSet.As<IQueryable<Discount>>().Setup(m => m.Provider).Returns(discounts.AsQueryable().Provider);
        mockDbSet.As<IQueryable<Discount>>().Setup(m => m.Expression).Returns(discounts.AsQueryable().Expression);
        mockDbSet.As<IQueryable<Discount>>().Setup(m => m.ElementType).Returns(discounts.AsQueryable().ElementType);
        mockDbSet.As<IQueryable<Discount>>().Setup(m => m.GetEnumerator()).Returns(discounts.GetEnumerator());

        mockContext.Setup(m => m.Discounts).Returns(mockDbSet.Object);

        var controller = new PaymentController(mockConfig.Object, mockContext.Object);

        // Act
        var result = await controller.GetAllDiscount();

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result); // Ensure we got an Ok response
        var returnValue = Assert.IsAssignableFrom<IEnumerable<Discount>>(actionResult.Value); // Ensure the value is a list of discounts
        Assert.Empty(returnValue); // Verify that the returned list is empty
    }
    [Fact]
    public async Task UpdateDiscount_ShouldReturnNoContent_WhenValidDiscountProvided()
    {
        // Arrange
        var mockContext = new Mock<kmsContext>();
        var mockConfig = new Mock<IConfiguration>();

        var discountDTO = new DiscountDTO
        {
            DiscountId = 1,
            Number = 10,
            Discount1 = 5
        };

        var existingDiscount = new Discount
        {
            DiscountId = 1,
            Number = 5,
            Discount1 = 2
        };

        // Mock the DbSet<Discount> to return the existing discount
        var mockDbSet = new Mock<DbSet<Discount>>();
        mockDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(existingDiscount);

        // Set up the context to return the mock DbSet
        mockContext.Setup(m => m.Discounts).Returns(mockDbSet.Object);

        // Set up SaveChangesAsync to simulate saving the changes
        mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var controller = new PaymentController(mockConfig.Object, mockContext.Object);

        // Act
        var result = await controller.UpdateDiscount(discountDTO);

        // Assert
        var actionResult = Assert.IsType<NoContentResult>(result);  // Ensure the result is of type NoContentResult

        // Verify that the existingDiscount has been updated
        Assert.Equal(discountDTO.Number, existingDiscount.Number);
        Assert.Equal(discountDTO.Discount1, existingDiscount.Discount1);
    }
    [Fact]
    public async Task DeleteDiscount_ShouldReturnNoContent_WhenDiscountFoundAndDeleted()
    {
        // Arrange
        var mockContext = new Mock<kmsContext>();
        var mockConfig = new Mock<IConfiguration>();

        var discountId = 1;

        var existingDiscount = new Discount
        {
            DiscountId = discountId,
            Number = 10,
            Discount1 = 5
        };

        // Mock the DbSet<Discount> to return the existing discount
        var mockDbSet = new Mock<DbSet<Discount>>();
        mockDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync(existingDiscount);

        // Set up the context to return the mock DbSet
        mockContext.Setup(m => m.Discounts).Returns(mockDbSet.Object);

        // Set up SaveChangesAsync to simulate saving the changes (deleting the discount)
        mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var controller = new PaymentController(mockConfig.Object, mockContext.Object);

        // Act
        var result = await controller.DeleteDiscount(discountId);

        // Assert
        var actionResult = Assert.IsType<NoContentResult>(result);  // Ensure the result is NoContentResult (HTTP 204)

        // Verify that the discount has been deleted
        mockContext.Verify(m => m.Discounts.Remove(It.Is<Discount>(d => d.DiscountId == discountId)), Times.Once);
        mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task GetPaymentHistory_ShouldReturnNotFound_WhenNoChildrenFound()
    {
        // Arrange
        var parentId = 1;

        var mockContext = new Mock<kmsContext>();
        var mockConfig = new Mock<IConfiguration>();

        // Mock DbSet<Children> to return an empty list
        mockContext.Setup(x => x.Children).ReturnsDbSet(new List<Child>());

        var controller = new PaymentController(mockConfig.Object, mockContext.Object);

        // Act
        var result = await controller.GetPaymentHistory(parentId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
    }
    [Fact]
    public async Task GetPaymentHistory_ShouldReturnNotFound_WhenNoPaymentHistoryFound()
    {
        // Arrange
        var parentId = 1;

        var children = new List<Child>
    {
        new Child { StudentId = 1, ParentId = parentId, FullName = "Child 1" }
    };

        var mockContext = new Mock<kmsContext>();
        var mockConfig = new Mock<IConfiguration>();

        // Mock DbSet<Children> to return the children list
        mockContext.Setup(x => x.Children).ReturnsDbSet(children);

        // Mock DbSet<Payments> to return an empty list
        mockContext.Setup(x => x.Payments).ReturnsDbSet(new List<Payment>());

        var controller = new PaymentController(mockConfig.Object, mockContext.Object);

        // Act
        var result = await controller.GetPaymentHistory(parentId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
    }
    [Fact]
    public async Task GetPaymentHistory_ShouldReturnPaymentHistory_WhenValidDataExists()
    {
        var paymentDate = new DateTime(2024, 11, 1); // Original DateTime value
        var dateOnlyValue = DateOnly.FromDateTime(paymentDate); // Convert to DateOnly

        // Arrange
        var parentId = 1;

        var children = new List<Child>
    {
        new Child { StudentId = 1, ParentId = parentId, FullName = "Child 1" },
        new Child { StudentId = 2, ParentId = parentId, FullName = "Child 2" }
    };

        var payments = new List<Payment>
    {
        new Payment
        {
            PaymentId = 1,
            StudentId = 1,
            PaymentDate = dateOnlyValue,
            TotalAmount = 100000,
            PaymentName = "Tuition",
            Status = 1,
            Tuition = new Tuition
            {
                TuitionId = 1,
                StudentId = 1,
                SemesterId = 1,
                TuitionFee = 100000,
                Semester = new Semester
                {
                    SemesterId = 1,
                    Name = "Semester 1"
                },
                Discount = new Discount
                {
                    DiscountId = 1,
                    Discount1 = 5
                }
            }
        }
    };

        var mockContext = new Mock<kmsContext>();
        var mockConfig = new Mock<IConfiguration>();

        // Mock DbSet<Children> to return the children list
        mockContext.Setup(x => x.Children).ReturnsDbSet(children);

        // Mock DbSet<Payments> to return the payments list
        mockContext.Setup(x => x.Payments).ReturnsDbSet(payments);

        // Mock DbSet<Checkservices> if required
        mockContext.Setup(x => x.Checkservices).ReturnsDbSet(new List<Checkservice>());

        var controller = new PaymentController(mockConfig.Object, mockContext.Object);

        // Act
        var result = await controller.GetPaymentHistory(parentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

       
    }

}
