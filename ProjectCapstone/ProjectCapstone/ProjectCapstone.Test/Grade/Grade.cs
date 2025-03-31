using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Respository.Services;
using System;
using System.Collections.Generic;
using BusinessObject.DTOS;
using BusinessObject.Models;
using System.ComponentModel.DataAnnotations;

namespace ProjectCapstone.Test.Grade
{
    public class GradeTests
    {
        private readonly DbContextOptions<kmsContext> _dbOptions;
        private readonly kmsContext _context;

        public GradeTests()
        {
            var dbName = $"TestDatabase_{Guid.NewGuid()}";
            _dbOptions = new DbContextOptionsBuilder<kmsContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            _context = new kmsContext(_dbOptions);
        }
        [Theory]
        [InlineData(1, true)] // Grade with ID 1 exists
        [InlineData(2, false)] // Grade with ID 2 does not exist
        public async Task GetGradeByIdAsync_ShouldReturnCorrectGradeOrNull(int id, bool doesExist)
        {
            // Arrange: Clear existing data and seed initial data if required
            _context.Grades.RemoveRange(_context.Grades);
            await _context.SaveChangesAsync();

            if (doesExist)
            {
                _context.Grades.Add(new BusinessObject.Models.Grade
                {
                    GradeId = id,
                    Name = "Test Grade",
                    Description = "This is a test grade.",
                    BaseTuitionFee = 150000,
                });
                await _context.SaveChangesAsync();
            }

            var repository = new GradeRepository(null ,_context);

            // Act
            var result = await repository.GetGradeByIdAsync(id);

            // Assert
            if (doesExist)
            {
                Assert.NotNull(result);
                Assert.Equal(id, result.GradeId);
                Assert.Equal("Test Grade", result.Name);
            }
            else
            {
                Assert.Null(result);
            }
        }
        [Fact]
        public async Task AddGradeAsync_ShouldAddGradeToDatabase()
        {
            // Arrange: Prepare test data
            var gradeDto = new GradeModelDTO
            {
                Name = "Grade 1",
                BaseTuitionFee = 1000.00M,
                Description = "This is a test grade."
            };

            var repository = new GradeRepository(null, _context);

            // Act: Call the method to add the grade
            var result = await repository.AddGradeAsync(gradeDto);

            // Assert: Verify the grade was added
            var addedGrade = await _context.Grades.FindAsync(result.GradeId);
            Assert.NotNull(addedGrade); // Ensure that the grade is saved
            Assert.Equal(gradeDto.Name, addedGrade.Name);
            Assert.Equal(gradeDto.BaseTuitionFee, addedGrade.BaseTuitionFee);
            Assert.Equal(gradeDto.Description, addedGrade.Description);
        }

        public static IEnumerable<object[]> GetUpdateGradeTestData()
        {
            return new List<object[]>
            {
                // Successful update case
                new object[] { 1, "Updated Grade Name", 1200.00M, "Updated Description", true, true, true },

                // Grade does not exist
                new object[] { 99, "Non-Existing Grade", 1500.00M, "Does not exist", false, false, false },

                // Invalid Base Tuition Fee (negative value)
                 
            };
        }

        [Theory]
        [MemberData(nameof(GetUpdateGradeTestData))]
        public async Task UpdateGradeAsync_ShouldHandleVariousScenarios(
            int gradeId,
            string name,
            decimal? baseTuitionFee,
            string description,
            bool doesGradeExist,
            bool isValidData,
            bool isExpectedToSucceed)
        {
            // Arrange: Create in-memory context and seed data
            using var context = new kmsContext(_dbOptions);
            var gradeRepository = new GradeRepository(null,context);

            // Seed data if necessary
            if (doesGradeExist)
            {
                context.Grades.Add(new BusinessObject.Models.Grade
                {
                    GradeId = gradeId,
                    Name = "Old Grade Name",
                    BaseTuitionFee = 1000.00M,
                    Description = "Old Description"
                });
            }

            await context.SaveChangesAsync();

            var gradeDto = new GradeModelDTO
            {
                GradeId = gradeId,
                Name = name,
                BaseTuitionFee = baseTuitionFee,
                Description = description
            };

            // Act: Try to update grade
            if (isExpectedToSucceed)
            {
                var result = await gradeRepository.UpdateGradeAsync(gradeDto);

                // Assert: Verify that the grade was updated successfully
                Assert.NotNull(result);
                Assert.Equal(name, result.Name);
                Assert.Equal(baseTuitionFee, result.BaseTuitionFee);
                Assert.Equal(description, result.Description);
            }
            else
            {
                // Assert: Verify that an exception was thrown or the result was null
                if (isValidData)
                {
                    var exception = await Assert.ThrowsAsync<ValidationException>(() => gradeRepository.UpdateGradeAsync(gradeDto));
                    Assert.Equal("Base Tuition Fee must be greater than zero.", exception.Message);
                }
                else
                {
                    var result = await gradeRepository.UpdateGradeAsync(gradeDto);
                    Assert.Null(result); // If not expected to succeed, result should be null
                }
            }
        }
        public static IEnumerable<object[]> GetSoftDeleteTestData()
        {
            return new List<object[]>
            {
                // Test Case 1: Grade exists and is deleted
                new object[] { 1, true, true },

                // Test Case 2: Grade does not exist, should return false
                new object[] { 99, false, false },

            };
        }

        [Theory]
        [MemberData(nameof(GetSoftDeleteTestData))]
        public async Task SoftDeleteGradeAsync_ShouldHandleVariousScenarios(int gradeId, bool doesGradeExist, bool isExpectedToSucceed)
        {
            // Arrange: Create an in-memory context and add test data if necessary
            using var context = new kmsContext(_dbOptions);

            if (doesGradeExist)
            {
                // Seed data if grade exists
                context.Grades.Add(new BusinessObject.Models.Grade { GradeId = gradeId, Name = "Grade Test", BaseTuitionFee = 1000.00M, Description = "Test Description" });
            }

            await context.SaveChangesAsync();

            var gradeRepository = new GradeRepository(null,context);

            // Act: Perform the soft delete
            var result = await gradeRepository.SoftDeleteGradeAsync(gradeId);

            // Assert: Verify the result
            Assert.Equal(isExpectedToSucceed, result); // The result should match expectation

            // Additional check: If deletion is expected, verify that the grade was deleted
            if (isExpectedToSucceed)
            {
                var deletedGrade = await context.Grades.FindAsync(gradeId);
                Assert.Null(deletedGrade); // Grade should be deleted if deletion was successful
            }
            else
            {
                var notDeletedGrade = await context.Grades.FindAsync(gradeId);
                Assert.Null(notDeletedGrade); // Grade should still not exist if deletion failed (non-existent)
            }
        }

    }
}
