using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backlogr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFollows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReviewComment_AspNetUsers_UserId",
                table: "ReviewComment");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewComment_Reviews_ReviewId",
                table: "ReviewComment");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewLike_AspNetUsers_UserId",
                table: "ReviewLike");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewLike_Reviews_ReviewId",
                table: "ReviewLike");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReviewLike",
                table: "ReviewLike");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReviewComment",
                table: "ReviewComment");

            migrationBuilder.RenameTable(
                name: "ReviewLike",
                newName: "ReviewLikes");

            migrationBuilder.RenameTable(
                name: "ReviewComment",
                newName: "ReviewComments");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewLike_UserId_ReviewId",
                table: "ReviewLikes",
                newName: "IX_ReviewLikes_UserId_ReviewId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewLike_ReviewId",
                table: "ReviewLikes",
                newName: "IX_ReviewLikes_ReviewId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewComment_UserId",
                table: "ReviewComments",
                newName: "IX_ReviewComments_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewComment_ReviewId",
                table: "ReviewComments",
                newName: "IX_ReviewComments_ReviewId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewLikes",
                table: "ReviewLikes",
                column: "ReviewLikeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewComments",
                table: "ReviewComments",
                column: "ReviewCommentId");

            migrationBuilder.CreateTable(
                name: "Follows",
                columns: table => new
                {
                    FollowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FollowerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FollowingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Follows", x => x.FollowId);
                    table.ForeignKey(
                        name: "FK_Follows_AspNetUsers_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Follows_AspNetUsers_FollowingId",
                        column: x => x.FollowingId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowerId_FollowingId",
                table: "Follows",
                columns: new[] { "FollowerId", "FollowingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowingId",
                table: "Follows",
                column: "FollowingId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewComments_AspNetUsers_UserId",
                table: "ReviewComments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewComments_Reviews_ReviewId",
                table: "ReviewComments",
                column: "ReviewId",
                principalTable: "Reviews",
                principalColumn: "ReviewId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewLikes_AspNetUsers_UserId",
                table: "ReviewLikes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewLikes_Reviews_ReviewId",
                table: "ReviewLikes",
                column: "ReviewId",
                principalTable: "Reviews",
                principalColumn: "ReviewId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReviewComments_AspNetUsers_UserId",
                table: "ReviewComments");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewComments_Reviews_ReviewId",
                table: "ReviewComments");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewLikes_AspNetUsers_UserId",
                table: "ReviewLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewLikes_Reviews_ReviewId",
                table: "ReviewLikes");

            migrationBuilder.DropTable(
                name: "Follows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReviewLikes",
                table: "ReviewLikes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReviewComments",
                table: "ReviewComments");

            migrationBuilder.RenameTable(
                name: "ReviewLikes",
                newName: "ReviewLike");

            migrationBuilder.RenameTable(
                name: "ReviewComments",
                newName: "ReviewComment");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewLikes_UserId_ReviewId",
                table: "ReviewLike",
                newName: "IX_ReviewLike_UserId_ReviewId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewLikes_ReviewId",
                table: "ReviewLike",
                newName: "IX_ReviewLike_ReviewId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewComments_UserId",
                table: "ReviewComment",
                newName: "IX_ReviewComment_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewComments_ReviewId",
                table: "ReviewComment",
                newName: "IX_ReviewComment_ReviewId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewLike",
                table: "ReviewLike",
                column: "ReviewLikeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewComment",
                table: "ReviewComment",
                column: "ReviewCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewComment_AspNetUsers_UserId",
                table: "ReviewComment",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewComment_Reviews_ReviewId",
                table: "ReviewComment",
                column: "ReviewId",
                principalTable: "Reviews",
                principalColumn: "ReviewId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewLike_AspNetUsers_UserId",
                table: "ReviewLike",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewLike_Reviews_ReviewId",
                table: "ReviewLike",
                column: "ReviewId",
                principalTable: "Reviews",
                principalColumn: "ReviewId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
