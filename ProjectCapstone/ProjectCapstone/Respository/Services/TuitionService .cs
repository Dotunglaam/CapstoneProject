using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.DTOS;
using System.Drawing;
using System.Web.Http.Results;
using System.Xml.Linq;
using MimeKit;
using MailKit.Net.Smtp;
using System.Threading.Tasks;

namespace Respository.Services
{
    public class TuitionService : ITuitionService
    {
        private readonly IResetPasswordTokenRepository _ResetPasswordToken;
        private readonly kmsContext _context;
        private readonly IConfiguration _configuration;
        public TuitionService(kmsContext context, IResetPasswordTokenRepository resetPasswordTokenRepository, IConfiguration configuration)
        {
            _context = context;
            _ResetPasswordToken = resetPasswordTokenRepository;
            _configuration = configuration;
        }

        public async Task GenerateMonthlyTuitionRecords()
        {
            DateTime now = DateTime.Now;
            DateTime lastDayOfMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 20, 0, 0);

            if (now.Date == lastDayOfMonth.Date && now.Hour == 20 && now.Minute == 0)
            //if (now.Day == 9)
            {
                var currentSemester = await _context.Semesters
                                        .Where(s => s.StartDate.Value.AddMonths(-1) <= now && s.EndDate >= now)
                                        .OrderByDescending(s => s.StartDate)
                                        .FirstOrDefaultAsync();

                if (currentSemester == null)
                {
                    Console.WriteLine("Không có kỳ học nào hiện tại.");
                    return;
                }

                var children = await _context.Children.Include(c => c.Parent).ThenInclude(x => x.ParentNavigation).Where(x => x.Status == 1).ToListAsync();

                foreach (var child in children)
                {
                    DateTime nextMonth = now.AddMonths(1);
                    if (nextMonth >= currentSemester.StartDate && nextMonth <= currentSemester.EndDate)
                    {
                        // Kiểm tra xem đã có bản ghi học phí với thời gian EndDate lớn hơn tháng kế tiếp không
                        var existingTuitionRecord = await _context.Tuitions
                        .Where(t => t.StudentId == child.StudentId && t.EndDate >= new DateTime(nextMonth.Year, nextMonth.Month, 1))
                        .FirstOrDefaultAsync();


                        if (existingTuitionRecord == null)
                        {
                            var grade = await _context.Grades.FindAsync(child.GradeId);
                            if (grade == null) continue;

                            decimal baseTuitionFee = grade.BaseTuitionFee ?? 0;
                            var tuitionRecord = new Tuition
                            {
                                StudentId = child.StudentId,
                                SemesterId = currentSemester.SemesterId,
                                StartDate = new DateTime(nextMonth.Year, nextMonth.Month, 1),
                                EndDate = new DateTime(nextMonth.Year, nextMonth.Month, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month)),
                                TuitionFee = baseTuitionFee,
                                DueDate = new DateOnly(nextMonth.Year, nextMonth.Month, 5),
                                DiscountId = 1,
                                TotalFee = baseTuitionFee,
                                IsPaid = 0,
                                StatusTuitionLate = 0,
                                LastEmailSentDate = null,
                                SendMailByPr = 0 
                            };

                            await _context.Tuitions.AddAsync(tuitionRecord);

                            // Lấy các dịch vụ mà học sinh đã sử dụng trong tháng này
                            var servicesUsed = await _context.Checkservices
                                .Where(cs => cs.StudentId == child.StudentId && cs.Status == 1 && cs.PayService == 0)
                                .Include(cs => cs.Service)
                                .ToListAsync();

                            // Tính tổng tiền của các dịch vụ đã sử dụng
                            decimal serviceTotal = 0;
                            var serviceDetails = new List<ServiceDetailDto>();  // Lưu chi tiết dịch vụ

                            foreach (var serviceUsage in servicesUsed)
                            {
                                decimal serviceCost = (decimal)(serviceUsage.Service.ServicePrice ?? 0);
                                serviceTotal += serviceCost;

                                serviceDetails.Add(new ServiceDetailDto
                                {
                                    ServiceName = serviceUsage.Service.ServiceName,
                                    Price = serviceCost,
                                    DateUsed = serviceUsage.Date.Value.ToDateTime(new TimeOnly(0, 0)),
                                    Quantity = 1, // Giả sử mỗi lần sử dụng là 1 đơn vị
                                    ServiceDescription = serviceUsage.Service.ServiceDes
                                });
                            }

                            // Tạo file Excel với thông tin dịch vụ
                            string excelFilePath = await GenerateExcelFile(serviceDetails);

                            await SendTuitionEmailNotification(child.Parent, child.FullName, tuitionRecord, serviceTotal, excelFilePath);
                        }
                        else
                        {
                            var existingTuitionRecord1 = await _context.Tuitions
                            .Where(t => t.StudentId == child.StudentId && t.EndDate >= new DateTime(nextMonth.Year, nextMonth.Month, 1))
                            .ToListAsync();
                            foreach (var t in existingTuitionRecord1)
                            {
                                t.SendMailByPr = 1;
                            }
                            // Lấy các dịch vụ mà học sinh đã sử dụng trong tháng này
                            var servicesUsed = await _context.Checkservices
                                .Where(cs => cs.StudentId == child.StudentId && cs.Status == 1 && cs.PayService == 0)
                                .Include(cs => cs.Service)
                                .ToListAsync();

                            // Tính tổng tiền của các dịch vụ đã sử dụng
                            decimal serviceTotal = 0;
                            var serviceDetails = new List<ServiceDetailDto>();  // Lưu chi tiết dịch vụ

                            foreach (var serviceUsage in servicesUsed)
                            {
                                decimal serviceCost = (decimal)(serviceUsage.Service.ServicePrice ?? 0);
                                serviceTotal += serviceCost;

                                serviceDetails.Add(new ServiceDetailDto
                                {
                                    ServiceName = serviceUsage.Service.ServiceName,
                                    Price = serviceCost,
                                    DateUsed = serviceUsage.Date.Value.ToDateTime(new TimeOnly(0, 0)),
                                    Quantity = 1, // Giả sử mỗi lần sử dụng là 1 đơn vị
                                    ServiceDescription = serviceUsage.Service.ServiceDes
                                });
                            }

                            // Tạo file Excel với thông tin dịch vụ
                            string excelFilePath = await GenerateExcelFile(serviceDetails);

                            await SendTuitionEmailNotification(child.Parent, child.FullName, existingTuitionRecord, serviceTotal, excelFilePath);
                        }



                    }
                }

