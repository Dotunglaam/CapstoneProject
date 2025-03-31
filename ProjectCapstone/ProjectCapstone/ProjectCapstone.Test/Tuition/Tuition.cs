using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.EntityFrameworkCore;
using Respository.Interfaces;
using Respository.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ProjectCapstone.Test.tuitions;

public class TuitionServiceTests
{
    private readonly Mock<kmsContext> _mockContext;
    private readonly Mock<IResetPasswordTokenRepository> _mockResetPasswordToken;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public TuitionServiceTests()
    {
        _mockContext = new Mock<kmsContext>();
        _mockResetPasswordToken = new Mock<IResetPasswordTokenRepository>();
        _mockConfiguration = new Mock<IConfiguration>();
    }

    [Fact]
    public async Task GenerateMonthlyTuitionRecords_ShouldNotGenerateNewRecord_WhenRecordExistsForNextMonth()
    {
        // Arrange
        var now = DateTime.Now;
        var nextMonth = now.AddMonths(1);

        // Mock Semesters
        var currentSemester = new Semester
        {
            SemesterId = 1,
            StartDate = new DateTime(now.Year, 9, 1),
            EndDate = new DateTime(now.Year, 12, 31),
            Status = 1
        };

        _mockContext.Setup(c => c.SaveChangesAsync(CancellationToken.None));


        // Mock Children
        var child = new Child
        {
            StudentId = 1,
            GradeId = 1,
            FullName = "Child Test",
            Status = 1
        };

        _mockContext.Setup(c => c.SaveChangesAsync(CancellationToken.None));


        // Mock Tuition records (simulate an existing record)
        var existingTuitionRecord = new Tuition
        {
            StudentId = 1,
            SemesterId = 1,
            StartDate = new DateTime(nextMonth.Year, nextMonth.Month, 1),
            EndDate = new DateTime(nextMonth.Year, nextMonth.Month, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month)),
            DueDate = new DateOnly(nextMonth.Year, nextMonth.Month, 5),
            IsPaid = 0,
            StatusTuitionLate = 0
        };

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);


        // Mock DbSet for Tuitions (No new tuition record will be added)
        var mockDbSet = new Mock<DbSet<Tuition>>();
        _mockContext.Setup(c => c.Tuitions).Returns(mockDbSet.Object);

        // Mock SaveChangesAsync
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var tuitionService = new TuitionService(_mockContext.Object, _mockResetPasswordToken.Object, _mockConfiguration.Object);

        // Act
        await tuitionService.GenerateMonthlyTuitionRecords();

        // Assert that SaveChangesAsync is NOT called when a tuition record already exists for the next month
        mockDbSet.Verify(m => m.AddAsync(It.IsAny<Tuition>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
   
    // Helper method to mock DbSet
    private static Mock<DbSet<T>> MockDbSet<T>(List<T> list) where T : class
    {
        var queryable = list.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        return mockSet;
    }


    [Fact]
    public async Task GenerateMonthlyTuitionRecordsClick_ShouldNotGenerateNewRecord_WhenRecordExistsForNextMonth()
    {
        // Arrange
        var now = DateTime.Now;
        var nextMonth = now.AddMonths(1);

        // Mock Semesters
        var currentSemester = new Semester
        {
            SemesterId = 1,
            StartDate = new DateTime(now.Year, 9, 1),
            EndDate = new DateTime(now.Year, 12, 31),
            Status = 1
        };

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);


        // Mock Children
        var child = new Child
        {
            StudentId = 1,
            GradeId = 1,
            FullName = "Child Test",
            Status = 1
        };

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);


        // Mock Tuition records (simulate an existing record for the next month)
        var existingTuitionRecord = new Tuition
        {
            StudentId = 1,
            SemesterId = 1,
            StartDate = new DateTime(nextMonth.Year, nextMonth.Month, 1),
            EndDate = new DateTime(nextMonth.Year, nextMonth.Month, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month)),
            DueDate = new DateOnly(nextMonth.Year, nextMonth.Month, 5),
            IsPaid = 0,
            StatusTuitionLate = 0
        };

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);


        // Mock SaveChangesAsync
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(1);

        var tuitionService = new TuitionService(_mockContext.Object, _mockResetPasswordToken.Object, _mockConfiguration.Object);

        // Act
        await tuitionService.GenerateMonthlyTuitionRecordsClick();

        // Assert that AddAsync is not called, since a record already exists for the next month
        _mockContext.Verify(c => c.Tuitions.AddAsync(It.IsAny<Tuition>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    

}
