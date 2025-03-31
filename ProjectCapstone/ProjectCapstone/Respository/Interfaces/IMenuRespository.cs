using BusinessObject.DTOS;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Interfaces
{
    public interface IMenuRespository
    {
        Task<List<MenuHasGradeMapper>> GetMenuByDate(DateTime startDate, DateTime endDate);
        Task<MenuHasGradeMapper> GetMenuByMenuId(int MenuId);
        Task ImportMenuExcel(IFormFile file);
        Task UpdateMenu(MenuMapper menuMapper);
        Task UpdateMenuStatus(MenuStatusMapper menuMapper);
        Task SendMenuToAllParentsMail();
    }
}
