using BusinessObject.DTOS;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.AspNetCore.Http;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class MenuRespository : IMenuRespository
    {
        private readonly MenuDAO _menuDAO;

        public MenuRespository(MenuDAO menuDAO)
        {
            _menuDAO = menuDAO;
        }
        public async Task<List<MenuHasGradeMapper>> GetMenuByDate(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _menuDAO.GetMenuByDate(startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task<MenuHasGradeMapper> GetMenuByMenuId(int MenuId)
        {
            try
            {
                return await _menuDAO.GetMenuByMenuId(MenuId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        
        public async Task ImportMenuExcel(IFormFile file)
        {
            try
            {
                await _menuDAO.ImportMenuExcel(file);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task UpdateMenu(MenuMapper menuMapper)
        {
            try
            {
                await _menuDAO.UpdateMenu(menuMapper);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task UpdateMenuStatus(MenuStatusMapper menuMapper)
        {
            try
            {
                await _menuDAO.UpdateMenuStatus(menuMapper);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task SendMenuToAllParentsMail()
        {
            try
            {
                await _menuDAO.SendMenuToAllParentsMail();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
