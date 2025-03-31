using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class Payment
    {
        public int PaymentId { get; set; }
        public DateOnly? PaymentDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? Status { get; set; }
        public int? TuitionId { get; set; }
        public int? ServiceId { get; set; }
        public int StudentId { get; set; }
        public string? PaymentName { get; set; }

        public virtual Service? Service { get; set; }
        public virtual Child Student { get; set; } = null!;
        public virtual Tuition? Tuition { get; set; }
    }
}
