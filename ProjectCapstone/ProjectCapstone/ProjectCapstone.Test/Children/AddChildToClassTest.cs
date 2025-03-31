using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ProjectCapstone.Test.Children
{
    public class AddChildToClassTest
    {
        private readonly DbContextOptions<kmsContext> _dbOptions;
        private readonly IMapper _mapper;

        public AddChildToClassTest()
        {
            // Tạo DbContextOptions cho cơ sở dữ liệu in-memory
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

        [Fact]
        public async Task AddChildToClass_ShouldThrowException_WhenClassNotFound()
        {
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var dao = new ChildrenDAO(context, _mapper);

                // Act & Assert: Kiểm tra nếu lớp học không tồn tại
                var exception = await Assert.ThrowsAsync<Exception>(
                    () => dao.AddChildToClass(999, 1));  // ClassId không tồn tại

                Assert.StartsWith("Error adding students to class:", exception.Message); // Kiểm tra phần đầu thông báo
                Assert.Contains("Class not found", exception.Message);
            }
        }

        [Fact]
        public async Task AddChildToClass_ShouldThrowException_WhenChildrenNotFound()
        {
            // Arrange
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var dao = new ChildrenDAO(context, _mapper);

                // Act & Assert: Kiểm tra nếu học sinh không tồn tại
                var exception = await Assert.ThrowsAsync<Exception>(
                    () => dao.AddChildToClass(1, 999));  // StudentId không tồn tại

                Assert.StartsWith("Error adding students to class:", exception.Message); // Kiểm tra phần đầu thông báo
                Assert.Contains("Class not found", exception.Message); // Kiểm tra phần sau thông báo
            }
        }

        [Fact]
        public async Task AddChildToClass_ShouldThrowException_WhenStudentAlreadyInClass()
        {
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Tạo lớp và học sinh
                var classItem = new Class { ClassId = 1, ClassName = "Test Class" };
                var student = new Child { StudentId = 1, FullName = "Test Student", Code = "Student001" };

                context.Classes.Add(classItem);
                context.Children.Add(student);
                await context.SaveChangesAsync();

                var classHasChild = new ClassHasChild { ClassId = 1, StudentId = 1, Date = DateTime.Now };
                context.ClassHasChildren.Add(classHasChild);
                await context.SaveChangesAsync();

                var dao = new ChildrenDAO(context, _mapper);

                // Act & Assert: Kiểm tra nếu học sinh đã có trong lớp
                var exception = await Assert.ThrowsAsync<Exception>(
                    () => dao.AddChildToClass(1, 1));  // Học sinh đã có trong lớp này

                Assert.StartsWith("Error adding students to class:", exception.Message); // Kiểm tra phần đầu thông báo
                Assert.Contains("The student has been assigned to this class", exception.Message);
            }
        }

        [Fact]
        public async Task AddChildToClass_ShouldThrowException_WhenStudentHasAnotherClass()
        {
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Tạo lớp và học sinh
                var class1 = new Class { ClassId = 1, ClassName = "Class 1" };
                var class2 = new Class { ClassId = 2, ClassName = "Class 2" };
                var student = new Child { StudentId = 1, FullName = "Test Student", Code = "Student001" };

                context.Classes.Add(class1);
                context.Classes.Add(class2);
                context.Children.Add(student);
                await context.SaveChangesAsync();

                var classHasChild = new ClassHasChild { ClassId = 1, StudentId = 1, Date = DateTime.Now };
                context.ClassHasChildren.Add(classHasChild);
                await context.SaveChangesAsync();

                var dao = new ChildrenDAO(context, _mapper);

                // Act & Assert: Kiểm tra nếu học sinh đã có lớp khác
                var exception = await Assert.ThrowsAsync<Exception>(
                    () => dao.AddChildToClass(2, 1));  // Học sinh đã có lớp khác

                Assert.StartsWith("Error adding students to class:", exception.Message); // Kiểm tra phần đầu thông báo
                Assert.Contains("Student already has another class. Cannot be added to this class.", exception.Message);
            }
        }

        [Fact]
        public async Task AddChildToClass_ShouldAddChildToClass_WhenValidData()
        {
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Tạo lớp và học sinh
                var classItem = new Class { ClassId = 1, ClassName = "Test Class" };
                var student = new Child { StudentId = 1, FullName = "Test Student", Code = "Student001" };

                context.Classes.Add(classItem);
                context.Children.Add(student);
                await context.SaveChangesAsync();

                var dao = new ChildrenDAO(context, _mapper);

                // Act: Thêm học sinh vào lớp
                await dao.AddChildToClass(1, 1);

                // Assert: Kiểm tra học sinh có được thêm vào lớp không
                var classHasChild = await context.ClassHasChildren
                    .FirstOrDefaultAsync(ch => ch.ClassId == 1 && ch.StudentId == 1);

                Assert.NotNull(classHasChild);
                Assert.Equal(1, classHasChild.ClassId);
                Assert.Equal(1, classHasChild.StudentId);
            }
        }
    }
}
