using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ChitFundManager.Data;
using AutoMapper;
using ChitFundManager.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace PaymentsController.Controllers
{
    public class PaymentsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public PaymentsController(IMapper mapper, AppDbContext context)
        {
            _mapper=mapper;
            _context=context;
        }

        [HttpGet("payments/{chitId}/month/{monthNumber}")]
        public async Task<IActionResult> GetPaymentsByMonth(Guid chitId, int monthNumber)
        {
            var payments = await _context.Payments
                .Where(p => p.ChitGroupId == chitId && p.MonthNumber == monthNumber)
                .Include(p => p.Member) // optional but useful
                .ToListAsync();

            if (payments == null || payments.Count == 0)
            {
                return NotFound("No payments found for this month.");
            }

            return Ok(payments);
        }


       [HttpPost("mark-paid")]
        public async Task<IActionResult> MarkPaymentAsPaid([FromBody] MarkPaymentDto request)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p =>
                    p.ChitGroupId == request.ChitGroupId &&
                    p.MemberId == request.MemberId &&
                    p.MonthNumber == request.MonthNumber);

            if (payment == null)
            {
                return NotFound("Payment record not found. It should be created during auction.");
            }

            // 🔥 Prevent overpayment
            if (request.AmountPaid > payment.AmountToPay)
            {
                return BadRequest("Amount exceeds remaining balance.");
            }

            // Reduce remaining amount
            payment.AmountToPay -= request.AmountPaid;

            // Increase total paid
            payment.AmountPaid += request.AmountPaid;

            // Check if fully paid
            if (payment.AmountToPay == 0)
            {
                payment.IsPaid = true;
                payment.PaidDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Payment updated successfully",
                remainingAmount = payment.AmountToPay,
                totalPaid = payment.AmountPaid,
                isFullyPaid = payment.IsPaid
            });
        }

        [HttpGet("member/{memberId}")]
        public async Task<IActionResult> GetMemberPaymentHistory(Guid memberId)
        {
            var payments = await _context.Payments
                .Where(p => p.MemberId == memberId)
                .OrderBy(p => p.MonthNumber)
                .ToListAsync();

            if (!payments.Any())
                return NotFound("No payment history found");

            var result = payments.Select(p => new
            {
                p.MonthNumber,
                p.AmountPaid,
                RemainingAmount = p.AmountToPay,
                p.IsPaid,
                p.PaidDate
            });

            return Ok(new
            {
                totalPaid = payments.Sum(p => p.AmountPaid),
                totalPending = payments.Sum(p => p.AmountToPay),
                data = result
            });
        }


        [HttpGet("dashboard/{chitId}")]
        public async Task<IActionResult> GetChitSummary(Guid chitId)
        {
            // 🔍 Get chit group
            var chit = await _context.ChitGroups
                .FirstOrDefaultAsync(c => c.Id == chitId);

            if (chit == null)
                return NotFound("Chit group not found");

            // 📅 Get current month (latest auction month)
            var currentMonth = await _context.Auctions
                .Where(a => a.ChitGroupId == chitId)
                .OrderByDescending(a => a.MonthNumber)
                .Select(a => a.MonthNumber)
                .FirstOrDefaultAsync();

            // 💳 Get payments of current month
            var payments = await _context.Payments
                .Where(p => p.ChitGroupId == chitId && p.MonthNumber == currentMonth)
                .ToListAsync();

            if (!payments.Any())
            {
                return Ok(new
                {
                    chitId,
                    message = "No payments found for current month"
                });
            }

            // 📊 Calculations
            var totalCollection = payments.Sum(p => p.AmountPaid);
            var totalPending = payments.Sum(p => p.AmountToPay);

            var paidMembers = payments.Count(p => p.IsPaid);
            var unpaidMembers = payments.Count(p => !p.IsPaid);

            // 📦 Response
            return Ok(new
            {
                chitId,
                totalMembers = chit.TotalMembers,
                totalAmount = chit.TotalAmount,
                currentMonth,

                totalCollection,
                totalPending,

                paidMembers,
                unpaidMembers
            });
        }




//Pdf 
       [HttpGet("pdf/{chitId}/month/{month}")]
