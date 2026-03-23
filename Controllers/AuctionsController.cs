using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ChitFundManager.Data;
using AutoMapper;
using ChitFundManager.Models;
namespace AuctionController.Controllers
{
    public class AuctionController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public AuctionController(IMapper mapper, AppDbContext context)
        {
            _mapper=mapper;
            _context=context;
        }

        [HttpPost("CreateAuction")]
        public async Task<IActionResult> CreateAuction(CreateAuctionDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                var chitGroup = await _context.ChitGroups
                    .FirstOrDefaultAsync(x => x.Id == dto.ChitGroupId);

                if (chitGroup == null)
                    return NotFound("Chit group not found");


                var winnerMember = await _context.ChitMembers
                    .FirstOrDefaultAsync(x => x.MemberId == dto.WinnerMemberId);

                if (winnerMember == null || winnerMember.ChitGroupId != dto.ChitGroupId)
                    return BadRequest("Invalid winner member");

                if (winnerMember.HasWon)
                    return BadRequest("This member has already won");


                var lastMonth = await _context.Auctions
                    .Where(a => a.ChitGroupId == dto.ChitGroupId)
                    .OrderByDescending(a => a.MonthNumber)
                    .Select(a => a.MonthNumber)
                    .FirstOrDefaultAsync();

                var nextMonth = lastMonth + 1;

                if (nextMonth > chitGroup.TotalMembers)
                    return BadRequest("All months completed");


                var auctionExists = await _context.Auctions
                    .AnyAsync(a => a.ChitGroupId == dto.ChitGroupId && a.MonthNumber == nextMonth);

                if (auctionExists)
                    return BadRequest("Auction already created for this month");

                // 💰 Calculations
                var commissionAmt = (chitGroup.TotalAmount * chitGroup.CommissionPercent) / 100;

                var distributableAmount = chitGroup.TotalAmount - dto.BidAmount + commissionAmt;

                var perPersonAmt = Math.Round(distributableAmount / chitGroup.TotalMembers, 2);

                // 🧾 Create auction
                var model = _mapper.Map<Auction>(dto);
                model.Id = Guid.NewGuid();
                model.MonthNumber = nextMonth;
                model.CommissionAmount = commissionAmt;
                model.MonthlyPayablePerMember = perPersonAmt;
                model.AuctionDate = DateTime.UtcNow;

                // ✅ FIXED
                model.WinnerAmount = chitGroup.TotalAmount - dto.BidAmount;

                // ✅ FIXED
                model.TotalCollection = chitGroup.TotalAmount - dto.BidAmount + commissionAmt;

                // Winner update
                winnerMember.WinningAmount = model.WinnerAmount;
                winnerMember.WinningMonth = nextMonth;
                winnerMember.HasWon = true;


                var members = await _context.ChitMembers
                    .Where(m => m.ChitGroupId == dto.ChitGroupId)
                    .ToListAsync();

                if (members.Count == 0)
                    return BadRequest("No members found in this chit group");


                var paymentsExist = await _context.Payments
                    .AnyAsync(p => p.ChitGroupId == dto.ChitGroupId && p.MonthNumber == nextMonth);

                if (paymentsExist)
                    return BadRequest("Payments already created for this month");


                foreach (var member in members)
                {
                    var payment = new Payment
                    {
                        Id = Guid.NewGuid(),
                        ChitGroupId = dto.ChitGroupId,
                        MemberId = member.MemberId,
                        MonthNumber = nextMonth,

                        AmountToPay = perPersonAmt,
                        AmountPaid = 0,

                        IsPaid = false,
                        PaidDate = null
                    };

                    await _context.Payments.AddAsync(payment);
                }


                await _context.Auctions.AddAsync(model);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Auction created & payments generated successfully",
                    month = nextMonth,
                    perMemberAmount = perPersonAmt,
                    winnerAmount = model.WinnerAmount
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message = "Something went wrong",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpGet("{chitId}/month/{monthNumber}")]
        public async Task<IActionResult> GetAuctionByMonth(Guid chitId, int monthNumber)
        {
            var auction = await _context.Auctions
                                .Include(a => a.WinnerMember)
                                .FirstOrDefaultAsync(a => 
                                    a.ChitGroupId == chitId && 
                                    a.MonthNumber == monthNumber);
        
            if (auction == null)
            {
                return NotFound("Auction not found for given month");
            }
        
            var dto= _mapper.Map<AuctionResponseDto>(auction);
            return Ok(dto);
        }

        [HttpGet("{chitId}")]
        public async Task<IActionResult> GetAllAuctionsByChit(Guid chitId)
        {
            var auctions = await _context.Auctions
                                    .Where(a => a.ChitGroupId == chitId)
                                    .Include(a => a.WinnerMember)
                                    .OrderBy(a => a.MonthNumber) // 🔥 important
                                    .ToListAsync();

            
            if (auctions == null || !auctions.Any())
            {
                return NotFound("No auctions found for this chit group");
            }

            var dto= _mapper.Map<List<AuctionResponseDto>>(auctions);

            return Ok(dto);
        }
        


