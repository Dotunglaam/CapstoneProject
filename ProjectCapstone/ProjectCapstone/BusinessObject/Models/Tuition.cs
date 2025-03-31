using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Tuition
    {
        public Tuition()
        {
            Payments = new HashSet<Payment>();
        }

        public int TuitionId { get; set; }
        public int StudentId { get; set; }
        public int SemesterId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? TuitionFee { get; set; }
        public DateOnly? DueDate { get; set; }
        public int? IsPaid { get; set; }
        public decimal? TotalFee { get; set; }
        public int DiscountId { get; set; }
        public int? StatusTuitionLate { get; set; }
        public DateTime? LastEmailSentDate { get; set; }
        public int? SendMailByPr { get; set; }

        public virtual Discount Discount { get; set; } = null!;
        public virtual Semester Semester { get; set; } = null!;
        public virtual Child Student { get; set; } = null!;
        public virtual ICollection<Payment> Payments { get; set; }
    }
}
