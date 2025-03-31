using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ProjectCapstone.Test.Menu
{
    public class UpdateMenuTest
    {
        private readonly DbContextOptions<kmsContext> _dbOptions;

        public UpdateMenuTest()
        {
            _dbOptions = new DbContextOptionsBuilder<kmsContext>()
                .UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}")
                .Options;
        }

        [Theory]
        [InlineData(1, "2023-12-01", "2023-12-07", "2023-12-05", "2023-12-10", 1, "A1", "Food 1", "Monday")]
        [InlineData(2, "2023-12-05", "2023-12-10", "2023-12-06", "2023-12-12", 2, "B2", "Food 2", "Tuesday")]
        [InlineData(3, "2023-12-10", "2023-12-15", "2023-12-11", "2023-12-16", 3, "C3", "Food 3", "Wednesday")]
        [InlineData(4, "2023-12-01", "2023-12-07", "2023-12-02", "2023-12-08", 4, "D4", "New Food", "Thursday")]
        [InlineData(5, "2023-12-01", "2023-12-07", "2023-12-03", "2023-12-09", 5, "E5", "Updated Food", "Friday")]
        public async Task UpdateMenu_ShouldUpdateMenu_WhenValidData(int menuId, string startDate, string endDate,
    string newStartDate, string newEndDate,
    int menuDetailId, string mealCode,
    string foodName, string dayOfWeek)
        {
            // Arrange
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Tạo menu
                var existingMenu = new BusinessObject.Models.Menu
                {
                    MenuId = menuId,
                    StartDate = DateOnly.Parse(startDate),
                    EndDate = DateOnly.Parse(endDate),
                    Status = 1
                };
                context.Menus.Add(existingMenu);
                await context.SaveChangesAsync(); // Lưu menu

                // Nếu menu có MenuDetails, tạo sẵn dữ liệu chi tiết
                if (menuDetailId > 0)
                {
                    var existingDetail = new Menudetail
                    {
                        MenuDetailId = menuDetailId,
                        MealCode = mealCode,
                        FoodName = foodName,
                        DayOfWeek = dayOfWeek,
                        MenuId = menuId
                    };
                    context.Menudetails.Add(existingDetail);
                    await context.SaveChangesAsync(); // Lưu MenuDetails nếu có
                }

                // Tạo menuMapper
                var menuMapper = new MenuMapper
                {
                    MenuID = menuId,
                    StartDate = DateOnly.Parse(newStartDate),
                    EndDate = DateOnly.Parse(newEndDate),
                    MenuDetails = new List<MenuDetailMapper>
                    {
                        new MenuDetailMapper
                        {
                            MenuDetailId = menuDetailId,
                            MealCode = mealCode,
                            FoodName = foodName,
                            DayOfWeek = dayOfWeek
                        }
                    }
                };

                var mockMapper = new Mock<IMapper>();
                mockMapper.Setup(m => m.Map<Menudetail>(It.IsAny<MenuDetailMapper>()))
                          .Returns((MenuDetailMapper detailMapper) => new Menudetail
                          {
                              MenuDetailId = detailMapper.MenuDetailId,
                              MealCode = detailMapper.MealCode,
                              FoodName = detailMapper.FoodName,
                              DayOfWeek = detailMapper.DayOfWeek,
                          });

                var dao = new MenuDAO(context, mockMapper.Object);

                // Act
                await dao.UpdateMenu(menuMapper);

                // Assert
                var updatedMenu = await context.Menus.FindAsync(menuId);
                Assert.NotNull(updatedMenu);  // Kiểm tra menu có tồn tại sau khi cập nhật
                Assert.Equal(DateOnly.Parse(newStartDate), updatedMenu.StartDate);
                Assert.Equal(DateOnly.Parse(newEndDate), updatedMenu.EndDate);

                // Assert nếu menuDetails có thay đổi hoặc thêm mới
                var updatedDetail = await context.Menudetails
                    .FirstOrDefaultAsync(md => md.MenuDetailId == menuDetailId);
                Assert.NotNull(updatedDetail);
                Assert.Equal(mealCode, updatedDetail.MealCode);
                Assert.Equal(foodName, updatedDetail.FoodName);
                Assert.Equal(dayOfWeek, updatedDetail.DayOfWeek);
            }
        }
        [Fact]
        public async Task UpdateMenu_ShouldThrowException_WhenMenuNotFound()
        {
            // Arrange
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var menuMapper = new MenuMapper
                {
                    MenuID = 9999, // ID không tồn tại
                    StartDate = DateOnly.Parse("2023-12-01"),
                    EndDate = DateOnly.Parse("2023-12-07"),
                    MenuDetails = new List<MenuDetailMapper>
            {
                new MenuDetailMapper
                {
                    MenuDetailId = 1,
                    MealCode = "A1",
                    FoodName = "Food 1",
                    DayOfWeek = "Monday"
                }
            }
                };

                var mockMapper = new Mock<IMapper>();
                var dao = new MenuDAO(context, mockMapper.Object);

                // Act & Assert
                var exception = await Assert.ThrowsAsync<Exception>(() => dao.UpdateMenu(menuMapper));
                Assert.Equal("Menu not found", exception.Message);
            }
        }
        [Fact]
        public async Task UpdateMenu_ShouldNotUpdateDetails_WhenMenuDetailsAreEmpty()
        {
            // Arrange
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var existingMenu = new BusinessObject.Models.Menu
                {
                    MenuId = 1,
                    StartDate = DateOnly.Parse("2023-12-01"),
                    EndDate = DateOnly.Parse("2023-12-07"),
                    Status = 1
                };
                context.Menus.Add(existingMenu);
                await context.SaveChangesAsync();

                var menuMapper = new MenuMapper
                {
                    MenuID = 1,
                    StartDate = DateOnly.Parse("2023-12-10"),
                    EndDate = DateOnly.Parse("2023-12-15"),
                    MenuDetails = new List<MenuDetailMapper>() // Không có chi tiết menu
                };

                var mockMapper = new Mock<IMapper>();
                var dao = new MenuDAO(context, mockMapper.Object);

                // Act
                await dao.UpdateMenu(menuMapper);

                // Assert
                var updatedMenu = await context.Menus.FindAsync(1);
                Assert.NotNull(updatedMenu);
                Assert.Equal(DateOnly.Parse("2023-12-10"), updatedMenu.StartDate);
                Assert.Equal(DateOnly.Parse("2023-12-15"), updatedMenu.EndDate);

                // Kiểm tra không có MenuDetails được thêm mới
                var menuDetails = await context.Menudetails.Where(md => md.MenuId == 1).ToListAsync();
                Assert.Empty(menuDetails);
            }
        }
        [Fact]
        public async Task UpdateMenu_ShouldAddNewMenuDetail_WhenMenuDetailDoesNotExist()
        {
            // Arrange
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var existingMenu = new BusinessObject.Models.Menu
                {
                    MenuId = 1,
                    StartDate = DateOnly.Parse("2023-12-01"),
                    EndDate = DateOnly.Parse("2023-12-07"),
                    Status = 1
                };
                context.Menus.Add(existingMenu);
                await context.SaveChangesAsync();

                var menuMapper = new MenuMapper
                {
                    MenuID = 1,
                    StartDate = DateOnly.Parse("2023-12-01"),
                    EndDate = DateOnly.Parse("2023-12-07"),
                    MenuDetails = new List<MenuDetailMapper>
            {
                new MenuDetailMapper
                {
                    MealCode = "F6",
                    FoodName = "New Food",
                    DayOfWeek = "Saturday"
                }
            }
                };

                var mockMapper = new Mock<IMapper>();
                mockMapper.Setup(m => m.Map<Menudetail>(It.IsAny<MenuDetailMapper>()))
                          .Returns((MenuDetailMapper detailMapper) => new Menudetail
                          {
                              MenuDetailId = 0, // Đánh dấu là mới
                              MealCode = detailMapper.MealCode,
                              FoodName = detailMapper.FoodName,
                              DayOfWeek = detailMapper.DayOfWeek,
                          });

                var dao = new MenuDAO(context, mockMapper.Object);

                // Act
                await dao.UpdateMenu(menuMapper);

                // Assert
                var newDetail = await context.Menudetails
                    .FirstOrDefaultAsync(md => md.MealCode == "F6" && md.MenuId == 1);
                Assert.NotNull(newDetail);
                Assert.Equal("New Food", newDetail.FoodName);
                Assert.Equal("Saturday", newDetail.DayOfWeek);
            }
        }
        [Fact]
        public async Task UpdateMenu_ShouldUpdateExistingMenuDetail_WhenMenuDetailExists()
        {
            // Arrange
            using (var context = new kmsContext(_dbOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var existingMenu = new BusinessObject.Models.Menu
                {
                    MenuId = 1,
                    StartDate = DateOnly.Parse("2023-12-01"),
                    EndDate = DateOnly.Parse("2023-12-07"),
                    Status = 1
                };
                context.Menus.Add(existingMenu);
                await context.SaveChangesAsync();

                var existingDetail = new Menudetail
                {
                    MenuDetailId = 1,
                    MealCode = "A1",
                    FoodName = "Old Food",
                    DayOfWeek = "Monday",
                    MenuId = 1
                };
                context.Menudetails.Add(existingDetail);
                await context.SaveChangesAsync();

                var menuMapper = new MenuMapper
                {
                    MenuID = 1,
                    StartDate = DateOnly.Parse("2023-12-01"),
                    EndDate = DateOnly.Parse("2023-12-07"),
                    MenuDetails = new List<MenuDetailMapper>
            {
                new MenuDetailMapper
                {
                    MenuDetailId = 1,  // Cập nhật detail cũ
                    MealCode = "A1",
                    FoodName = "Updated Food",
                    DayOfWeek = "Monday"
                }
            }
                };

                var mockMapper = new Mock<IMapper>();
                mockMapper.Setup(m => m.Map<Menudetail>(It.IsAny<MenuDetailMapper>()))
                          .Returns((MenuDetailMapper detailMapper) => new Menudetail
                          {
                              MenuDetailId = detailMapper.MenuDetailId,
                              MealCode = detailMapper.MealCode,
                              FoodName = detailMapper.FoodName,
                              DayOfWeek = detailMapper.DayOfWeek,
                          });

                var dao = new MenuDAO(context, mockMapper.Object);

                // Act
                await dao.UpdateMenu(menuMapper);

                // Assert
                var updatedDetail = await context.Menudetails
                    .FirstOrDefaultAsync(md => md.MenuDetailId == 1);
                Assert.NotNull(updatedDetail);
                Assert.Equal("Updated Food", updatedDetail.FoodName);
                Assert.Equal("Monday", updatedDetail.DayOfWeek);
            }
        }

    }
}