        [HttpGet("dashboard/messages")]
        public async Task<IActionResult> GetMessages()
        {
            var payments = await _context.Payments
                .Where(p => !p.IsPaid)
                .Include(p => p.Member)
                .Include(p => p.ChitGroup)
                .ToListAsync();

            if (!payments.Any())
                return Ok("No pending payments");

            var result = payments
                .GroupBy(p => p.Member.PhoneNumber)
                .Select(g =>
                {
                    var name = g.First().Member.Name.Split(" ")[0];

                    decimal grandTotal = 0;

                    var message = $"Hello {name} garu,\n\nChit Payment Summary (Latest Month):\n\n";

                    // 🔥 Group by ChitGroup
                    var chitGroups = g.GroupBy(x => x.ChitGroup.ChitName);

                    foreach (var chit in chitGroups)
                    {
                        // 👉 Get latest month only
                        var latestMonth = chit.Max(x => x.MonthNumber);

                        var latestPayments = chit
                            .Where(x => x.MonthNumber == latestMonth)
                            .ToList();

                        var entries = latestPayments.Count;
                        var perEntry = latestPayments.First().AmountToPay + latestPayments.First().AmountPaid;
                        var total = latestPayments.Sum(x => x.AmountToPay);
                        var loss = (perEntry * entries) - total;

                        grandTotal += total;

                        message += $"🔹 {chit.Key}\n";
                        message += $"Month {latestMonth}: ₹{perEntry:N0} × {entries} (Loss ₹{loss:N0}) → ₹{total:N0}\n\n";
                    }

                    message += $"Total Amount to Pay: ₹{grandTotal:N0}\n\n";
                    message += $"Please pay the amount. Thank you.";

                    return new
                    {
                        Phone = g.Key,
                        Message = message
                    };
                })
                .ToList();

            return Ok(result);
        }

        [HttpPut("auction/full-update/{auctionId}")]
public async Task<IActionResult> FullUpdateAuction(Guid auctionId, Guid winnerMemberId, decimal bidAmount)
{
    using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        // 🔍 Get auction
        var auction = await _context.Auctions
            .FirstOrDefaultAsync(a => a.Id == auctionId);

        if (auction == null)
            return NotFound("Auction not found");

        var chitGroup = await _context.ChitGroups
            .FirstOrDefaultAsync(x => x.Id == auction.ChitGroupId);

        if (chitGroup == null)
            return NotFound("Chit group not found");

        int month = auction.MonthNumber;

        // 🔍 Get winner (same as CREATE)
        var winnerMember = await _context.ChitMembers
            .FirstOrDefaultAsync(x => x.MemberId == winnerMemberId && x.ChitGroupId == chitGroup.Id);

        if (winnerMember == null)
            return BadRequest("Invalid winner member");

        // ❗ Check if someone already paid
        var payments = await _context.Payments
            .Where(p => p.ChitGroupId == chitGroup.Id && p.MonthNumber == month)
            .ToListAsync();

        if (payments.Any(p => p.AmountPaid > 0))
            return BadRequest("Cannot update. Payments already started.");

        // 🗑 Delete old payments
        if (payments.Any())
            _context.Payments.RemoveRange(payments);

        // ❗ Reset OLD winner (VERY IMPORTANT)
        var oldWinner = await _context.ChitMembers
            .FirstOrDefaultAsync(x => x.WinningMonth == month && x.ChitGroupId == chitGroup.Id);

        if (oldWinner != null)
        {
            oldWinner.HasWon = false;
            oldWinner.WinningAmount = null;
            oldWinner.WinningMonth = null;
        }

        // 💰 SAME CALCULATIONS AS CREATE
        var commissionAmt = (chitGroup.TotalAmount * chitGroup.CommissionPercent) / 100;

        var distributableAmount = chitGroup.TotalAmount - bidAmount + commissionAmt;

        var perPersonAmt = Math.Round(distributableAmount / chitGroup.TotalMembers, 2);

        // 🔄 UPDATE AUCTION (FULL RESET)
        auction.WinnerMemberId = winnerMemberId;
        auction.BidAmount = bidAmount;
        auction.CommissionAmount = commissionAmt;
        auction.MonthlyPayablePerMember = perPersonAmt;
        auction.AuctionDate = DateTime.UtcNow;

        auction.WinnerAmount = chitGroup.TotalAmount - bidAmount;
        auction.TotalCollection = chitGroup.TotalAmount - bidAmount + commissionAmt;

        // 🏆 Set NEW winner
        winnerMember.HasWon = true;
        winnerMember.WinningAmount = auction.WinnerAmount;
        winnerMember.WinningMonth = month;

        // 👥 Get members
        var members = await _context.ChitMembers
            .Where(m => m.ChitGroupId == chitGroup.Id)
            .ToListAsync();

        // 💳 Recreate payments
        foreach (var member in members)
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                ChitGroupId = chitGroup.Id,
                MemberId = member.MemberId,
                MonthNumber = month,
                AmountToPay = perPersonAmt,
                AmountPaid = 0,
                IsPaid = false,
                PaidDate = null
            };

            await _context.Payments.AddAsync(payment);
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            message = "Auction fully updated successfully",
            month,
            newWinner = winnerMemberId,
            perMemberAmount = perPersonAmt
        });
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();

        return StatusCode(500, new
        {
            message = "Error updating auction",
            error = ex.InnerException?.Message ?? ex.Message
        });
    }
}
    }
}