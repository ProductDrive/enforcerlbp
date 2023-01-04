using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class repairFeedbackreply : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnersName",
                table: "FeedbackReplies",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnersName",
                table: "FeedbackReplies");
        }
    }
}
