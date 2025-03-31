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

namespace ProjectCapstone.Test.Menu
{
    public class UpdateMenuStatusTest
    {
        private readonly DbContextOptions<kmsContext> _dbOptions;

        public UpdateMenuStatusTest()
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


    }
}