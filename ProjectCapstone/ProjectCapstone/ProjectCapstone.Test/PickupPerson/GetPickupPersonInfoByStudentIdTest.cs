using AutoMapper;
using BusinessObject.Models;
using BusinessObject.DTOS;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ProjectCapstone.Test.Pickup
{
    public class GetPickupPersonInfoByStudentIdTest
    {
        private readonly DbContextOptions<kmsContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetPickupPersonInfoByStudentIdTest()
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
        public async Task GetPickupPersonInfoByStudentIdAsync_ShouldReturnEmptyList_WhenNoPickupPersonFound()
        {
            // Arrange
            var studentId = 1;  // StudentId không tồn tại trong cơ sở dữ liệu
            using var context = new kmsContext(_dbOptions);

            var dao = new PickupPersonDAO(context, _mapper);  // Khởi tạo DAO

            // Act
            var result = await dao.GetPickupPersonInfoByStudentIdAsync(studentId);

            // Assert
            Assert.Empty(result);  // Kết quả phải là danh sách rỗng vì không tìm thấy PickupPerson liên quan
        }

        [Fact]
        public async Task GetPickupPersonInfoByStudentIdAsync_ShouldReturnPickupPerson_WhenPickupPersonFound()
        {
            // Arrange
            var studentId = 1;  // StudentId hợp lệ và có PickupPerson liên kết
            using var context = new kmsContext(_dbOptions);

            // Tạo PickupPerson và Child
            var pickupPerson = new PickupPerson
            {
                PickupPersonId = 1,
                Name = "John Doe",
                PhoneNumber = "123456789",
                Uuid = "a47d5d34-b7a1-46a9-bc39-57f9bc2729db",
                ImageUrl = "http://example.com/image.jpg"
            };

            var child = new Child
            {
                StudentId = studentId,
                FullName = "Jane Doe",
                Code = "S123",
                Avatar = "http://example.com/avatar.jpg",
                ParentId = 1  // Kết nối với ParentId hợp lệ
            };

            // Kết nối PickupPerson với Child
            pickupPerson.Students.Add(child);

            context.PickupPeople.Add(pickupPerson);
            context.Children.Add(child);
            await context.SaveChangesAsync(); // Lưu dữ liệu vào cơ sở dữ liệu

            var dao = new PickupPersonDAO(context, _mapper);  // Khởi tạo DAO

            // Act
            var result = await dao.GetPickupPersonInfoByStudentIdAsync(studentId);

            // Assert
            Assert.NotEmpty(result);  // Kết quả không phải rỗng
            var pickupPersonResult = result.First();  // Giả sử có ít nhất một PickupPerson

            // Kiểm tra thông tin của PickupPerson
            Assert.Equal(pickupPerson.PickupPersonId, pickupPersonResult.PickupPersonID);
            Assert.Equal(pickupPerson.Name, pickupPersonResult.Name);
            Assert.Equal(pickupPerson.PhoneNumber, pickupPersonResult.PhoneNumber);
            Assert.Equal(pickupPerson.Uuid, pickupPersonResult.UUID);
            Assert.Equal(pickupPerson.ImageUrl, pickupPersonResult.ImageUrl);

            // Kiểm tra danh sách học sinh liên kết
            var studentResult = pickupPersonResult.Students.First();
            Assert.Equal(child.FullName, studentResult.FullName);
            Assert.Equal(child.Code, studentResult.Code);
            Assert.Equal(child.Avatar, studentResult.Avatar);
        }

        [Fact]
        public async Task GetPickupPersonInfoByStudentIdAsync_ShouldReturnMultiplePickupPersons_WhenMultiplePickupPersonsFound()
        {
            // Arrange
            var studentId = 1;  // StudentId hợp lệ và có nhiều PickupPerson liên kết
            using var context = new kmsContext(_dbOptions);

            // Tạo PickupPerson 1
            var pickupPerson1 = new PickupPerson
            {
                PickupPersonId = 1,
                Name = "John Doe",
                PhoneNumber = "123456789",
                Uuid = "a47d5d34-b7a1-46a9-bc39-57f9bc2729db",
                ImageUrl = "http://example.com/image1.jpg"
            };

            // Tạo PickupPerson 2
            var pickupPerson2 = new PickupPerson
            {
                PickupPersonId = 2,
                Name = "Jane Smith",
                PhoneNumber = "987654321",
                Uuid = "b47d5d34-b7a1-46a9-bc39-57f9bc2729db",
                ImageUrl = "http://example.com/image2.jpg"
            };

            // Tạo Child
            var child = new Child
            {
                StudentId = studentId,
                FullName = "Michael Doe",
                Code = "S1234",
                Avatar = "http://example.com/avatar2.jpg",
                ParentId = 1
            };

            // Kết nối PickupPerson với Child
            pickupPerson1.Students.Add(child);
            pickupPerson2.Students.Add(child);

            context.PickupPeople.AddRange(pickupPerson1, pickupPerson2);
            context.Children.Add(child);
            await context.SaveChangesAsync(); // Lưu dữ liệu vào cơ sở dữ liệu

            var dao = new PickupPersonDAO(context, _mapper);  // Khởi tạo DAO

            // Act
            var result = await dao.GetPickupPersonInfoByStudentIdAsync(studentId);

            // Assert
            Assert.NotEmpty(result);  // Kết quả không phải rỗng
            Assert.Equal(2, result.Count);  // Kiểm tra có 2 PickupPerson

            // Kiểm tra các PickupPerson
            Assert.Contains(result, r => r.PickupPersonID == pickupPerson1.PickupPersonId);
            Assert.Contains(result, r => r.PickupPersonID == pickupPerson2.PickupPersonId);
        }
    }
}
