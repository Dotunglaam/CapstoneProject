using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class MenuDAO
    {
        private readonly kmsContext _context;
        private readonly IMapper _mapper;
        private readonly ClassDAO _classDAO;
        public MenuDAO(kmsContext dbContext, IMapper mapper)
        {
            _context = dbContext;
            _mapper = mapper;
        }

        public MenuDAO(kmsContext dbContext, IMapper mapper, ClassDAO classDAO)
        {
            _context = dbContext;
            _mapper = mapper;
            _classDAO = classDAO;
        }

        public async Task<List<MenuHasGradeMapper>> GetMenuByDate(DateTime startDate, DateTime endDate)
        {
            var start = DateOnly.FromDateTime(startDate);
            var end = DateOnly.FromDateTime(endDate);

            var menus = await _context.Menus
                .Where(m => m.StartDate >= start && m.EndDate <= end)
                .Include(m => m.MenuHasGrades)        
                .Include(m => m.Menudetails)          
                .ToListAsync();                      

            var menuDtos = menus.Select(m => new MenuHasGradeMapper
            {
                MenuID = m.MenuId,
                StartDate = m.StartDate.Value,
                EndDate = m.EndDate.Value,
                Status = m.Status.Value,
                GradeIDs = m.MenuHasGrades.Select(mhg => mhg.GradeId).ToList(), 
                MenuDetails = m.Menudetails.Select(md => new MenuDetailMapper
                {
                    MenuDetailId = md.MenuDetailId,
                    MealCode = md.MealCode,
                    FoodName = md.FoodName,
                    DayOfWeek = md.DayOfWeek
                }).ToList() 
            }).ToList();

            return menuDtos;
        }

        public async Task<MenuHasGradeMapper> GetMenuByMenuId(int menuId)
        {
            var menu = await _context.Menus
                .Where(m => m.MenuId == menuId) // Tìm theo MenuId
                .Include(m => m.MenuHasGrades)
                .Include(m => m.Menudetails)
                .FirstOrDefaultAsync(); // Lấy một đối tượng duy nhất hoặc null nếu không tìm thấy

            if (menu == null)
            {
                return null; // Trả về null nếu không tìm thấy
            }

            var menuDto = new MenuHasGradeMapper
            {
                MenuID = menu.MenuId,
                StartDate = menu.StartDate.Value,
                EndDate = menu.EndDate.Value,
                Status = menu.Status.Value,
                GradeIDs = menu.MenuHasGrades.Select(mhg => mhg.GradeId).ToList(),
                MenuDetails = menu.Menudetails.Select(md => new MenuDetailMapper
                {
                    MenuDetailId = md.MenuDetailId,
                    MealCode = md.MealCode,
                    FoodName = md.FoodName,
                    DayOfWeek = md.DayOfWeek
                }).ToList()
            };

            return menuDto;
        }


        public async Task ImportMenuExcel(IFormFile file)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];

                        // Lưu StartDate và EndDate từ ô B2 và C2
                        var startDate = DateTime.TryParse(worksheet.Cells[2, 2].Text, out DateTime startDateValue)
                            ? DateOnly.FromDateTime(startDateValue) : (DateOnly?)null;
                        var endDate = DateTime.TryParse(worksheet.Cells[2, 3].Text, out DateTime endDateValue)
                            ? DateOnly.FromDateTime(endDateValue) : (DateOnly?)null;

                        // Lưu GradeId từ ô E2
                        var gradeIds = worksheet.Cells[2, 5].Text.Split(',') // Giả sử GradeIds là một chuỗi phân cách bằng dấu phẩy
                            .Select(id => int.TryParse(id, out int parsedGradeId) ? parsedGradeId : (int?)null)
                            .Where(id => id.HasValue)
                            .ToList();

                        var overlappingGradeMenu = await _context.MenuHasGrades
                            .Include(mhg => mhg.Menu)
                            .Where(mhg => gradeIds.Contains(mhg.GradeId) &&
                                          (mhg.Menu.StartDate <= endDate && mhg.Menu.EndDate >= startDate))
                            .ToListAsync();

                        if (overlappingGradeMenu.Any())
                        {
                            throw new Exception($"A menu with overlapping grades exists for the selected date range. Conflicting GradeIds: {string.Join(", ", overlappingGradeMenu.Select(mhg => mhg.GradeId))}");
                        }

                        var menu = new Menu
                        {
                            StartDate = startDate,
                            EndDate = endDate,
                            SchoolId = 1, 
                            Status = 0
                        };

                        _context.Menus.Add(menu);
                        await _context.SaveChangesAsync(); 

                        foreach (var gradeId in gradeIds)
                        {
                            var menuHasGrade = new MenuHasGrade
                            {
                                MenuId = menu.MenuId,
                                GradeId = gradeId.Value
                            };

                            _context.MenuHasGrades.Add(menuHasGrade); 
                        }

                        await _context.SaveChangesAsync(); 

                        var daysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };

                        // Lưu chi tiết từ hàng 4 trở đi
                        var rowCount = worksheet.Dimension.Rows;
                        for (int row = 4; row <= rowCount; row++)
                        {
                            var mealCode = worksheet.Cells[row, 1].Text; // Cột A (MealCode)
                            for (int col = 2; col <= 7; col++) // Cột B-G (FoodName theo từng ngày)
                            {
                                var foodName = worksheet.Cells[row, col].Text;
                                if (!string.IsNullOrWhiteSpace(foodName))
                                {
                                    var menuDetail = new Menudetail
                                    {
                                        MenuId = menu.MenuId,
                                        MealCode = mealCode,
                                        FoodName = foodName,
                                        DayOfWeek = daysOfWeek[col - 2].ToString()
                                    };

                                    _context.Menudetails.Add(menuDetail);
                                }
                            }
                        }

                        await _context.SaveChangesAsync(); 
                    }
                }
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                throw new Exception($"Error importing menu data: {ex.Message}. Inner Exception: {innerExceptionMessage}");
            }
        }

        public async Task UpdateMenu(MenuMapper menuMapper)
        {
            try
            {
                var existingMenu = await _context.Menus.FindAsync(menuMapper.MenuID);
                if (existingMenu == null)
                {
                    throw new Exception("Menu not found");
                }

                existingMenu.StartDate = menuMapper.StartDate;
                existingMenu.EndDate = menuMapper.EndDate;

                var existingMenuDetails = await _context.Menudetails
                    .Where(md => md.MenuId == menuMapper.MenuID)
                    .ToDictionaryAsync(md => md.MenuDetailId);

                foreach (var detailMapper in menuMapper.MenuDetails)
                {
                    if (existingMenuDetails.TryGetValue(detailMapper.MenuDetailId, out var existingDetail))
                    {
                        existingDetail.MealCode = detailMapper.MealCode;
                        existingDetail.FoodName = detailMapper.FoodName;
                        existingDetail.DayOfWeek = detailMapper.DayOfWeek;
                    }
                    else
                    {
                        var newDetail = _mapper.Map<Menudetail>(detailMapper);
                        newDetail.MenuId = menuMapper.MenuID;
                        _context.Menudetails.Add(newDetail);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex.Message != "Menu not found")
                {
                    throw new Exception("An error occurred while saving changes.", ex);
                }
                else
                {
                    throw ex;
                }
            }
        }

        public async Task UpdateMenuStatus(MenuStatusMapper menuMapper)
        {
            try
            {
                var existingMenu = await _context.Menus.FindAsync(menuMapper.MenuID);
                if (existingMenu != null)
                {
                    existingMenu.Status = menuMapper.Status;

                    _context.Menus.Update(existingMenu);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Menu not found");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != "Menu not found")
                {
                    throw new Exception("An error occurred while saving changes.", ex);
                }
                else
                {
                    throw ex;
                }
            }
        }
        public async Task SendMenuToAllParentsMail()
        {
            try
            {
                var parentDetails = await _context.Children
                    .Include(c => c.Parent) 
                        .ThenInclude(p => p.ParentNavigation) 
                    .Where(c => c.Parent.ParentNavigation.Status == 1) 
                    .Select(c => new
                    {
                        ParentId = c.Parent.ParentId, 
                        ParentName = c.Parent.Name,
                        ParentMail = c.Parent.ParentNavigation.Mail,
                        StudentName = c.FullName,
                        GradeName = c.Grade.Name
                    })
                    .ToListAsync();

                var groupedParents = parentDetails
                    .GroupBy(p => new { p.ParentId, p.ParentName, p.ParentMail })
                    .ToList();

                foreach (var group in groupedParents)
                {
                    var parentInfo = group.Key;
                    var studentInfo = group.Select(g => $"- Tên trẻ: <b>{g.StudentName}</b>, Thuộc khối: <b>{g.GradeName}</b>").ToList();

                    string subject = "Thông báo Thực đơn tuần KMS";
                    string body = $@"
                            <html>
                            <body style=""font-family: Arial, sans-serif;"">
                                <div style=""border: 1px solid #ccc; padding: 20px; width: 100%; box-sizing: border-box;"">
                                    <p>Kính gửi Quý khách hàng <b>{parentInfo.ParentName}</b>,</p>
                                    <p>Xin trân trọng thông báo thực đơn tuần này của các con Quý khách:</p>
                                    {string.Join("<br>", studentInfo)}
                                    <p>Quý khách hãy kiểm tra hệ thống trên Web để xem chi tiết.</p>      
                                    <p>Trân trọng cảm ơn Quý khách !!!</p>
                                </div>  
                            </body>
                            </html>";

                    _classDAO.SendMailToParent(parentInfo.ParentMail, subject, body);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error sending mail: " + ex.Message);
            }
        }
    }
}
