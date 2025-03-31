using BusinessObject.DTOS;
using BusinessObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectCapstone.VNPay;
using Twilio.Http;

namespace ProjectCapstone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly kmsContext _context;
        public PaymentController(IConfiguration configuration, kmsContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // 1. API để tạo URL thanh toán qua VNPay sandbox
        [HttpPost("create-payment-url")]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] PaymentRequest request)
        {

            var vnp_ReturnUrl = "http://edunest.io.vn/payment-history"; // VNPay sẽ trả về URL này sau khi thanh toán
            var vnp_Url = _configuration["VNPaySettings:vnp_Url"];
            var vnp_TmnCode = _configuration["VNPaySettings:vnp_TmnCode"];
            var vnp_HashSecret = _configuration["VNPaySettings:vnp_HashSecret"];
            var vnpay = new VNPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (request.Amount * 100).ToString()); // VNPay yêu cầu số tiền nhân 100
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress.ToString());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"{request.PaymentName}");
            vnpay.AddRequestData("vnp_OrderType", "billpayment");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);

            var childId = request.ChildId;  
            var tuitionId = request.TuitionId ?? 0;  
            var serviceIds = request.ServiceId ?? new List<int> { 0 };  

            // Nếu ServiceIds không rỗng, ghép chúng thành một chuỗi, nếu không thì trả về "0"
            var serviceIdsPart = serviceIds.Any() ? string.Join("-", serviceIds) : "0";

            // Tạo chuỗi transactionRef với các giá trị đã xử lý
            var transactionRef = $"{childId}-{tuitionId}-{serviceIdsPart}-{DateTime.Now.Ticks}";
            vnpay.AddRequestData("vnp_TxnRef", transactionRef);

            // update tuition when paren click thanh toán 
            if (request.TuitionId != 0 || request.TuitionId != null) //trả về null
            {
                var tuitionRecord = await _context.Tuitions.FindAsync(request.TuitionId);
                if (tuitionRecord != null)
                {
                    tuitionRecord.EndDate = tuitionRecord.EndDate.Value.AddMonths(request.Month);
                    tuitionRecord.TotalFee = request.TuitionAmount;
                    tuitionRecord.DiscountId = request.Discount;
                    _context.Tuitions.Update(tuitionRecord);
                }
            }

            if ((request.ServiceId == null || !request.ServiceId.Any()) && (request.TuitionId != 0 || request.TuitionId != null)) // test request.ServiceId = rỗng oce tạo bản ghi, null k tao đc   => ServiceId trả về rỗng, có TuitionId thi phải trả về 3 trường sau tuitionAmount, discount
            {
                var payment = new Payment
                {
                    PaymentDate = DateOnly.FromDateTime(DateTime.Now),
                    TotalAmount = request.Amount,
                    Status = 0, // Assume 0 as 'Pending'
                    TuitionId = request.TuitionId,
                    ServiceId = null,
                    StudentId = request.ChildId,
                    PaymentName = request.PaymentName,
                };

                _context.Payments.Add(payment);
            }
            if ((request.ServiceId != null || request.ServiceId.Any()) && (request.TuitionId == 0 || request.TuitionId == null)) // request.TuitionId = rỗng lỗi, = null và ServiceId = 1 or 2 tạo=> TuitionId phải trả về null
            {

                foreach (var serviceId in request.ServiceId)
                {
                    var payment = new Payment
                    {
                        PaymentDate = DateOnly.FromDateTime(DateTime.Now),
                        TotalAmount = request.Amount,
                        Status = 0, // Assume 0 as 'Pending'
                        TuitionId = null,
                        ServiceId = serviceId,
                        StudentId = request.ChildId,
                        PaymentName = request.PaymentName,
                    };

                    _context.Payments.Add(payment);
                }
            }
            if ((request.ServiceId != null || request.ServiceId.Any()) && (request.TuitionId != null && request.TuitionId != 0))
            {
                foreach (var serviceId in request.ServiceId)
                {
                    var payment = new Payment
                    {
                        PaymentDate = DateOnly.FromDateTime(DateTime.Now),
                        TotalAmount = request.Amount,
                        Status = 0, // Assume 0 as 'Pending'
                        TuitionId = request.TuitionId,
                        ServiceId = serviceId,
                        StudentId = request.ChildId,
                        PaymentName = request.PaymentName,
                    };

                    _context.Payments.Add(payment);
                }
            }

            await _context.SaveChangesAsync(); // Save payment record to database


            var paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return Ok(new { url = paymentUrl });
        }
        public class VNPayCallbackRequest
        {
            public Dictionary<string, string> Data { get; set; }

        }

        //API để xử lý phản hồi từ VNPay sau khi thanh toán
        [HttpPost("payment-callback")]
        public async Task<IActionResult> PaymentCallback([FromBody] VNPayCallbackRequest request)
        {
            var vnpayData = HttpContext.Request.Query;
            var vnp_HashSecret = _configuration["VNPaySettings:vnp_HashSecret"];
            var vnpay = new VNPayLibrary();

            foreach (var item in request.Data)
            {
                if (!string.IsNullOrEmpty(item.Key) && item.Key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(item.Key, item.Value);
                }
            }

            // Kiểm tra chữ ký hợp lệ
            var isValidSignature = vnpay.ValidateSignature(request.Data["vnp_SecureHash"], vnp_HashSecret);

            if (isValidSignature)
            {
                var transactionRef = vnpay.GetResponseData("vnp_TxnRef");
                var resultCode = vnpay.GetResponseData("vnp_ResponseCode");

                // Parse the transaction reference to extract ChildId, TuitionId, and ServiceIds
                var refParts = transactionRef.Split('-');

                // Extract the ChildId and TuitionId
                var childId = int.Parse(refParts[0]);
                var tuitionId = int.Parse(refParts[1]);

                // Extract all ServiceIds (everything between ChildId and TuitionId)
                var serviceIds = refParts.Skip(2).Take(refParts.Length - 3).Select(int.Parse).ToList();

                if (resultCode == "00") // Thanh toán thành công
                {
                    var now = DateTime.Now;
                    // Cập nhật trạng thái Payment thành 1 (đã thanh toán)
                    var payments = await _context.Payments
                        .Where(p => p.StudentId == childId && p.Status == 0)
                        .ToListAsync();
                    foreach (var payment in payments)
                    {
                        payment.Status = 1; // Đánh dấu là đã thanh toán
                    }

                    // Cập nhật Tuition cho học sinh
                    if (tuitionId != 0 || tuitionId != null) {
                        var tuition = await _context.Tuitions
                        .FirstOrDefaultAsync(t => t.StudentId == childId && t.TuitionId == tuitionId);
                        if (tuition != null)
                        {
                            tuition.IsPaid = 1; // Đánh dấu là đã thanh toán
                        }
                    }

                    if (serviceIds != null && serviceIds.Any() && serviceIds.All(id => id != 0)) { 

                        // Cập nhật trạng thái Service trong Checkservice
                        var checkServices = await _context.Checkservices
                            .Where(cs => cs.StudentId == childId && serviceIds.Contains(cs.ServiceId) && cs.DatePayService == null && cs.Status == 1)
                            .ToListAsync();
                        foreach (var checkService in checkServices)
                        {
                            checkService.DatePayService = DateOnly.FromDateTime(now);
                            checkService.PayService = 1; // Đánh dấu dịch vụ đã hoàn thành
                        }
                    }

                    await _context.SaveChangesAsync();

                    return Ok(new { message = "Thanh toán thành công!" });
                }
                else
                {
                    var payments = await _context.Payments
                       .Where(p => p.StudentId == childId && p.Status == 0)
                       .ToListAsync();
                    foreach (var payment in payments)
                    {
                        payment.Status = 2; // Đánh dấu là hủy
                    }
                    await _context.SaveChangesAsync();

                    var payments1 = await _context.Payments
                        .Where(p => p.StudentId == childId && p.Status == 2).ToListAsync();
                    _context.Payments.RemoveRange(payments1);
                    await _context.SaveChangesAsync();

                    return BadRequest(new { message = "Thanh toán thất bại!" });
                }
            }
            else
            {
                return BadRequest(new { message = "Chữ ký không hợp lệ!" });
            }
        }

        [HttpGet("getAllDiscount")]
        public async Task<IActionResult> GetAllDiscount() {

            var allDiscount = _context.Discounts.ToList();
            return Ok(allDiscount);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Discount>> GetDiscount(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);

            if (discount == null)
            {
                return NotFound(new { message = "Discount not found" });
            }

            return discount;
        }

        [HttpPost("CreateDiscount")]
        public async Task<ActionResult<Discount>> CreateDiscount([FromBody] DiscountDTO discount)
        {
            if (discount == null)
            {
                return BadRequest(new { message = "Invalid discount data" });
            }
            var discount1 = new Discount
            {
                Number = discount.Number,
                Discount1 = discount.Discount1,
            };
            _context.Discounts.Add(discount1);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDiscount), new { id = discount.DiscountId }, discount);
        }

        [HttpPut("UpdateDiscount")]
        public async Task<IActionResult> UpdateDiscount([FromBody] DiscountDTO discount)
        {
            if (discount.DiscountId != discount.DiscountId)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var existingDiscount = await _context.Discounts.FindAsync(discount.DiscountId);
            if (existingDiscount == null)
            {
                return NotFound(new { message = "Discount not found" });
            }

            // Update the fields
            existingDiscount.Number = discount.Number;
            existingDiscount.Discount1 = discount.Discount1;

            _context.Discounts.Update(existingDiscount);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("DeleteDiscount/{id}")]
        public async Task<IActionResult> DeleteDiscount(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
            {
                return NotFound(new { message = "Discount not found" });
            }

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("history/{parentId}")]
        public async Task<IActionResult> GetPaymentHistory(int parentId)
        {
            // Lấy danh sách trẻ em của phụ huynh
            var children = await _context.Children
                .Where(c => c.ParentId == parentId)
                .ToListAsync();

            if (children == null || children.Count == 0)
            {
                return NotFound(new { message = "Không có trẻ nào của phụ huynh này." });
            }

            // Lấy lịch sử thanh toán và include các navigation properties
            var paymentHistory = await _context.Payments
                .Where(p => children.Select(c => c.StudentId).Contains(p.StudentId) && p.Status == 1)
                .Include(p => p.Tuition)
                    .ThenInclude(t => t.Semester)
                .Include(p => p.Tuition)
                    .ThenInclude(t => t.Discount)
                .Include(p => p.Student)
                .Include(p => p.Service)
                .ToListAsync();

            if (paymentHistory == null || paymentHistory.Count == 0)
            {
                return NotFound(new { message = "Không tìm thấy lịch sử thanh toán." });
            }

            // Grouping payments
            var groupedPayments = paymentHistory
                .GroupBy(p => new
                {
                    p.StudentId,
                    p.PaymentDate,
                    p.TotalAmount,
                    p.PaymentName,
                    p.TuitionId
                })
                .ToList();

            // Map dữ liệu
            var finalResult = groupedPayments.Select(g => new
            {
                ChildId = g.Key.StudentId,
                ChildName = g.FirstOrDefault()?.Student?.FullName ?? "Unknown",
                PaymentDate = g.Key.PaymentDate,
                TotalAmount = g.Key.TotalAmount,
                PaymentName = g.Key.PaymentName,
                TuitionId = g.Key.TuitionId,
                TuitionDetails = g.FirstOrDefault()?.Tuition == null ? null : new
                {
                    g.FirstOrDefault().Tuition.TuitionId,
                    g.FirstOrDefault().Tuition.StudentId,
                    g.FirstOrDefault().Tuition.SemesterId,
                    g.FirstOrDefault().Tuition.StartDate,
                    g.FirstOrDefault().Tuition.EndDate,
                    g.FirstOrDefault().Tuition.TuitionFee,
                    g.FirstOrDefault().Tuition.DueDate,
                    g.FirstOrDefault().Tuition.IsPaid,
                    g.FirstOrDefault().Tuition.TotalFee,
                    g.FirstOrDefault().Tuition.DiscountId,
                    g.FirstOrDefault().Tuition.StatusTuitionLate,
                    DiscountDetails = g.FirstOrDefault().Tuition.Discount == null ? null : new
                    {
                        g.FirstOrDefault().Tuition.Discount.DiscountId,
                        g.FirstOrDefault().Tuition.Discount.Number,
                        g.FirstOrDefault().Tuition.Discount.Discount1
                    },
                    SemesterDetails = g.FirstOrDefault().Tuition.Semester == null ? null : new
                    {
                        g.FirstOrDefault().Tuition.Semester.SemesterId,
                        g.FirstOrDefault().Tuition.Semester.Name,
                        g.FirstOrDefault().Tuition.Semester.StartDate,
                        g.FirstOrDefault().Tuition.Semester.EndDate
                    }
                },
                Services = g.Where(p => p.ServiceId != null).Select(p => new
                {
                    p.ServiceId,
                    p.Service?.ServiceName,
                    p.Service?.ServicePrice,
                    Quantity = _context.Checkservices
                        .Where(cs => cs.ServiceId == p.ServiceId
                                     && cs.StudentId == p.StudentId
                                     && cs.Status == 1
                                     && cs.PayService == 1
                                     && cs.DatePayService == p.PaymentDate)
                        .Count(),
                    TotalPrice = _context.Checkservices
                        .Where(cs => cs.ServiceId == p.ServiceId
                                     && cs.StudentId == p.StudentId
                                     && cs.PayService == 1
                                     && cs.DatePayService == p.PaymentDate)
                        .Sum(cs => cs.Service.ServicePrice)
                }).ToList()
            }).ToList();

            return Ok(finalResult);
        }

        [HttpGet("get-all-payment-histories")]
        public async Task<IActionResult> GetAllPaymentHistories()
        {
            // Lấy tất cả trẻ em trong hệ thống
            var children = await _context.Children.ToListAsync();

            if (children == null || children.Count == 0)
            {
                return NotFound(new { message = "Không tìm thấy danh sách trẻ." });
            }

            // Lấy tất cả lịch sử thanh toán và include các navigation properties
            var paymentHistory = await _context.Payments
                .Where(p => p.Status == 1) // Lọc chỉ những thanh toán đã được thực hiện
                .Include(p => p.Tuition)
                .ThenInclude(t => t.Semester)
                .Include(p => p.Tuition)
                .ThenInclude(t => t.Discount)
                .Include(p => p.Student)
                .ThenInclude(x => x.Parent)
                .Include(p => p.Service)
                .ToListAsync();

            if (paymentHistory == null || paymentHistory.Count == 0)
            {
                return NotFound(new { message = "Không tìm thấy lịch sử thanh toán." });
            }

            // Grouping payments
            var groupedPayments = paymentHistory
                .GroupBy(p => new
                {
                    p.StudentId,
                    p.PaymentDate,
                    p.TotalAmount,
                    p.PaymentName,
                    p.TuitionId
                })
                .ToList();

            // Map dữ liệu
            var finalResult = groupedPayments.Select(g => new
            {
                ParentId = g.FirstOrDefault()?.Student?.ParentId,
                ParentName = g.FirstOrDefault()?.Student?.Parent?.Name ?? "Unknown",
                ChildId = g.Key.StudentId,
                ChildName = g.FirstOrDefault()?.Student?.FullName ?? "Unknown",
                PaymentDate = g.Key.PaymentDate,
                TotalAmount = g.Key.TotalAmount,
                PaymentName = g.Key.PaymentName,
                TuitionId = g.Key.TuitionId,
                TuitionDetails = g.FirstOrDefault()?.Tuition == null ? null : new
                {
                    g.FirstOrDefault().Tuition.TuitionId,
                    g.FirstOrDefault().Tuition.StudentId,
                    g.FirstOrDefault().Tuition.SemesterId,
                    g.FirstOrDefault().Tuition.StartDate,
                    g.FirstOrDefault().Tuition.EndDate,
                    g.FirstOrDefault().Tuition.TuitionFee,
                    g.FirstOrDefault().Tuition.DueDate,
                    g.FirstOrDefault().Tuition.IsPaid,
                    g.FirstOrDefault().Tuition.TotalFee,
                    g.FirstOrDefault().Tuition.DiscountId,
                    g.FirstOrDefault().Tuition.StatusTuitionLate,
                    DiscountDetails = g.FirstOrDefault().Tuition.Discount == null ? null : new
                    {
                        g.FirstOrDefault().Tuition.Discount.DiscountId,
                        g.FirstOrDefault().Tuition.Discount.Number,
                        g.FirstOrDefault().Tuition.Discount.Discount1
                    },
                    SemesterDetails = g.FirstOrDefault().Tuition.Semester == null ? null : new
                    {
                        g.FirstOrDefault().Tuition.Semester.SemesterId,
                        g.FirstOrDefault().Tuition.Semester.Name,
                        g.FirstOrDefault().Tuition.Semester.StartDate,
                        g.FirstOrDefault().Tuition.Semester.EndDate
                    }
                },
                Services = g.Where(p => p.ServiceId != null).Select(p => new
                {
                    p.ServiceId,
                    p.Service?.ServiceName,
                    p.Service?.ServicePrice,
                    Quantity = _context.Checkservices
                        .Where(cs => cs.ServiceId == p.ServiceId
                                     && cs.StudentId == p.StudentId
                                     && cs.PayService == 1
                                     && cs.DatePayService == g.Key.PaymentDate)
                        .Count(),
                    TotalPrice = _context.Checkservices
                        .Where(cs => cs.ServiceId == p.ServiceId
                                     && cs.StudentId == p.StudentId
                                     && cs.PayService == 1
                                     && cs.DatePayService == g.Key.PaymentDate)
                        .Sum(cs => cs.Service.ServicePrice)
                }).ToList()
            }).ToList();

            return Ok(finalResult);
        }

    }
}
