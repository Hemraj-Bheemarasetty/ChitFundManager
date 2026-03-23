using Microsoft.EntityFrameworkCore;
using ChitFundManager.Models;

namespace ChitFundManager.Data{
public class AppDbContext : DbContext
{
    public DbSet<Member> Members { get; set; }
    public DbSet<ChitGroup> ChitGroups { get; set; }
    public DbSet<ChitMember> ChitMembers { get; set; }
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<Payment> Payments { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
}