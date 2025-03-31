using AutoMapper;
using BusinessObject.Models;
using BusinessObject.DTOS;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;

namespace ProjectCapstone.Test.Pickup;

public class GetPickupPersonInfoByUUIDTest
{
    private readonly DbContextOptions<kmsContext> _dbOptions;
    private readonly IMapper _mapper;

    public GetPickupPersonInfoByUUIDTest()
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
    public async Task GetPickupPersonInfoByUUIDAsync_ShouldReturnNull_WhenPickupPersonNotFound()
    {
        // Arrange
        var uuid = "a47d5d34-b7a1-46a9-bc39-57f9bc2729db"; // UUID hợp lệ không có trong cơ sở dữ liệu
        using var context = new kmsContext(_dbOptions);

        var dao = new PickupPersonDAO(context, _mapper);  // Khởi tạo DAO

        // Act
        var result = await dao.GetPickupPersonInfoByUUIDAsync(uuid);

        // Assert
        Assert.Null(result);  // Kết quả phải là null vì không tìm thấy PickupPerson
    }

    [Fact]
    public async Task GetPickupPersonInfoByUUIDAsync_ShouldReturnEmptyStudents_WhenNoStudentsAssociated()
    {
        // Arrange
        var uuid = "b123f324-b7a1-46a9-bc39-57f9bc2729db"; // UUID hợp lệ nhưng không có học sinh liên kết
        using var context = new kmsContext(_dbOptions);

        // Tạo PickupPerson không có học sinh liên kết
        var pickupPerson = new PickupPerson
        {
            PickupPersonId = 2,
            Name = "Alice Smith",
            PhoneNumber = "987654321",
            Uuid = uuid,
            ImageUrl = "http://example.com/image2.jpg"
        };

        context.PickupPeople.Add(pickupPerson);
        await context.SaveChangesAsync(); // Lưu PickupPerson vào cơ sở dữ liệu

        var dao = new PickupPersonDAO(context, _mapper);  // Khởi tạo DAO

        // Act
        var result = await dao.GetPickupPersonInfoByUUIDAsync(uuid);

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
    public async Task GetPickupPersonInfoByUUIDAsync_ShouldThrowArgumentException_WhenUuidIsInvalid()
    {
        // Arrange
        var invalidUuid = "invalid-uuid";  // UUID không hợp lệ
        using var context = new kmsContext(_dbOptions);

        var dao = new PickupPersonDAO(context, _mapper);  // Khởi tạo DAO

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => dao.GetPickupPersonInfoByUUIDAsync(invalidUuid));
    }
}
