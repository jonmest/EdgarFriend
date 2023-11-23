using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EdgarFriend.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FundamentalEntries",
                columns: table => new
                {
                    Cik = table.Column<string>(type: "text", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    Frame = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    Val = table.Column<double>(type: "double precision", nullable: false),
                    Accn = table.Column<string>(type: "text", nullable: true),
                    Form = table.Column<string>(type: "text", nullable: false),
                    Fy = table.Column<int>(type: "integer", nullable: true),
                    Fp = table.Column<string>(type: "text", nullable: false),
                    Filed = table.Column<string>(type: "text", nullable: false),
                    Start = table.Column<DateOnly>(type: "date", nullable: true),
                    End = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundamentalEntries", x => new { x.Cik, x.Frame, x.Label });
                });

            migrationBuilder.CreateTable(
                name: "SymbolMappings",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Cik = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SymbolMappings", x => x.Symbol);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FundamentalEntries_Cik_PeriodType_Label_Symbol",
                table: "FundamentalEntries",
                columns: new[] { "Cik", "PeriodType", "Label", "Symbol" });

            migrationBuilder.CreateIndex(
                name: "IX_SymbolMappings_Symbol_Cik",
                table: "SymbolMappings",
                columns: new[] { "Symbol", "Cik" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FundamentalEntries");

            migrationBuilder.DropTable(
                name: "SymbolMappings");
        }
    }
}
