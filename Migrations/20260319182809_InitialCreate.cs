using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChitFundManager.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChitGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChitName = table.Column<string>(type: "text", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalMembers = table.Column<int>(type: "integer", nullable: false),
                    DurationMonths = table.Column<int>(type: "integer", nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChitGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Auctions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChitGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    MonthNumber = table.Column<int>(type: "integer", nullable: false),
                    WinnerMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    BidAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    WinnerAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalCollection = table.Column<decimal>(type: "numeric", nullable: false),
                    MonthlyPayablePerMember = table.Column<decimal>(type: "numeric", nullable: false),
                    AuctionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auctions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Auctions_ChitGroups_ChitGroupId",
                        column: x => x.ChitGroupId,
                        principalTable: "ChitGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Auctions_Members_WinnerMemberId",
                        column: x => x.WinnerMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChitMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChitGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    HasWon = table.Column<bool>(type: "boolean", nullable: false),
                    WinningMonth = table.Column<int>(type: "integer", nullable: true),
                    WinningAmount = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChitMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChitMembers_ChitGroups_ChitGroupId",
                        column: x => x.ChitGroupId,
                        principalTable: "ChitGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChitMembers_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChitGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    MonthNumber = table.Column<int>(type: "integer", nullable: false),
                    AmountToPay = table.Column<decimal>(type: "numeric", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_ChitGroups_ChitGroupId",
                        column: x => x.ChitGroupId,
                        principalTable: "ChitGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_ChitGroupId",
                table: "Auctions",
                column: "ChitGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_WinnerMemberId",
                table: "Auctions",
                column: "WinnerMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ChitMembers_ChitGroupId",
                table: "ChitMembers",
                column: "ChitGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ChitMembers_MemberId",
                table: "ChitMembers",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ChitGroupId",
                table: "Payments",
                column: "ChitGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MemberId",
                table: "Payments",
                column: "MemberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auctions");

            migrationBuilder.DropTable(
                name: "ChitMembers");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ChitGroups");

            migrationBuilder.DropTable(
                name: "Members");
        }
    }
}
