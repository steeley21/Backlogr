using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backlogr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewLikesAndComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewComment",
                columns: table => new
                {
                    ReviewCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewComment", x => x.ReviewCommentId);
                    table.ForeignKey(
                        name: "FK_ReviewComment_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReviewComment_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "ReviewId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewLike",
                columns: table => new
                {
                    ReviewLikeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewLike", x => x.ReviewLikeId);
                    table.ForeignKey(
                        name: "FK_ReviewLike_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReviewLike_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "ReviewId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewComment_ReviewId",
                table: "ReviewComment",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewComment_UserId",
                table: "ReviewComment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLike_ReviewId",
                table: "ReviewLike",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLike_UserId_ReviewId",
                table: "ReviewLike",
                columns: new[] { "UserId", "ReviewId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewComment");

            migrationBuilder.DropTable(
                name: "ReviewLike");
        }
    }
}
