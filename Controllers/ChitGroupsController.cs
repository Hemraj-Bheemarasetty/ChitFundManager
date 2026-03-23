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