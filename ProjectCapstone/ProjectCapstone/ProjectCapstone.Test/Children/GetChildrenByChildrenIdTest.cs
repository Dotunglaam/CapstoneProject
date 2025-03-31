using AutoMapper;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;

namespace ProjectCapstone.Test.Children;

public class GetChildrenByChildrenIdTest
{
    private readonly DbContextOptions<kmsContext> _dbOptions;
    private readonly IMapper _mapper;

    public GetChildrenByChildrenIdTest()
    {
        _dbOptions = new DbContextOptionsBuilder<kmsContext>()
                        .UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}")
                        .Options;

        var configuration = new MapperConfiguration(cfg => { /* Mapping cấu hình */ });
        _mapper = configuration.CreateMapper();
    }

    [Theory]
    [InlineData(999, "Children not found")] // Trường hợp không tìm thấy trẻ
    [InlineData(-1, "Children not found")]  // ID không hợp lệ
    public async Task GetChildrenByChildrenId_ShouldThrowException_WhenChildrenDoesNotExist(int childrenID, string expectedMessage)
    {
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var dao = new ChildrenDAO(context, _mapper);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => dao.GetChildrenByChildrenId(childrenID));

            Assert.Contains(expectedMessage, exception.Message);
        }
    }

    [Theory]
    [InlineData(1)] // Trẻ tồn tại với ID 1
    public async Task GetChildrenByChildrenId_ShouldReturnChildren_WhenChildrenExists(int childrenID)
    {
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Arrange: Thêm dữ liệu mẫu
            var child = new Child
            {
                StudentId = 1,
                Code = "C001",
                FullName = "John Doe",
                ClassHasChildren = new List<ClassHasChild>
                {
                    new ClassHasChild
                    {
                        Class = new Class
                        {
                            ClassId = 101,
                            ClassName = "Class A",
                            IsActive = 1
                        }
                    }
                }
            };
            context.Children.Add(child);
            await context.SaveChangesAsync();

            var dao = new ChildrenDAO(context, _mapper);

            // Act
            var result = await dao.GetChildrenByChildrenId(childrenID);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(childrenID, result.StudentId);
            Assert.Single(result.Classes); // Kiểm tra có 1 lớp học được ánh xạ
        }
    }
}
