using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ChitFundManager.Data;
using AutoMapper;
using ChitFundManager.Models;
namespace ChitFundManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembersController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public MembersController(IMapper mapper, AppDbContext context)
        {
            _mapper=mapper;
            _context=context;
        }


        [HttpPost("AddMember")]
        public async Task<IActionResult> createMember(CreateMemberDto dto)
        {
            var model=_mapper.Map<Member>(dto);
            model.Id=Guid.NewGuid();
            await _context.Members.AddAsync(model);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                data=model,
                message="Member Created"
            });
        }

        [HttpGet("getAllmembers")]
        public async Task<IActionResult> GetAllMembers()
        {
            var members = await _context.Members.ToListAsync();

            return Ok(new
            {
                data = members
            });
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMemberById(Guid id)
        {
            var member = await _context.Members.FindAsync(id);
        
            if (member == null)
                return NotFound("Member not found");
        
            return Ok(new
            {
                data = member
            });
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateMember(Guid id,[FromForm] UpdateMemberDto dto)
        {
            var member = await _context.Members.FindAsync(id);

            if (member == null)
                return NotFound("Member not found");

            if (dto.Name != null)
                member.Name = dto.Name;

            if (dto.PhoneNumber != null)
                member.PhoneNumber = dto.PhoneNumber;

            if (dto.Address != null)
                member.Address = dto.Address;

            var phoneExists = await _context.Members
                            .AnyAsync(x => x.PhoneNumber == dto.PhoneNumber && x.Id != id);

            if (phoneExists)
                 return BadRequest("Phone number already exists");

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Member updated successfully",
                data = member
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(Guid id)
        {
            var member = await _context.Members.FindAsync(id);
        
            if (member == null)
                return NotFound("Member not found");
        
            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
        
            return Ok(new
            {
                message = "Member deleted successfully"
            });
        }
    }
}