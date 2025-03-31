using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class DashBoardPrincipleMapper
    {
        
    }
    public class FinancialSummaryDTO
    {
        public int Year { get; set; } // Năm tài chính
        public decimal TotalRevenue { get; set; } // Tổng doanh thu trong năm
        public List<FinancialSummaryByMonthDTO> FinancialSummaryByMonth { get; set; } // Doanh thu theo tháng
    }

    public class FinancialSummaryByMonthDTO
    {
        public string Month { get; set; } // Tên tháng (January, February, ...)

        // Doanh thu học phí
        public decimal TuitionRevenue { get; set; }

        // Doanh thu dịch vụ
        public decimal ServiceRevenue { get; set; }

        // Tổng doanh thu (học phí + dịch vụ)
        public decimal TotalRevenue { get; set; }
    }


    public class MonthlyEnrollmentDTO
    {
        public int Month { get; set; }
        public int NewStudents { get; set; }
    }

    public class EnrollmentStatisticsDTO
    {
        public int Year { get; set; }
        public List<MonthlyEnrollmentDTO> MonthlyEnrollments { get; set; }
    }


    // DTO đại diện cho dữ liệu của giáo viên
    public class TeacherWithTotalDTO
    {
        public int TotalTeacherNow { get; set; }   // Tổng số giáo viên
        public List<TeacherDTO> Teachers { get; set; }  // Danh sách giáo viên
    }
    public class TeacherDTO
    {
        public int TeacherId { get; set; }
        public string Name { get; set; }
        public List<ClassDTO> Classes { get; set; } // Danh sách lớp mà giáo viên phụ trách
       
    }

    public class ClassDTO
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string GradeName { get; set; }        
    }

    public class AccountmentDTO
    {
        public int Month { get; set; }
        public int NewAccount { get; set; }
    }

    public class AccountmentStatisticsDTO
    {
        public int Year { get; set; }
        public List<AccountmentDTO> MonthlyAccountments { get; set; }
    }

}
