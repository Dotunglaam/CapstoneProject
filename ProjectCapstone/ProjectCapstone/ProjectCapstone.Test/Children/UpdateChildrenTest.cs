using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ProjectCapstone.Test.Children
{
    public class UpdateChildrenTest
    {
        private readonly DbContextOptions<kmsContext> _dbOptions;
        private readonly IMapper _mapper;

        public UpdateChildrenTest()
        {
            _dbOptions = new DbContextOptionsBuilder<kmsContext>()
                        .UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}")
                        .Options;

            // Cấu hình AutoMapper
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ChildrenMapper, Child>();
            });
            _mapper = configuration.CreateMapper();
        }

        [Theory]
        [InlineData(999, 1, "Children not found.")] // ParentId không hợp lệ
        [InlineData(1, 999, "Children not found.")]  // GradeId không hợp lệ
        [InlineData(999, 999, "Children not found.")]
        [InlineData(-1, 999, "Children not found.")]
        [InlineData(999, -1, "GradeId must be a valid value.")]
        [InlineData(-1, -1, "GradeId must be a valid value.")]
        public async Task UpdateChildren_ShouldThrowException_WhenParentOrGradeIsInvalid(int parentId, int gradeId, string expectedMessage)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                if (parentId != 999) // Thêm Parent hợp lệ nếu ParentId không bị kiểm tra
                {
                    context.Parents.Add(new Parent { ParentId = parentId, Name = "Valid Parent" });
                }

                if (gradeId != 999) // Thêm Grade hợp lệ nếu GradeId không bị kiểm tra
                {
                    context.Grades.Add(new BusinessObject.Models.Grade { GradeId = gradeId, Name = "Valid Grade" });
                }

                await context.SaveChangesAsync();

                var dao = new ChildrenDAO(context, _mapper);

                var childrenMapper = new ChildrenMapper
                {
                    StudentId = 1, // Giả sử học sinh đã tồn tại
                    ParentId = parentId,
                    GradeId = gradeId,
                    FullName = "Updated Name",
                    Dob = DateTime.Now.AddYears(-5)
                };

                // Act & Assert
                var exception = await Assert.ThrowsAsync<Exception>(() => dao.UpdateChildren(childrenMapper));

                Assert.StartsWith("An error occurred while updating child:", exception.Message);

                // Kiểm tra nội dung chính xác
                Assert.Contains(expectedMessage, exception.Message);
            }
        }

        [Theory]
        [InlineData(null, "2000-01-01", "FullName is required.")]
        [InlineData("John Doe", null, "Date of birth is required.")]
        public async Task UpdateChildren_ShouldThrowException_WhenMandatoryFieldsAreMissing(string fullName, string dob, string expectedMessage)
        {
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Tạo Parent và Grade hợp lệ
                context.Parents.Add(new Parent { ParentId = 1, Name = "Valid Parent" });
                context.Grades.Add(new BusinessObject.Models.Grade { GradeId = 1, Name = "Valid Grade" });
                await context.SaveChangesAsync();

                var dao = new ChildrenDAO(context, _mapper);

                // Tạo một học sinh đã tồn tại
                var child = new Child
                {
                    StudentId = 1,
                    ParentId = 1,
                    GradeId = 1,
                    FullName = "Old Name",
                    Dob = DateTime.Now.AddYears(-5),
                    Code = "ST0001"
                };

                context.Children.Add(child);
                await context.SaveChangesAsync();

                var childrenMapper = new ChildrenMapper
                {
                    StudentId = 1,
                    ParentId = 1,
                    GradeId = 1,
                    FullName = fullName,
                    Dob = dob == null ? (DateTime?)null : DateTime.Parse(dob)
                };

                var exception = await Assert.ThrowsAsync<Exception>(() => dao.UpdateChildren(childrenMapper));

                // Kiểm tra phần đầu thông báo
                Assert.StartsWith("An error occurred while updating child:", exception.Message);

                // Kiểm tra nội dung chính xác
                Assert.Contains(expectedMessage, exception.Message);

            }
        }
        
        [Fact]
        public async Task UpdateChildren_ShouldUpdateSuccessfully_WhenParentAndGradeAreValid()
        {
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Arrange: Tạo dữ liệu Parent và Grade hợp lệ
                var parent = new Parent { ParentId = 1, Name = "Parent A" };
                var grade = new BusinessObject.Models.Grade { GradeId = 1, Name = "Grade 1" };

                context.Parents.Add(parent);
                context.Grades.Add(grade);
                await context.SaveChangesAsync();

                // Thêm một học sinh vào cơ sở dữ liệu
                var child = new Child
                {
                    StudentId = 1,
                    ParentId = 1,
                    GradeId = 1,
                    FullName = "John Doe",
                    Dob = DateTime.Now.AddYears(-5),
                    Code = "ST0001"
                };

                context.Children.Add(child);
                await context.SaveChangesAsync();

                var dao = new ChildrenDAO(context, _mapper);

                var childrenMapper = new ChildrenMapper
                {
                    StudentId = 1, // Sử dụng StudentId hợp lệ
                    ParentId = 1,
                    GradeId = 1,
                    FullName = "Updated Name",
                    Dob = DateTime.Now.AddYears(-5)
                };

                // Act
                await dao.UpdateChildren(childrenMapper);

                // Assert
                var updatedChild = await context.Children.FindAsync(1);

                Assert.NotNull(updatedChild);
                Assert.Equal(childrenMapper.FullName, updatedChild.FullName);
                Assert.Equal(childrenMapper.ParentId, updatedChild.ParentId);
                Assert.Equal(childrenMapper.GradeId, updatedChild.GradeId);
            }
        }
    }
}