                await _context.SaveChangesAsync();  // Lưu thay đổi bất đồng bộ
            }
        }
        public async Task<List<TuitionRecord>> GenerateMonthlyTuitionRecordsClick(bool overrideCheck = false)
        {
            DateTime now = DateTime.Now;
            DateTime lastDayOfMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 20, 0, 0);

            if (!overrideCheck && now != lastDayOfMonth)
            {
                Console.WriteLine("Hôm nay không phải ngày cuối tháng.");
                return new List<TuitionRecord>();  // Trả về danh sách rỗng nếu không phải ngày cuối tháng
            }

            List<TuitionRecord> createdTuitionRecords = new List<TuitionRecord>();  // Danh sách lưu trữ bản ghi học phí mới tạo dưới dạng DTO

            if (now == lastDayOfMonth || overrideCheck == true)
            {
                var currentSemester = await _context.Semesters
                        .Where(s => s.StartDate.Value.AddMonths(-1) <= now && s.EndDate >= now)
                        .OrderByDescending(s => s.StartDate)
                        .FirstOrDefaultAsync();

                if (currentSemester == null)
                {
                    Console.WriteLine("Không có kỳ học nào hiện tại.");
                    return createdTuitionRecords;  // Trả về danh sách rỗng nếu không có kỳ học
                }

                var children = await _context.Children.Include(c => c.Parent).ThenInclude(x => x.ParentNavigation).Where(x => x.Status == 1).ToListAsync();

                foreach (var child in children)
                {
                    DateTime nextMonth = now.AddMonths(1);
                    if (nextMonth >= currentSemester.StartDate && nextMonth <= currentSemester.EndDate)
                    {
                        // Kiểm tra xem đã có bản ghi học phí với thời gian EndDate lớn hơn tháng kế tiếp không
                        var existingTuitionRecord = await _context.Tuitions
                        .Where(t => t.StudentId == child.StudentId && t.EndDate >= new DateTime(nextMonth.Year, nextMonth.Month, 1))
                        .FirstOrDefaultAsync();

                        if (existingTuitionRecord == null)
                        {
                            var grade = await _context.Grades.FindAsync(child.GradeId);
                            if (grade == null) continue;

                            decimal baseTuitionFee = grade.BaseTuitionFee ?? 0;
                            var tuitionRecord = new Tuition
                            {
                                StudentId = child.StudentId,
                                SemesterId = currentSemester.SemesterId,
                                StartDate = new DateTime(nextMonth.Year, nextMonth.Month, 1),
                                EndDate = new DateTime(nextMonth.Year, nextMonth.Month, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month)),
                                TuitionFee = baseTuitionFee,
                                DueDate = new DateOnly(nextMonth.Year, nextMonth.Month, 5),
                                DiscountId = 1,
                                TotalFee = baseTuitionFee,
                                IsPaid = 0,
                                StatusTuitionLate = 0,
                                LastEmailSentDate = null,
                                SendMailByPr = 0
                            };

                            await _context.Tuitions.AddAsync(tuitionRecord);
                            await _context.SaveChangesAsync();  // Lưu bản ghi học phí ngay khi thêm mới

                            // Chuyển đổi bản ghi học phí sang DTO và thêm vào danh sách
                            var tuitionDTO = new TuitionRecord
                            {
                                TuitionId = tuitionRecord.TuitionId,
                                StudentId = tuitionRecord.StudentId,
                                SemesterId = tuitionRecord.SemesterId,
                                StartDate = tuitionRecord.StartDate,
                                EndDate = tuitionRecord.EndDate,
                                TuitionFee = tuitionRecord.TuitionFee,
                                TotalFee = tuitionRecord.TotalFee,
                                DueDate = tuitionRecord.DueDate,
                                DiscountId = tuitionRecord.DiscountId,
                                IsPaid = tuitionRecord.IsPaid,
                                StatusTuitionLate = tuitionRecord.StatusTuitionLate,
                                LastEmailSentDate = tuitionRecord.LastEmailSentDate,
                                SendMailByPr = (int)(tuitionRecord.SendMailByPr),
                            };

                            createdTuitionRecords.Add(tuitionDTO);  // Thêm DTO vào danh sách
                        }
                    }
                }
            }

            return createdTuitionRecords;  // Trả về danh sách các bản ghi học phí dưới dạng DTO
        }

        public async Task<string> GenerateExcelFile(List<ServiceDetailDto> serviceDetails)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Sử dụng thư viện EPPlus hoặc ClosedXML để tạo file Excel
            var filePath = Path.Combine(Path.GetTempPath(), $"ServiceDetails_{Guid.NewGuid()}.xlsx");

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets.Add("Service Details");

                // Thêm tiêu đề
                worksheet.Cells[1, 1].Value = "Service Name";
                worksheet.Cells[1, 2].Value = "Price";
                worksheet.Cells[1, 3].Value = "Date Used";
                worksheet.Cells[1, 4].Value = "Quantity";
                worksheet.Cells[1, 5].Value = "Description";
                worksheet.Cells[1, 6].Value = "Thành Tiền";
                int row = 2;
                foreach (var detail in serviceDetails)
                {
                    worksheet.Cells[row, 1].Value = detail.ServiceName;
                    worksheet.Cells[row, 2].Value = detail.Price;
                    worksheet.Cells[row, 3].Value = detail.DateUsed.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 4].Value = detail.Quantity;
                    worksheet.Cells[row, 5].Value = detail.ServiceDescription;
                    worksheet.Cells[row, 6].Formula = $"B{row}*D{row}"; //
                    row++;
                }
                // Tính tổng số tiền cho cột "Thành Tiền"
                worksheet.Cells[row + 1, 5].Value = "Tổng Số Tiền:";
                worksheet.Cells[row + 1, 6].Formula = $"SUM(F2:F{row - 1})";
                await package.SaveAsync();
            }

            return filePath;
        }

        private async Task SendTuitionEmailNotification(Parent parent, string name, Tuition tuitionRecord, decimal serviceTotal, string excelFilePath)
        {
            // Kiểm tra nếu tuitionRecord là null
            if (tuitionRecord.IsPaid == 1)
            {
                ;
                DateTime nextMonth = tuitionRecord.EndDate.Value.AddMonths(1);
                DateTime endDateTime = new DateTime(nextMonth.Year, nextMonth.Month, 1);
                DateOnly endDateOnly = DateOnly.FromDateTime(endDateTime);
                // Trường hợp đã đóng học phí
                string emailBody = $@"
                <html>
                    <body style=""font-family: Arial, sans-serif;"">
                        <div style=""border: 1px solid #ccc; padding: 20px; width: 100%; box-sizing: border-box;"">
                            <p>Kính gửi Quý khách hàng <b>{parent.Name}</b>,</p>
                            <p>Xin trân trọng cảm ơn Quý khách hàng đã sử dụng dịch vụ của chúng tôi.</p>
                            <p>Đơn vị <b>Nhà trẻ KMS</b> của Quý khách hàng.</p>
                            <p>Thông tin về học phí cho trẻ <b>{name}</b>:</p>  
                            <p>1. Học phí cho tháng {DateTime.Now.AddMonths(1):MM/yyyy}: <b>Đã đóng học phí từ {DateOnly.FromDateTime((DateTime)tuitionRecord.StartDate)} đến {DateOnly.FromDateTime((DateTime)tuitionRecord.EndDate)}.</b></p>
                            <p></p>
                            <p>2. Tổng chi phí dịch vụ trong tháng: <b>{serviceTotal.ToString("N0")}</b> VND</p>
                            <p>Để xem chi tiết các dịch vụ đã sử dụng trong tháng, vui lòng tải file Excel ở tệp đính kèm dưới đây (nếu không có dịch vụ vui lòng bỏ qua)</p>
                            <p>Quý khách hãy kiểm tra mục thanh toán trên Web để xem chi tiết các mục phải đóng</p>
                            <p><b>Vui lòng đăng nhập link sau để thực hiện thanh toán: <a href=""http://edunest.io.vn/"">http://edunest.io.vn/</a></b></p>  
                            <p>Trân trọng cảm ơn Quý khách và chúc Quý khách nhiều thành công khi sử dụng dịch vụ!</p>
                        </div>  
                    </body>
                </html>
                ";


                await SendMailWithAttachment(new List<string> { parent.ParentNavigation.Mail }, $"Thông báo về học phí cho tháng {DateTime.Now.AddMonths(1):MM/yyyy}.", emailBody, excelFilePath);
            }
            else
            {
                // Trường hợp chưa có học phí
                string formattedTuitionFee = tuitionRecord.TuitionFee?.ToString("N0") ?? "0";
                string formattedServiceTotal = serviceTotal.ToString("N0");

                string emailBody = $@"
                <html>
                    
                    <body style=""font-family: Arial, sans-serif;"">
                        <div style=""border: 1px solid #ccc; padding: 20px; width: 100%; box-sizing: border-box;"">
                            <p>Kính gửi Quý khách hàng <b>{parent.Name}</b>,</p>
                            <p>Xin trân trọng cảm ơn Quý khách hàng đã sử dụng dịch vụ của chúng tôi.</p>
                            <p>Đơn vị <b>Nhà trẻ ABC</b> của Quý khách hàng.</p>
                            <p>Thông tin đóng tiền của khách hàng cho trẻ <b>{name}</b> gồm có:</p>  
                            <p>1. Học phí cho tháng {DateTime.Now.AddMonths(1):MM/yyyy}: <b>{formattedTuitionFee}</b> VND. Hạn đến <b>{tuitionRecord.DueDate}</p>  
                            <p></p>
                            <p>2. Tổng chi phí dịch vụ trong tháng: <b>{formattedServiceTotal}</b> VND</p>
                            <p>Để xem chi tiết các dịch vụ đã sử dụng trong tháng, vui lòng tải file Excel ở tệp đính kèm dưới đây (nếu không có dịch vụ vui lòng bỏ qua)</p>
                            <p>Quý khách hãy kiểm tra mục thanh toán trên Web để xem chi tiết các mục phải đóng</p>
                            <p><b>Vui lòng đăng nhập link sau để thực hiện thanh toán: <a href=""http://edunest.io.vn/"">http://edunest.io.vn/</a></b></p>  
                            <p>Trân trọng cảm ơn Quý khách và chúc Quý khách nhiều thành công khi sử dụng dịch vụ!</p>
                        </div>  
                    </body>
                </html>
                ";

                await SendMailWithAttachment(new List<string> { parent.ParentNavigation.Mail }, $"Thông báo về học phí cho tháng {DateTime.Now.AddMonths(1):MM/yyyy}.", emailBody, excelFilePath);
            }
        }




        public async Task SendMailWithAttachment(IEnumerable<string> emails, string subject, string body, string attachmentPath)
        {
            var tasks = emails.Select(email => SendMailAsync(email, subject, body, attachmentPath));
            await Task.WhenAll(tasks); // Gửi tất cả email song song
        }

        public async Task SendMailAsync(string email, string subject, string body, string attachmentPath)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Edunest", "lamdthe163085@fpt.edu.vn"));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = body };

                // Thêm file đính kèm nếu có
                if (File.Exists(attachmentPath))
                {
                    bodyBuilder.Attachments.Add(attachmentPath);
                }
                else
                {
                    Console.WriteLine("File không tồn tại: " + attachmentPath);
                    return;
                }

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, false);  // Sử dụng Gmail SMTP hoặc dịch vụ khác
                    await client.AuthenticateAsync("lamdthe163085@fpt.edu.vn", "iyni xhmb rtij tnen");
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                Console.WriteLine("Email sent to: " + email);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email to " + email + ": " + ex.Message);
            }
        }


        public async Task<List<object>> GetTuitionRecordsByParentIdAsync(int parentId)
        {
            // Xác định tháng và năm cho học phí và dịch vụ
            var children = await _context.Children
            .Where(c => c.ParentId == parentId && c.Status == 1)

            .Select(c => new
            {
                c.StudentId,
                c.GradeId,
                c.Code,
                c.FullName,
                c.NickName,
                c.Dob,
                c.Gender,
                c.Status,
                c.EthnicGroups,
                c.Nationality,
                c.Religion,
                c.Avatar,

                // Học phí chưa thanh toán cho tháng sau (tháng tiếp theo)
                Tuition = c.Tuitions.Where(t => t.IsPaid == 0) // Lấy học phí của tháng tiếp theo
                .Select(t => new
                {
                    t.TuitionId,
                    t.StudentId,
                    t.SemesterId,
                    t.StartDate,
                    t.EndDate,
                    t.TuitionFee,
                    t.DueDate,
                    t.TotalFee,
                    t.DiscountId,
                    t.IsPaid,
                    t.StatusTuitionLate,
                }).ToList(),
                // Dịch vụ đã sử dụng của tháng trước (tháng trước)
                ServicesUsed = c.Checkservices
                .Where(cs => cs.Date.HasValue
                             && cs.Status == 1 && cs.PayService == 0) // Lọc dịch vụ đã sử dụng
                .GroupBy(cs => new { cs.Service.ServiceId, cs.Service.ServiceName, cs.Service.ServicePrice })
                .Select(g => new
                {
                    Service = g.Key.ServiceId,
                    ServiceName = g.Key.ServiceName,
                    Price = g.Key.ServicePrice,
                    Quantity = g.Count(),
                }).ToList()
            })
            .ToListAsync();

            return children.Cast<object>().ToList(); // Return as a list of objects

        }
        public async Task<Result> SendTuitionReminderAsync(bool isManualTrigger = false)
        {
            var today = DateTime.Today;

            // Lọc các học phí cần gửi email (trong khoảng StartDate đến DueDate)
            var pendingTuitions = await _context.Tuitions
                .Where(t => t.DueDate.HasValue &&
                            t.StartDate.HasValue &&
                            t.StartDate.Value <= today && t.IsPaid == 0 &&
                            t.DueDate.Value >= DateOnly.FromDateTime(today)) // Kiểm tra không vượt quá DueDate
                .Include(t => t.Student)
                    .ThenInclude(s => s.Parent)
                        .ThenInclude(p => p.ParentNavigation)
                .ToListAsync();

            // Kiểm tra nếu không có học phí trong khoảng thời gian hợp lệ
            if (!pendingTuitions.Any())
            {
                return new Result
                {
                    Success = false,
                    Message = "Send an email reminder for tuition fee payment failure. It can only be sent between the 1st and 5th of each month!"
                };
            }

            bool emailSentToday = false;

            foreach (var tuition in pendingTuitions)
            {
                // Kiểm tra nếu đã gửi email trong ngày hôm nay
                if (tuition.LastEmailSentDate.HasValue && tuition.LastEmailSentDate.Value.Date == today)
                {
                    emailSentToday = true;
                    continue; // Bỏ qua nếu đã gửi email trong ngày hôm nay
                }

                var parentEmail = tuition.Student.Parent.ParentNavigation.Mail;
                var childName = tuition.Student.FullName;
                var dueDate = tuition.DueDate.Value.ToDateTime(TimeOnly.MinValue);

                // Soạn nội dung email
                var subject = "Thông báo đóng học phí - Sắp đến hạn thanh toán";
                var body = $@"
                    <html>
                        <body style=""font-family: Arial, sans-serif;"">
                            <div style=""border: 1px solid #ccc; padding: 20px; width: 100%; box-sizing: border-box; background-color: #f9f9f9;"">
                                <p>Xin chào <b>{tuition.Student.Parent.Name}</b>,</p>
                                <p>Chúng tôi xin thông báo rằng học phí của bé <b>{childName}</b> sẽ đến hạn thanh toán vào ngày <b>{dueDate:dd/MM/yyyy}</b>.</p>
                                <p>Để đảm bảo không làm gián đoạn quá trình học tập của bé, vui lòng hoàn tất thanh toán trước ngày này.</p>
                                <p><b>Vui lòng đăng nhập link sau để thực hiện thanh toán: <a href=""http://edunest.io.vn/"">http://edunest.io.vn/</a></b></p>  
                                <p>Trân trọng,</p>
                                <p><b>Nhà trường</b></p>
                            </div>  
                        </body>
                    </html>
                    ";

                // Gửi email thủ công hoặc tự động
                _ResetPasswordToken.SendMail(parentEmail, subject, body);

                // Cập nhật thời gian gửi email cho học phí này
                tuition.LastEmailSentDate = DateTime.Today;
                _context.Update(tuition);
            }

            // Nếu đã gửi email trong ngày, trả về thông báo
            if (emailSentToday)
            {
                return new Result
                {
                    Success = false,
                    Message = "The tuition fee reminder email has been sent today. Please do not send it again."
                };
            }

            // Nếu gửi thành công
            await _context.SaveChangesAsync();

            return new Result
            {
                Success = true,
                Message = "Email send sucsecc."
            };
        }

        //send mail dong phat qua han hoc phi va update trong DB
        public async Task UpdateTuitionStatusAndNotifyAsync()
        {
            try
            {
                var currentDate = DateTime.Today;

                // Lọc các bản ghi học phí có DueDate đã qua mà chưa thanh toán
                var overdueTuitions = await _context.Tuitions
                    .Where(t => t.DueDate.HasValue && t.DueDate.Value.ToDateTime(TimeOnly.MinValue) < currentDate && t.IsPaid == 0)
                    .Include(t => t.Student)
                        .ThenInclude(s => s.Parent)
                            .ThenInclude(p => p.ParentNavigation)  // Lấy thông tin người dùng
                    .ToListAsync();

                foreach (var tuition in overdueTuitions)
                {
                    // Cập nhật trạng thái và tính lại tổng học phí
                    tuition.StatusTuitionLate = 1;  // Đánh dấu là đã quá hạn
                    tuition.TotalFee = tuition.TuitionFee + 200000; 

                    // Lưu thay đổi vào cơ sở dữ liệu
                    _context.Tuitions.Update(tuition);
                    await _context.SaveChangesAsync();

                    // Gửi email thông báo cho phụ huynh
                    var parentEmail = tuition.Student.Parent.ParentNavigation.Mail;
                    var childName = tuition.Student.FullName;
                    var dueDate = tuition.DueDate.Value.ToString("dd/MM/yyyy");

                    var subject = "Thông báo học phí đã quá hạn";
                    var body = $@"
                    <p>Xin chào <b>{tuition.Student.Parent.Name}</b>,</p>
                    <p>Học phí của bé <b>{childName}</b> đã quá hạn thanh toán vào ngày <b>{dueDate}</b>.</p>
                    <p>Tiền phạt do đóng muộn là <b>200,000 VND</b> sẽ được cộng vào tiền <b>học phí</b></p>
                    <p>Vui lòng thanh toán tiền học phí là: <b>{tuition.TuitionFee:N0} VND</b> với tiền phạt. Tổng số tiền là: <b>{tuition.TotalFee:N0} VND</b> để tránh gián đoạn việc học.</p>
                    <p><b>Vui lòng đăng nhập link sau để thực hiện thanh toán: <a href=""http://edunest.io.vn/"">http://edunest.io.vn/</a></b></p>  
                    <p>Trân trọng,</p>
                    <p>Nhà trường</p>";

                    // Gửi email
                    _ResetPasswordToken.SendMail(parentEmail, subject, body);
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nếu có
                Console.WriteLine($"Lỗi khi cập nhật học phí: {ex.Message}");
            }
        }
        public async Task<List<TuitionDTO>> GetAllTuitionsAsync()
        {
            return await _context.Tuitions 
                .Include(t => t.Student)
                .Include(t => t.Semester) 
                .Include(t => t.Discount) 
                .Select(t => new TuitionDTO
                {
                    TuitionId = t.TuitionId,
                    ParentName = t.Student.Parent.Name,
                    Mail = t.Student.Parent.ParentNavigation.Mail,
                    PhoneNumber = t.Student.Parent.ParentNavigation.PhoneNumber,
                    StudentId = t.StudentId,
                    StudentName = t.Student.FullName,
                    Code = t.Student.Code, 
                    SemesterId = t.SemesterId,
                    SemesterName = t.Semester.Name, 
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    TuitionFee = t.TuitionFee,
                    DueDate = t.DueDate,
                    IsPaid = t.IsPaid,
                    StatusTuitionLate = t.StatusTuitionLate,
                    LastEmailSentDate = t.LastEmailSentDate,
                    SendMailByPR = (int)t.SendMailByPr
                })
                .ToListAsync();
        }
        public async Task ApproveAndSendEmailsForAllTuitions()
        {
            var tuitionRecord = await _context.Tuitions
                .Include(t => t.Student)
                .Include(t => t.Student.Parent)
                .ThenInclude(x => x.ParentNavigation)
                .Where(x => x.IsPaid == 0 && x.SendMailByPr == 0)
                .ToListAsync();

            if (tuitionRecord == null)
            {
                return;
            }
            foreach (var record in tuitionRecord)
            {

                // Lấy các dịch vụ mà học sinh đã sử dụng trong tháng này
                var servicesUsed = await _context.Checkservices
                                .Where(cs => cs.StudentId == record.StudentId && cs.PayService == 0)
                                .Include(cs => cs.Service)
                                .ToListAsync();


                decimal serviceTotal = 0;
                var serviceDetails = new List<ServiceDetailDto>();

                foreach (var serviceUsage in servicesUsed)
                {
                    decimal serviceCost = (decimal)(serviceUsage.Service.ServicePrice ?? 0);
                    serviceTotal += serviceCost;

                    serviceDetails.Add(new ServiceDetailDto
                    {
                        ServiceName = serviceUsage.Service.ServiceName,
                        Price = serviceCost,
                        DateUsed = serviceUsage.Date.Value.ToDateTime(new TimeOnly(0, 0)),
                        Quantity = 1,
                        ServiceDescription = serviceUsage.Service.ServiceDes
                    });
                }

                // Tạo file Excel với thông tin dịch vụ
                string excelFilePath = await GenerateExcelFile(serviceDetails);

                await SendTuitionEmailNotification(record.Student.Parent, record.Student.FullName, record, serviceTotal, excelFilePath);
                //Console.WriteLine("Đã gửi email thông báo cho phụ huynh.");
                record.SendMailByPr = 1;
                
            }
            await _context.SaveChangesAsync();
        }
        public async Task<int> DeleteUnpaidTuitionsAsync()
        {
            var time = DateTime.Now.AddMonths(1);
            // Lấy tất cả các bản ghi Tuition có IsPaid = 0
            var unpaidTuitions = await _context.Tuitions
                                                .Where(t => t.IsPaid == 0 && time.Month == t.DueDate.Value.Month && time.Year == t.DueDate.Value.Year)
                                                .ToListAsync();

            // Nếu có bản ghi, xóa chúng
            if (unpaidTuitions.Any())
            {
                _context.Tuitions.RemoveRange(unpaidTuitions);
                var deletedCount = await _context.SaveChangesAsync();
                return deletedCount; // Trả về số bản ghi đã xóa
            }

            // Nếu không có bản ghi nào, trả về 0
            return 0;
        }
    }
}
