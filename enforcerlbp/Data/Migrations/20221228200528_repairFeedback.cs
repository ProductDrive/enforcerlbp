using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class repairFeedback : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "FeedbackReplies");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "Feedbacks",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "ReceiverId",
                table: "Feedbacks",
                newName: "ExercisePrescriptionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "Feedbacks",
                newName: "SenderId");

            migrationBuilder.RenameColumn(
                name: "ExercisePrescriptionId",
                table: "Feedbacks",
                newName: "ReceiverId");

            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "FeedbackReplies",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
