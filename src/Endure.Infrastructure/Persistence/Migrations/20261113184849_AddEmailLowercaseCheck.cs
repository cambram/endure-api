using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Endure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailLowercaseCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_AppUsers_Email_Lowercase",
                table: "AppUsers",
                sql: "\"Email\" = lower(\"Email\")");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_AppUsers_Email_Lowercase",
                table: "AppUsers");
        }
    }
}
