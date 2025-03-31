using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObject.DTOS.Request;

namespace BusinessObject.DTOS
{
    public class Request
    {
        public class LoginResponse
        {
            public User User { get; set; }
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public DateTime ExpiryDate { get; set; }
            public string Message { get; set; } // Add this property for the message

        }
        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
        public class ResetRequest
        {
            public string Email { get; set; }
            public string NewPassword { get; set; }
        }
        public class RegisterViewModel
        {
            public string Mail { get; set; } = null!;
            public int RoleId { get; set; }
            public string Password { get; set; } = null!;

        }
        public class TokenAddModel
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public DateTime ExpiryDate { get; set; }
            public int UserId { get; set; }
        }
        public class ResetPasswordViewModel
        {

            [Required]
            public string Token { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }
        public class ForgotPasswordViewModel
        {
            [Required(ErrorMessage = "Email is required.")]
            public string Mail { get; set; }
        }
        public class ViewProfileModel
        {
            public int UserId { get; set; }
            public string? Firstname { get; set; }
            public string? LastName { get; set; }
            public string? Address { get; set; }
            public string? PhoneNumber { get; set; }
            public sbyte? Gender { get; set; }
            public DateTime? Dob { get; set; }
            public IFormFile? Avatar { get; set; }
        }
        public class ViewProfileTeacherForTableTecher {
            public int TeacherId { get; set; }
            public string? Name { get; set; }
            public string? Education { get; set; }
            public string? Experience { get; set; }
            public int HomeroomTeacher { get; set; }
        }
        public class TeacherForByAll
        {
            public int TeacherId { get; set; }
            public string? Name { get; set; }
            public string? Education { get; set; }
            public string? Experience { get; set; }

            public string? Firstname { get; set; }
            public string? LastName { get; set; }
            public string? Address { get; set; }
            public string? PhoneNumber { get; set; }
            public string Mail { get; set; } = null!;
            public sbyte? Status { get; set; }
            public sbyte? Gender { get; set; }
            public DateTime? Dob { get; set; }
            public string? Code { get; set; }
            public string? Avatar { get; set; }
            public int Role { get; set; }
            public int? HomeroomTeacher { get; set; }
            public List<string>? Classes { get; set; } = new List<string>();

        }

        public class ScheduleResponse
        {
            [Key]
            public int ScheduleId { get; set; }
            public ClassResponse Class { get; set; }
            public TimeResponse Time { get; set; }
            public ActivityResponse Activity { get; set; }
            public LocationResponse Location { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string? Note { get; set; }
            public int? Status { get; set; }
        }
        public class ScheduleRequest
        {
            public string ClassName { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string ActivityName { get; set; }     
            public string LocationName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string? Note { get; set; }
        }
        public class TeacherResponse
        {
            public int TeacherId { get; set; }
            public string Name { get; set; }
        }
        public class ClassResponse
        {
            public int ClassId { get; set; }
            public string ClassName { get; set; }
            public List<TeacherResponse> Teachers { get; set; } // List of teacher details
        }
        public class TimeResponse
        {
            public int TimeSlotId { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
        }
        public class LocationResponse
        {
            public int LocationId { get; set; }
            public string LocationName { get; set; }
            
        }
        public class ActivityResponse
        {
            public int ActivityId { get; set; }
            public string ActivityName { get; set; }
        }
    }
    public class ScheduleResponse1
    {
        public int ScheduleId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ActivityName { get; set; }
        public string LocationName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Status { get; set; }
        public string? Note { get; set; }
    }
    public class ChangePasswordViewModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
    public class CloudinarySettings
    {
        public string CloudName { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }
    public class AlbumCreateDto
    {
        public int ClassId { get; set; }
        public int CreateBy { get; set; }
        public string? AlbumName { get; set; }
        public string? Description { get; set; }
    }
    public class AlbumDtoh
    {
        public int AlbumId { get; set; }
        public int ClassId { get; set; }
        public int CreateBy { get; set; }
        public int ModifiBy { get; set; }
        public string? AlbumName { get; set; }
        public DateTime? TimePost { get; set; }
        public string? Description { get; set; }
        public int? Status { get; set; }
        public int? isActive { get; set; }
        public string? Reason { get; set; }
    }
    public class AlbumUpdateDto
    {
        public int AlbumId { get; set; }
        public int ClassId { get; set; }
        public string? AlbumName { get; set; }
        public string? Description { get; set; }

    }
    public class UpdateStatusOfAlbum
    {
        public int AlbumId { get; set; }
        public int? Status { get; set; }
        public string? Reason { get; set; }
    }
    public class ServiceDetailDto
    {
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public DateTime DateUsed { get; set; }
        public int Quantity { get; set; }
        public string ServiceDescription { get; set; }
    }
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public int ChildId { get; set; }
        public int? TuitionId { get; set; }
        public List<int>? ServiceId { get; set; }