public async Task<IActionResult> GenerateChitPdf(Guid chitId, int month)
{
    try
    {
        var chit = await _context.ChitGroups
            .FirstOrDefaultAsync(c => c.Id == chitId);

        if (chit == null)
            return NotFound("Chit not found");

        var payments = await _context.Payments
            .Where(p => p.ChitGroupId == chitId && p.MonthNumber == month)
            .Include(p => p.Member)
            .OrderBy(p => p.Member.Name)
            .ToListAsync();

        if (!payments.Any())
            return NotFound("No data for this month");

        var pdfBytes = GeneratePdf(chit, payments, month);

        // 🔥 OPTIONAL: Save locally for backup
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, $"Chit_{chitId}_Month_{month}.pdf");
        System.IO.File.WriteAllBytes(filePath, pdfBytes);

        return File(pdfBytes, "application/pdf", $"Chit_{month}.pdf");
    }
    catch (Exception ex)
    {
        return StatusCode(500, new
        {
            message = "Error generating PDF",
            error = ex.InnerException?.Message ?? ex.Message
        });
    }
}


private byte[] GeneratePdf(ChitGroup chit, List<Payment> payments, int month)
{
    var totalPaid = payments.Sum(p => p.AmountPaid);
    var totalPending = payments.Sum(p => p.AmountToPay);

    return Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Margin(20);

            // 🔥 HEADER
            page.Header().Column(col =>
            {
                col.Item().Text($"Chit Report - Month {month}")
                    .FontSize(22).Bold().AlignCenter();

                col.Item().Text($"Generated on: {DateTime.Now:dd-MM-yyyy}")
                    .FontSize(10).AlignRight();
            });

            // 🔥 CONTENT
            page.Content().Column(col =>
            {
                col.Spacing(10);

                // 🔹 Chit Details Box
                col.Item().Border(1).Padding(10).Column(inner =>
                {
                    inner.Item().Text($"Chit Name: {chit.ChitName}").Bold();
                    inner.Item().Text($"Total Amount: ₹ {chit.TotalAmount:N2}");
                    inner.Item().Text($"Total Members: {chit.TotalMembers}");
                    inner.Item().Text($"Month: {month}");
                });

                // 🔹 Table
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // Name
                        columns.RelativeColumn();  // Paid
                        columns.RelativeColumn();  // Remaining
                        columns.RelativeColumn();  // Status
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Border(1).Padding(5).Text("Member").Bold();
                        header.Cell().Border(1).Padding(5).Text("Paid").Bold();
                        header.Cell().Border(1).Padding(5).Text("Remaining").Bold();
                        header.Cell().Border(1).Padding(5).Text("Status").Bold();
                    });

                    // Data
                    foreach (var p in payments)
                    {
                        table.Cell().Border(1).Padding(5).Text(p.Member.Name);
                        table.Cell().Border(1).Padding(5).Text($"₹ {p.AmountPaid:N2}");
                        table.Cell().Border(1).Padding(5).Text($"₹ {p.AmountToPay:N2}");

                        table.Cell().Border(1).Padding(5)
                            .Text(p.IsPaid ? "PAID" : "PENDING")
                            .FontColor(p.IsPaid ? Colors.Green.Medium : Colors.Red.Medium)
                            .Bold();
                    }
                });

                // 🔥 SUMMARY SECTION (VERY IMPORTANT)
                col.Item().Border(1).Padding(10).Column(inner =>
                {
                    inner.Item().Text("Summary").Bold().FontSize(14);

                    inner.Item().Text($"Total Collected: ₹ {totalPaid:N2}");
                    inner.Item().Text($"Total Pending: ₹ {totalPending:N2}");

                    inner.Item().Text($"Paid Members: {payments.Count(p => p.IsPaid)}");
                    inner.Item().Text($"Pending Members: {payments.Count(p => !p.IsPaid)}");
                });
            });

            // 🔥 FOOTER
            page.Footer()
                .AlignCenter()
                .Text("Chit Fund Management System")
                .FontSize(10);
        });
    }).GeneratePdf();
}


[HttpDelete("payments/{chitId}/month/{month}")]
public async Task<IActionResult> DeletePayments(Guid chitId, int month)
{
    var payments = await _context.Payments
        .Where(p => p.ChitGroupId == chitId && p.MonthNumber == month)
        .ToListAsync();

    if (!payments.Any())
        return NotFound("No payments found");

    // ❗ Safety check
    if (payments.Any(p => p.AmountPaid > 0))
    {
        return BadRequest("Cannot delete payments. Some members have already paid.");
    }

    _context.Payments.RemoveRange(payments);
    await _context.SaveChangesAsync();

    return Ok("Payments deleted successfully. You can recreate auction.");
}
    }
}