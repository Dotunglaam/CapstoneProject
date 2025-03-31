using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BusinessObject.DTOS.DashBoardPrincipleMapper;

namespace DataAccess.DAO
{
    public class DashBoardPrincipleDAO
    {
        private readonly kmsContext _context;
        private readonly IMapper _mapper;

        public DashBoardPrincipleDAO(kmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }



        public async Task<List<EnrollmentStatisticsDTO>> GetEnrollmentStatisticsAsync()
        {
            // Lấy toàn bộ dữ liệu từ ClassHasChildren
            var classHasChildren = await _context.ClassHasChildren.ToListAsync();

            // Ánh xạ danh sách ClassHasChildren sang danh sách EnrollmentStatisticsDTO
            var enrollmentStatisticsDTOs = _mapper.Map<List<EnrollmentStatisticsDTO>>(classHasChildren);

            // Hiển thị thông tin từng năm và từng tháng
            foreach (var enrollmentStatistics in enrollmentStatisticsDTOs)
            {
                Console.WriteLine($"Year: {enrollmentStatistics.Year}");
                foreach (var monthly in enrollmentStatistics.MonthlyEnrollments)
                {
                    Console.WriteLine($"Month: {monthly.Month}, New Students: {monthly.NewStudents}");
                }
            }

            return enrollmentStatisticsDTOs;
        }


        public async Task<TeacherWithTotalDTO> GetTeacherWithTotalAsync()
        {
            // Lấy tất cả giáo viên cùng với danh sách các lớp và thông tin khối lớp của mỗi lớp
            var teachers = await _context.Teachers
                .Include(t => t.Classes)
                    .ThenInclude(c => c.Grade)
                .ToListAsync();

            // Tính tổng số giáo viên hiện tại
            int totalTeacherNow = teachers.Count;

            // Ánh xạ danh sách giáo viên sang TeacherDTO và tạo đối tượng TeacherWithTotalDTO
            var teacherWithTotalDTO = new TeacherWithTotalDTO
            {
                TotalTeacherNow = totalTeacherNow,
                Teachers = _mapper.Map<List<TeacherDTO>>(teachers)
            };

            return teacherWithTotalDTO;
        }


        public async Task<FinancialSummaryDTO> GetFinancialSummaryForYearAsync(int year)
        {
            // Lấy tất cả các payment trong năm được chỉ định
            var payments = await _context.Payments
                .Where(p => p.PaymentDate.HasValue && p.PaymentDate.Value.Year == year)
                .Include(p => p.Tuition)  // Lấy thông tin Tuition liên quan
                .Include(p => p.Service)  // Lấy thông tin Service liên quan
                .ToListAsync();

            // Nhóm các payment theo tháng và tính toán doanh thu
            var financialSummaryByMonth = payments
                .GroupBy(p => p.PaymentDate.Value.Month)
                .Select(g => new FinancialSummaryByMonthDTO
                {
                    // Lấy tên tháng
                    Month = new DateTime(year, g.Key, 1).ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture),

                    // Tính doanh thu học phí: Lọc các payment có liên kết với Tuition
                    TuitionRevenue = g.Where(p => p.TuitionId.HasValue)
                                      .Sum(p => p.Tuition.TotalFee ?? 0),  // Tổng học phí của tháng

                    // Tính doanh thu dịch vụ: Lọc các payment có liên kết với Service
                    ServiceRevenue = g.Where(p => p.ServiceId.HasValue)
                                      .Sum(p => p.Service.ServicePrice ?? 0),  // Tổng dịch vụ của tháng

                    // Tổng doanh thu: Cộng doanh thu học phí và dịch vụ
                    TotalRevenue = g.Sum(p => p.TotalAmount ?? 0)
                })
                .ToList();

            // Tính tổng doanh thu trong năm
            var totalRevenue = financialSummaryByMonth.Sum(f => f.TotalRevenue);

            // Chuẩn bị DTO trả về
            var financialSummaryDTO = new FinancialSummaryDTO
            {
                Year = year,
                TotalRevenue = totalRevenue,
                FinancialSummaryByMonth = financialSummaryByMonth
            };

            return financialSummaryDTO;
        }
        public async Task<List<AccountmentStatisticsDTO>> GetAccountmentStatisticsAsync()
        {
            // Lấy toàn bộ dữ liệu từ ClassHasChildren
            var classHasChildren = await _context.Users.ToListAsync();

            // Ánh xạ sang EnrollmentStatisticsDTO
            var enrollmentStatisticsDTO = _mapper.Map<List<AccountmentStatisticsDTO>>(classHasChildren);

            foreach (var enrollmentStatistics in enrollmentStatisticsDTO)
            {
                Console.WriteLine($"Year: {enrollmentStatistics.Year}");
                foreach (var monthly in enrollmentStatistics.MonthlyAccountments)
                {
                    Console.WriteLine($"Month: {monthly.Month}, New Students: {monthly.NewAccount}");
                }
            }
            return enrollmentStatisticsDTO;
        }

    }
}