        public string PaymentName { get; set; }
        public decimal TuitionAmount { get; set; }
        public int Month { get; set; }
        public int Discount { get; set; }
    }
    public class GradeModelDTO
    {

        public int GradeId { get; set; }
        [Required]
        public string? Name { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Base Tuition Fee must be greater than zero.")]
        public decimal? BaseTuitionFee { get; set; }

        public string Description { get; set; }
    }
    
    public class ChildTuitionDto
    {
        public int StudentId { get; set; }
        public int GradeId { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public string NickName { get; set; }
        public DateTime Dob { get; set; }
        public int Gender { get; set; }
        public int Status { get; set; }
        public string EthnicGroups { get; set; }
        public string Nationality { get; set; }
        public string Religion { get; set; }
        public string Avatar { get; set; }
        public List<TuitionRecordDto> Tuition { get; set; } = new List<TuitionRecordDto>();
        public List<MonthlyServiceUsageDto> ServicesUsed { get; set; } = new List<MonthlyServiceUsageDto>();
    }

    public class TuitionRecordDto
    {
        public int TuitionId { get; set; }
        public int SemesterId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TuitionFee { get; set; }
        public decimal TotalFee { get; set; }
        public DateOnly DueDate { get; set; }
        public int DiscountId { get; set; }
        public int IsPaid { get; set; }
    }

    public class MonthlyServiceUsageDto
    {
        public string Name { get; set; }
        public decimal Total { get; set; }
        public List<ServiceUsageDto> Services { get; set; } = new List<ServiceUsageDto>();
    }

    public class ServiceUsageDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal ServicePrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
    public class DiscountDTO
    {
        public int DiscountId { get; set; }
        public int? Number { get; set; }
        public int? Discount1 { get; set; }
    }
    public class PaymentHistoryDto
    {
        public int PaymentId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
        public string PaymentName { get; set; }
        public int StudentId { get; set; }
        public List<ServiceDto> Services { get; set; } = new List<ServiceDto>();
    }

    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal ServicePrice { get; set; }
    }
    public class TuitionDTO
    {
        public int TuitionId { get; set; }
        public int StudentId { get; set; }
        public string ParentName { get; set; }
        public string Mail { get; set; }
        public string PhoneNumber { get; set; }
        public string StudentName { get; set; } // Thêm trường StudentName
        public string Code { get; set; } = null!;
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } // Tên học kỳ
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? TuitionFee { get; set; }
        public DateOnly? DueDate { get; set; }
        public int? IsPaid { get; set; }
        public int? StatusTuitionLate { get; set; }
        public DateTime? LastEmailSentDate { get; set; }
        public int SendMailByPR { get; set; }

    }
    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public class TuitionRecord
    {
        public int TuitionId { get; set; }
        public int StudentId { get; set; }
        public int SemesterId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? TuitionFee { get; set; }
        public DateOnly? DueDate { get; set; }
        public int? IsPaid { get; set; }
        public decimal? TotalFee { get; set; }
        public int? DiscountId { get; set; }
        public int? StatusTuitionLate { get; set; }
        public DateTime? LastEmailSentDate { get; set; }
        public int? SendMailByPr { get; set; }
    }
}
