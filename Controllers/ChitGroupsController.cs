using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ChitFundManager.Data;
using AutoMapper;
// using ChitFundManager.DTO;
using ChitFundManager.Models;
namespace ChitFundManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChitGroupsContoller : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public ChitGroupsContoller(IMapper mapper, AppDbContext context)
        {
            _mapper=mapper;
            _context=context;
        }


        [HttpPost("chitGroup")]
        public async Task<IActionResult> addChitGroup(ChitGroupDto dto)
        {
            var model= _mapper.Map<ChitGroup>(dto);
            model.Id=Guid.NewGuid();
            model.IsActive=true;
            model.StartDate=DateTime.UtcNow;
            await _context.ChitGroups.AddAsync(model);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message="Chit Group Created successfully",
                data=model
            });
        } 


        [HttpGet("GetAll")]
        public async Task<IActionResult> getAllChits()
        {
            var models=await _context.ChitGroups.ToListAsync();

            var dto= _mapper.Map<List<ChitGroupGetDto>>(models);

            return Ok(new
            {
                data=dto
            });
        }

       [HttpGet("GetById")]
        public async Task<IActionResult> getByID(Guid Id)
        {
            var model = await _context.ChitGroups
                        .Include(x => x.Members)
                        .ThenInclude(cm => cm.Member)
                        .FirstOrDefaultAsync(x => x.Id == Id);

            if (model == null)
                return NotFound("Chit group not found");

            var dto = _mapper.Map<ChitGroupGetDto>(model);

            return Ok(new
            {
                data = dto
            });
        }

        [HttpPatch("{Id}")]
        public async Task<IActionResult> updateChitGroup(Guid Id,[FromForm] ChitGroupUpdateDto dto)
        {
            var model=await _context.ChitGroups.FindAsync(Id);

            if (dto == null)
        return BadRequest("Invalid request body");

            if (model == null)
                return NotFound();

            if (dto.ChitName != null)
                model.ChitName = dto.ChitName;

            model.TotalAmount = dto.TotalAmount ?? model.TotalAmount;
            model.TotalMembers = dto.TotalMembers ?? model.TotalMembers;
            model.DurationMonths = dto.DurationMonths ?? model.DurationMonths;
            model.CommissionPercent = dto.CommissionPercent ?? model.CommissionPercent;
            model.StartDate = dto.StartDate ?? model.StartDate;
            model.IsActive = dto.IsActive ?? model.IsActive;

            
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<ChitGroupGetDto>(model));
                }

    

            [HttpPatch("{id}/close")]
            public async Task<IActionResult> CloseChit(Guid id)
            {
                var chitGroup = await _context.ChitGroups.FindAsync(id);

                if (chitGroup == null)
                    return NotFound("Chit group not found");

                if (!chitGroup.IsActive)
                    return BadRequest("Chit group is already closed");

                chitGroup.IsActive = false;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Chit group closed successfully"
                });
            }

            [HttpPost("{chitGroupId}/members/{memberId}")]
            public async Task<IActionResult> AddMemberToChit(Guid chitGroupId, Guid memberId)
            {
                var chitGroup = await _context.ChitGroups.FindAsync(chitGroupId);
                if (chitGroup == null)
                    return NotFound("Chit group not found");

                var member = await _context.Members.FindAsync(memberId);
                if (member == null)
                    return NotFound("Member not found");

                var exists = await _context.ChitMembers
                    .AnyAsync(x => x.MemberId == memberId && x.ChitGroupId == chitGroupId);

                if (exists)
                    return BadRequest("Member already in chit group");

                var chitMember = new ChitMember
                {
                    Id = Guid.NewGuid(),
                    MemberId = memberId,
                    ChitGroupId = chitGroupId
                };

                await _context.ChitMembers.AddAsync(chitMember);
                await _context.SaveChangesAsync();

                return Ok("Member added to chit");
            }

            [HttpGet("{chitGroupId}/members")]
            public async Task<IActionResult> GetMembersOfChit(Guid chitGroupId)
            {
                var members = await _context.ChitMembers
                    .Where(cm => cm.ChitGroupId == chitGroupId)
                    .Select(cm => new CreateMemberDto
                    {
                        MemberId = cm.MemberId,
                        Name = cm.Member.Name,
                        PhoneNumber = cm.Member.PhoneNumber,
                        Address = cm.Member.Address
                    })
                    .ToListAsync();

                return Ok(new
                {
                    data = members
                });
            }

            [HttpDelete("{id}")]
public async Task<IActionResult> DeleteChitGroup(Guid id)
{
    using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        var chitGroup = await _context.ChitGroups
            .FirstOrDefaultAsync(x => x.Id == id);

        if (chitGroup == null)
            return NotFound("Chit group not found");

        // Check if auctions exist
        var auctionsExist = await _context.Auctions
            .AnyAsync(a => a.ChitGroupId == id);

        if (auctionsExist)
            return BadRequest("Cannot delete chit group. Auctions already exist.");

        // Remove members from chit
        var chitMembers = await _context.ChitMembers
            .Where(cm => cm.ChitGroupId == id)
            .ToListAsync();

        if (chitMembers.Any())
            _context.ChitMembers.RemoveRange(chitMembers);

        var payments = await _context.Payments
    .Where(p => p.ChitGroupId == id)
    .ToListAsync();

var auctions = await _context.Auctions
    .Where(a => a.ChitGroupId == id)
    .ToListAsync();

_context.Payments.RemoveRange(payments);
_context.Auctions.RemoveRange(auctions);
_context.ChitMembers.RemoveRange(chitMembers);
_context.ChitGroups.Remove(chitGroup);
        // Delete chit group
        _context.ChitGroups.Remove(chitGroup);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            message = "Chit group deleted successfully"
        });
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();

        return StatusCode(500, new
        {
            message = "Error deleting chit group",
            error = ex.InnerException?.Message ?? ex.Message
        });
    }
}

            [HttpDelete("{chitGroupId}/members/{memberId}")]
            public async Task<IActionResult> RemoveMemberFromChit(Guid chitGroupId, Guid memberId)
            {
                var chitMember = await _context.ChitMembers
                    .FirstOrDefaultAsync(x => x.ChitGroupId == chitGroupId && x.MemberId == memberId);
            
                if (chitMember == null)
                    return NotFound("Member not found in this chit group");
            
                _context.ChitMembers.Remove(chitMember);
                await _context.SaveChangesAsync();
            
                return Ok(new
                {
                    message = "Member removed from chit group successfully"
                });
            }

}
}