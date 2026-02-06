using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Infra.Migrations
{
    /// <inheritdoc />
    public partial class InitDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "file",
                columns: table => new
                {
                    uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false, comment: "Publicly accessible URL"),
                    type = table.Column<int>(type: "integer", nullable: false, comment: "Storage visibility type (Public, Private, etc.)"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Storage-aliased filename"),
                    origin_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Original filename from client"),
                    extension = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "File extension or MimeType"),
                    size = table.Column<long>(type: "bigint", nullable: true, comment: "File size in bytes"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file", x => x.uuid);
                },
                comment: "Metadata table for uploaded files (S3, etc.)");

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    identify = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "User login login id (Unique)"),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Hashed password"),
                    role = table.Column<int>(type: "integer", nullable: false, comment: "User access level (Admin, SubAdmin, User)"),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "The last time the user logged in"),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Admin UUID who created this user"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.uuid);
                    table.ForeignKey(
                        name: "fk_user_user_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "user",
                        principalColumn: "uuid");
                },
                comment: "User Table for trading system");

            migrationBuilder.CreateTable(
                name: "permission",
                columns: table => new
                {
                    uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    permission = table.Column<int>(type: "integer", nullable: false, comment: "The specific permission type (e.g., Trade_Read, Admin_Write)"),
                    target_is_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Target user who possesses this permission"),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Manager/Admin who granted this permission"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permission", x => x.uuid);
                    table.ForeignKey(
                        name: "fk_permission_user_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "user",
                        principalColumn: "uuid");
                    table.ForeignKey(
                        name: "fk_permission_user_target_is_id",
                        column: x => x.target_is_id,
                        principalTable: "user",
                        principalColumn: "uuid");
                },
                comment: "Fine-grained permission management table");

            migrationBuilder.CreateIndex(
                name: "ix_permission_created_by_id",
                table: "permission",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_permission_target_is_id_permission",
                table: "permission",
                columns: new[] { "target_is_id", "permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_created_by_id",
                table: "user",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_identify",
                table: "user",
                column: "identify",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "file");

            migrationBuilder.DropTable(
                name: "permission");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
