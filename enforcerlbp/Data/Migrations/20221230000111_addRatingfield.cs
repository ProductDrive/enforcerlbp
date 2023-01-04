using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class addRatingfield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Ratings",
                table: "Physiotherapists",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "RatingData",
                table: "Physiotherapists",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatingData",
                table: "Physiotherapists");

            migrationBuilder.AlterColumn<decimal>(
                name: "Ratings",
                table: "Physiotherapists",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}
