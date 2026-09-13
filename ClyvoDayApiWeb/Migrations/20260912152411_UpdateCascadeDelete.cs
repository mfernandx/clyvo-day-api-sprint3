using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClyvoDayApiWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_COMMUNITY_POSTS_USERS_UserId",
                table: "COMMUNITY_POSTS");

            migrationBuilder.AddForeignKey(
                name: "FK_COMMUNITY_POSTS_USERS_UserId",
                table: "COMMUNITY_POSTS",
                column: "UserId",
                principalTable: "USERS",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_COMMUNITY_POSTS_USERS_UserId",
                table: "COMMUNITY_POSTS");

            migrationBuilder.AddForeignKey(
                name: "FK_COMMUNITY_POSTS_USERS_UserId",
                table: "COMMUNITY_POSTS",
                column: "UserId",
                principalTable: "USERS",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
