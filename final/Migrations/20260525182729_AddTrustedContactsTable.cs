using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace final.Migrations
{
    /// <inheritdoc />
    public partial class AddTrustedContactsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrustedContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TrustedUserId = table.Column<int>(type: "int", nullable: false),
                    NickName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrustedPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DailyLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsedToday = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsedTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustedContacts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrustedContacts");
        }
    }
}
