using AutoMapper;
using BusinessObject.Models;
using BusinessObject.DTOS;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;

namespace ProjectCapstone.Test.Pickup;

public class GetPickupPersonInfoByParentIdTest
{
    private readonly DbContextOptions<kmsContext> _dbOptions;
    private readonly IMapper _mapper;

    public GetPickupPersonInfoByParentIdTest()
    {
        // Cấu hình DbContext cho cơ sở dữ liệu InMemory
        _dbOptions = new DbContextOptionsBuilder<kmsContext>()
                    .UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}")
                    .Options;

        // Cấu hình AutoMapper
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<PickupPerson, PickupPersonInfoDto>();
            cfg.CreateMap<Child, StudentInfoDto>();
        });
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public async Task GetPickupPersonInfoByParentIdAsync_ShouldReturnNull_WhenPickupPersonNotFound()
    {
        // Arrange
        var parentId = 999; // ParentId không tồn tại trong cơ sở dữ liệu
        using var context = new kmsContext(_dbOptions);

        var dao = new PickupPersonDAO(context, _mapper);  // Khởi tạo DAO

        // Act
        var result = await dao.GetPickupPersonInfoByParentIdAsync(parentId);

        // Assert
        Assert.Null(result);  // Kết quả phải là null vì không tìm thấy PickupPerson với ParentId
    }

    [Fact]
    public async Task GetPickupPersonInfoByParentIdAsync_ShouldReturnEmptyStudents_WhenNoStudentsAssociated()
    {
        // Arrange
        var parentId = 1;  // ParentId hợp lệ nhưng không có học sinh liên kết
        using var context = new kmsContext(_dbOptions);

        // Tạo PickupPerson không có học sinh liên kết
        var pickupPerson = new PickupPerson
        {
            PickupPersonId = 1,
            Name = "John Doe",
            PhoneNumber = "123456789",
            ParentId = parentId,  // Đảm bảo rằng ParentId trùng khớp
            Uuid = "a47d5d34-b7a1-46a9-bc39-57f9bc2729db",
            ImageUrl = "http://example.com/image.jpg"
        };

        context.PickupPeople.Add(pickupPerson);
        await context.SaveChangesAsync(); // Lưu PickupPerson vào cơ sở dữ liệu

        var dao = new PickupPersonDAO(context, _mapper);  // Khởi tạo DAO

        // Act
        var result = await dao.GetPickupPersonInfoByParentIdAsync(parentId);

        // Assert
        Assert.NotNull(result);  // Kết quả không phải null
        Assert.Equal(pickupPerson.PickupPersonId, result.PickupPersonID);
        Assert.Equal(pickupPerson.Name, result.Name);
        Assert.Equal(pickupPerson.PhoneNumber, result.PhoneNumber);
        Assert.Equal(pickupPerson.Uuid, result.UUID);
        Assert.Equal(pickupPerson.ImageUrl, result.ImageUrl);
        Assert.Empty(result.Students);  // Kiểm tra nếu không có học sinh, danh sách Students phải rỗng
    }

    [Fact]
    public async Task GetPickupPersonInfoByParentIdAsync_ShouldReturnStudents_WhenStudentsAreAssociated()
    {
        // Arrange
        var parentId = 1;  // ParentId hợp lệ và có học sinh liên kết
        using var context = new kmsContext(_dbOptions);

        // Tạo PickupPerson
        var pickupPerson = new PickupPerson
        {
            PickupPersonId = 1,
            Name = "John Doe",
            PhoneNumber = "123456789",
            ParentId = parentId,
            Uuid = "a47d5d34-b7a1-46a9-bc39-57f9bc2729db",
            ImageUrl = "http://example.com/image.jpg"
        };

        // Tạo học sinh
        var child = new Child
        {
            StudentId = 1,
            FullName = "Jane Doe",
            Code = "S123",
            Avatar = "http://example.com/avatar.jpg",
            ParentId = parentId  // Kết nối với ParentId
        };

        // Thêm PickupPerson vào Child.PickupPeople để thiết lập mối quan hệ
        child.PickupPeople.Add(pickupPerson);

        // Lưu dữ liệu vào cơ sở dữ liệu
        context.PickupPeople.Add(pickupPerson);
        context.Children.Add(child);
        await context.SaveChangesAsync();

        var dao = new PickupPersonDAO(context, _mapper);  // Khởi tạo DAO

        // Act
        var result = await dao.GetPickupPersonInfoByParentIdAsync(parentId);

        // Assert
        Assert.NotNull(result);  // Kết quả không phải null
        Assert.Equal(pickupPerson.PickupPersonId, result.PickupPersonID);
        Assert.Equal(pickupPerson.Name, result.Name);
        Assert.Equal(pickupPerson.PhoneNumber, result.PhoneNumber);
        Assert.Equal(pickupPerson.Uuid, result.UUID);
        Assert.Equal(pickupPerson.ImageUrl, result.ImageUrl);
        Assert.Single(result.Students);  // Kiểm tra chỉ có một học sinh liên quan
        Assert.Equal(child.FullName, result.Students[0].FullName);
        Assert.Equal(child.Code, result.Students[0].Code);
        Assert.Equal(child.Avatar, result.Students[0].Avatar);
    }

}
