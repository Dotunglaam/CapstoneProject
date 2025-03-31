using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCapstone.Test.Children;
public class AddChildrenTest
{
    private readonly DbContextOptions<kmsContext> _dbOptions;
    private readonly IMapper _mapper;

    public AddChildrenTest()
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
    [InlineData(999, 1, "ParentId 999 is invalid.")] // ParentId không hợp lệ
    [InlineData(1, 999, "GradeId 999 is invalid.")]  // GradeId không hợp lệ
    [InlineData(999, 999, "ParentId 999 is invalid.")]
    public async Task AddChildren_ShouldThrowException_WhenParentOrGradeIsInvalid(int parentId, int gradeId, string expectedMessage)
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
                ParentId = parentId,
                GradeId = gradeId,
                FullName = "John Doe",
                Dob = DateTime.Now.AddYears(-5)
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => dao.AddChildren(childrenMapper));

            Assert.Equal(expectedMessage, exception.Message);
        }
    }
    [Theory]
    [InlineData(null, "2000-01-01", "FullName is required.")]
    [InlineData("John Doe", null, "Date of Birth is required.")]
    public async Task AddChildren_ShouldThrowException_WhenMandatoryFieldsAreMissing(string fullName, string dob, string expectedMessage)
    {
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Parents.Add(new Parent { ParentId = 1, Name = "Valid Parent" });
            context.Grades.Add(new BusinessObject.Models.Grade { GradeId = 1, Name = "Valid Grade" });
            await context.SaveChangesAsync();

            var dao = new ChildrenDAO(context, _mapper);

            var childrenMapper = new ChildrenMapper
            {
                ParentId = 1,
                GradeId = 1,
                FullName = fullName,
                Dob = dob == null ? (DateTime?)null : DateTime.Parse(dob)
            };

            var exception = await Assert.ThrowsAsync<Exception>(() => dao.AddChildren(childrenMapper));
            Assert.Equal(expectedMessage, exception.Message);
        }
    }

    [Fact]
    public async Task AddChildren_ShouldAddSuccessfully_WhenParentAndGradeAreValid()
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

            var dao = new ChildrenDAO(context, _mapper);

            var childrenMapper = new ChildrenMapper
            {
                ParentId = 1,
                GradeId = 1,
                FullName = "John Doe",
                Dob = DateTime.Now.AddYears(-5)
            };

            // Act
            var result = await dao.AddChildren(childrenMapper);

            // Assert
            var child = await context.Children.FindAsync(result);

            Assert.NotNull(child);
            Assert.Equal(childrenMapper.FullName, child.FullName);
            Assert.Equal(childrenMapper.ParentId, child.ParentId);
            Assert.Equal(childrenMapper.GradeId, child.GradeId);
            Assert.StartsWith("ST", child.Code); // Kiểm tra mã được tạo bắt đầu với "ST"
        }
    }
}

