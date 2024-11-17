using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace data.access.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMedium : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logs_Media_MediumId",
                table: "Logs");

            migrationBuilder.DropTable(
                name: "Media");

            migrationBuilder.DropIndex(
                name: "IX_Logs_MediumId",
                table: "Logs");

            migrationBuilder.DropIndex(
                name: "IX_Logs_UserId_MediumId",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "MediumId",
                table: "Logs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MediumId",
                table: "Logs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'"),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    GuildId = table.Column<long>(type: "bigint", nullable: true),
                    LogType = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Media",
                columns: new[] { "Id", "DeletedAt", "GuildId", "LogType", "Name" },
                values: new object[,]
                {
                    { new Guid("21daae4a-3dfc-4f8b-b525-0a513376de1a"), null, null, "Watchable", "Youtube" },
                    { new Guid("46906a9f-1062-45fc-b5cd-8b30fa97062a"), null, null, "Readable", "Visual Novel" },
                    { new Guid("4bce7a21-3b1d-4ffd-a7eb-ea50724bf629"), null, null, "Other", "Other" },
                    { new Guid("676fb136-e379-4c6a-ad6a-b4aaad2e413e"), null, null, "Watchable", "Anime" },
                    { new Guid("8b51cd17-c2f0-4b4b-99ee-63c99924d107"), null, null, "Audible", "Podcast" },
                    { new Guid("bb2adabb-1271-468f-af03-b8457e3e6488"), null, null, "Anki", "Anki" },
                    { new Guid("c732a6f7-ccc6-44ad-88aa-e1da57e16c4a"), null, null, "Readable", "Book" },
                    { new Guid("ca56bf69-0f46-44a5-a052-cd878ebdf757"), null, null, "Writing", "Other" },
                    { new Guid("e11ca2ad-9903-4926-a21e-6640fac79089"), null, null, "Audible", "Audiobook" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Logs_MediumId",
                table: "Logs",
                column: "MediumId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_UserId_MediumId",
                table: "Logs",
                columns: new[] { "UserId", "MediumId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Logs_Media_MediumId",
                table: "Logs",
                column: "MediumId",
                principalTable: "Media",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
