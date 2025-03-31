using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Moq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using AutoMapper;

namespace ProjectCapstone.Test.Menu;

public class GetMenuByDateTest
{
    private readonly DbContextOptions<kmsContext> _dbOptions;

    public GetMenuByDateTest()
    {
        _dbOptions = new DbContextOptionsBuilder<kmsContext>()
            .UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}")
            .Options;
    }

    [Fact]
    public async Task GetMenuByDate_ShouldReturnMenus_WhenValidDateRange()
    {
        // Arrange
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Tạo dữ liệu giả
            var menu1 = new BusinessObject.Models.Menu
            {
                MenuId = 1,
                StartDate = new DateOnly(2023, 12, 1),  // Sử dụng DateOnly
                EndDate = new DateOnly(2023, 12, 7),    // Sử dụng DateOnly
                Status = 1
            };
            var menu2 = new BusinessObject.Models.Menu
            {
                MenuId = 2,
                StartDate = new DateOnly(2023, 12, 8),  // Sử dụng DateOnly
                EndDate = new DateOnly(2023, 12, 14),   // Sử dụng DateOnly
                Status = 1
            };

            context.Menus.Add(menu1);
            context.Menus.Add(menu2);

            var grade1 = new MenuHasGrade
            {
                MenuId = 1,
                GradeId = 1
            };
            var grade2 = new MenuHasGrade
            {
                MenuId = 2,
                GradeId = 2
            };

            context.MenuHasGrades.Add(grade1);
            context.MenuHasGrades.Add(grade2);

            var menuDetail1 = new Menudetail
            {
                MenuDetailId = 1,
                MealCode = "A1",
                FoodName = "Food 1",
                DayOfWeek = "Monday",
                MenuId = 1
            };
            var menuDetail2 = new Menudetail
            {
                MenuDetailId = 2,
                MealCode = "B1",
                FoodName = "Food 2",
                DayOfWeek = "Tuesday",
                MenuId = 2
            };

            context.Menudetails.Add(menuDetail1);
            context.Menudetails.Add(menuDetail2);

            await context.SaveChangesAsync();

            // Mock IMapper
            var mockMapper = new Mock<IMapper>();

            // Giả lập hành vi của IMapper
            mockMapper.Setup(m => m.Map<List<MenuHasGradeMapper>>(It.IsAny<IEnumerable<BusinessObject.Models.Menu>>()))
                .Returns((IEnumerable<BusinessObject.Models.Menu> menus) => menus.Select(menu => new MenuHasGradeMapper
                {
                    MenuID = menu.MenuId,
                    StartDate = menu.StartDate.HasValue ? menu.StartDate.Value : default(DateOnly),  // Nếu StartDate có giá trị thì lấy, nếu không thì dùng default(DateOnly)
                    EndDate = menu.EndDate.HasValue ? menu.EndDate.Value : default(DateOnly),        // Nếu EndDate có giá trị thì lấy, nếu không thì dùng default(DateOnly)
                    Status = menu.Status ?? 0,  // Sử dụng giá trị mặc định nếu Status null
                    GradeIDs = menu.MenuHasGrades.Select(g => g.GradeId).ToList(),
                    MenuDetails = menu.Menudetails.Select(md => new MenuDetailMapper
                    {
                        MenuDetailId = md.MenuDetailId,
                        MealCode = md.MealCode,
                        FoodName = md.FoodName,
                        DayOfWeek = md.DayOfWeek
                    }).ToList()
                }).ToList());



            // Khởi tạo DAO với Mock IMapper
            var dao = new MenuDAO(context, mockMapper.Object);

            // Act
            var result = await dao.GetMenuByDate(
                new DateTime(2023, 12, 1),   // Dùng DateTime trực tiếp
                new DateTime(2023, 12, 7)    // Dùng DateTime trực tiếp
            );

            // Assert
            Assert.NotNull(result);
            Assert.Single(result); // Chỉ có 1 menu trong phạm vi ngày
            Assert.Equal(1, result[0].MenuID);
            Assert.Equal(new DateOnly(2023, 12, 1), result[0].StartDate);
            Assert.Equal(new DateOnly(2023, 12, 7), result[0].EndDate);
            Assert.Contains(result[0].GradeIDs, grade => grade == 1);
            Assert.Equal(1, result[0].MenuDetails.Count); // Chỉ có 1 MenuDetail
        }
    }


    [Fact]
    public async Task GetMenuByDate_ShouldReturnEmptyList_WhenNoMenusInDateRange()
    {
        // Arrange
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Tạo dữ liệu giả
            var menu1 = new BusinessObject.Models.Menu
            {
                MenuId = 1,
                StartDate = new DateOnly(2023, 12, 1),
                EndDate = new DateOnly(2023, 12, 7),
                Status = 1
            };

            context.Menus.Add(menu1);
            await context.SaveChangesAsync();

            // Mock IMapper
            var mockMapper = new Mock<IMapper>();

            // Giả lập hành vi của IMapper
            mockMapper.Setup(m => m.Map<List<MenuHasGradeMapper>>(It.IsAny<IEnumerable<BusinessObject.Models.Menu>>()))
                .Returns((IEnumerable<BusinessObject.Models.Menu> menus) => menus.Select(menu => new MenuHasGradeMapper
                {
                    MenuID = menu.MenuId,
                    StartDate = menu.StartDate.HasValue ? menu.StartDate.Value : default(DateOnly),  // Nếu StartDate có giá trị thì lấy, nếu không thì dùng default(DateOnly)
                    EndDate = menu.EndDate.HasValue ? menu.EndDate.Value : default(DateOnly),        // Nếu EndDate có giá trị thì lấy, nếu không thì dùng default(DateOnly)
                    Status = menu.Status ?? 0,  // Sử dụng giá trị mặc định nếu Status null
                    GradeIDs = menu.MenuHasGrades.Select(g => g.GradeId).ToList(),
                    MenuDetails = menu.Menudetails.Select(md => new MenuDetailMapper
                    {
                        MenuDetailId = md.MenuDetailId,
                        MealCode = md.MealCode,
                        FoodName = md.FoodName,
                        DayOfWeek = md.DayOfWeek
                    }).ToList()
                }).ToList());

            var dao = new MenuDAO(context, mockMapper.Object);

            // Act
            var result = await dao.GetMenuByDate(new DateTime(2023, 12, 8), new DateTime(2023, 12, 14));

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Không có menu trong khoảng thời gian này
        }

    }
    [Fact]
    public async Task GetMenuByDate_ShouldReturnEmptyList_WhenStartDateIsGreaterThanEndDate()
    {
        // Arrange
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var menu1 = new BusinessObject.Models.Menu
            {
                MenuId = 1,
                StartDate = new DateOnly(2023, 12, 1),
                EndDate = new DateOnly(2023, 12, 7),
                Status = 1
            };

            context.Menus.Add(menu1);
            await context.SaveChangesAsync();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<List<MenuHasGradeMapper>>(It.IsAny<IEnumerable<BusinessObject.Models.Menu>>()))
                .Returns((IEnumerable<BusinessObject.Models.Menu> menus) => menus.Select(menu => new MenuHasGradeMapper
                {
                    MenuID = menu.MenuId,
                    StartDate = menu.StartDate ?? default(DateOnly),
                    EndDate = menu.EndDate ?? default(DateOnly),
                    Status = menu.Status ?? 0,
                    GradeIDs = menu.MenuHasGrades.Select(g => g.GradeId).ToList(),
                    MenuDetails = menu.Menudetails.Select(md => new MenuDetailMapper
                    {
                        MenuDetailId = md.MenuDetailId,
                        MealCode = md.MealCode,
                        FoodName = md.FoodName,
                        DayOfWeek = md.DayOfWeek
                    }).ToList()
                }).ToList());

            var dao = new MenuDAO(context, mockMapper.Object);

            // Act
            var result = await dao.GetMenuByDate(new DateTime(2023, 12, 7), new DateTime(2023, 12, 1));

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Kiểm tra rằng không có menu nào được trả về
        }
    }

    [Fact]
    public async Task GetMenuByDate_ShouldReturnMenus_WhenMultipleMenusExistInDateRange()
    {
        // Arrange
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var menu1 = new BusinessObject.Models.Menu
            {
                MenuId = 1,
                StartDate = new DateOnly(2023, 12, 1),
                EndDate = new DateOnly(2023, 12, 7),
                Status = 1
            };
            var menu2 = new BusinessObject.Models.Menu
            {
                MenuId = 2,
                StartDate = new DateOnly(2023, 12, 5),
                EndDate = new DateOnly(2023, 12, 10),
                Status = 1
            };

            context.Menus.Add(menu1);
            context.Menus.Add(menu2);
            await context.SaveChangesAsync();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<List<MenuHasGradeMapper>>(It.IsAny<IEnumerable<BusinessObject.Models.Menu>>()))
                .Returns((IEnumerable<BusinessObject.Models.Menu> menus) => menus.Select(menu => new MenuHasGradeMapper
                {
                    MenuID = menu.MenuId,
                    StartDate = menu.StartDate ?? default(DateOnly),
                    EndDate = menu.EndDate ?? default(DateOnly),
                    Status = menu.Status ?? 0,
                    GradeIDs = menu.MenuHasGrades.Select(g => g.GradeId).ToList(),
                    MenuDetails = menu.Menudetails.Select(md => new MenuDetailMapper
                    {
                        MenuDetailId = md.MenuDetailId,
                        MealCode = md.MealCode,
                        FoodName = md.FoodName,
                        DayOfWeek = md.DayOfWeek
                    }).ToList()
                }).ToList());

            var dao = new MenuDAO(context, mockMapper.Object);

            // Act
            var result = await dao.GetMenuByDate(new DateTime(2023, 12, 1), new DateTime(2023, 12, 7));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count); // Kiểm tra trả về 1 menu trong phạm vi ngày
        }
    }

    [Fact]
    public async Task GetMenuByDate_ShouldReturnMenusWithNoMenuDetails_WhenMenuDetailsEmpty()
    {
        // Arrange
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var menu1 = new BusinessObject.Models.Menu
            {
                MenuId = 1,
                StartDate = new DateOnly(2023, 12, 1),
                EndDate = new DateOnly(2023, 12, 7),
                Status = 1
            };
            context.Menus.Add(menu1);
            await context.SaveChangesAsync();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<List<MenuHasGradeMapper>>(It.IsAny<IEnumerable<BusinessObject.Models.Menu>>()))
                .Returns((IEnumerable<BusinessObject.Models.Menu> menus) => menus.Select(menu => new MenuHasGradeMapper
                {
                    MenuID = menu.MenuId,
                    StartDate = menu.StartDate ?? default(DateOnly),
                    EndDate = menu.EndDate ?? default(DateOnly),
                    Status = menu.Status ?? 0,
                    GradeIDs = menu.MenuHasGrades.Select(g => g.GradeId).ToList(),
                    MenuDetails = new List<MenuDetailMapper>() // Không có MenuDetails
                }).ToList());

            var dao = new MenuDAO(context, mockMapper.Object);

            // Act
            var result = await dao.GetMenuByDate(new DateTime(2023, 12, 1), new DateTime(2023, 12, 7));

            // Assert
            Assert.NotNull(result);
            Assert.Single(result); // Kiểm tra chỉ có 1 menu
            Assert.Empty(result[0].MenuDetails); // Kiểm tra menu này không có MenuDetails
        }
    }

    [Fact]
    public async Task GetMenuByDate_ShouldReturnMenus_WhenStatusIsDifferent()
    {
        // Arrange
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var menu1 = new BusinessObject.Models.Menu
            {
                MenuId = 1,
                StartDate = new DateOnly(2023, 12, 1),
                EndDate = new DateOnly(2023, 12, 7),
                Status = 1
            };
            var menu2 = new BusinessObject.Models.Menu
            {
                MenuId = 2,
                StartDate = new DateOnly(2023, 12, 8),
                EndDate = new DateOnly(2023, 12, 14),
                Status = 0  // Trạng thái khác
            };

            context.Menus.Add(menu1);
            context.Menus.Add(menu2);
            await context.SaveChangesAsync();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<List<MenuHasGradeMapper>>(It.IsAny<IEnumerable<BusinessObject.Models.Menu>>()))
                .Returns((IEnumerable<BusinessObject.Models.Menu> menus) => menus.Select(menu => new MenuHasGradeMapper
                {
                    MenuID = menu.MenuId,
                    StartDate = menu.StartDate ?? default(DateOnly),
                    EndDate = menu.EndDate ?? default(DateOnly),
                    Status = menu.Status ?? 0,
                    GradeIDs = menu.MenuHasGrades.Select(g => g.GradeId).ToList(),
                    MenuDetails = menu.Menudetails.Select(md => new MenuDetailMapper
                    {
                        MenuDetailId = md.MenuDetailId,
                        MealCode = md.MealCode,
                        FoodName = md.FoodName,
                        DayOfWeek = md.DayOfWeek
                    }).ToList()
                }).ToList());

            var dao = new MenuDAO(context, mockMapper.Object);

            // Act
            var result = await dao.GetMenuByDate(new DateTime(2023, 12, 1), new DateTime(2023, 12, 7));

            // Assert
            Assert.NotNull(result);
            Assert.Single(result); // Chỉ trả về menu có Status = 1
            Assert.Equal(1, result[0].MenuID);
        }
    }
}

