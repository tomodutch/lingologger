using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace data.access.Migrations
{
    /// <inheritdoc />
    public partial class SetupLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'"),
                    Medium = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AmountOfSeconds = table.Column<int>(type: "integer", maxLength: 86400, nullable: false),
                    Coefficient = table.Column<double>(type: "double precision", maxLength: 86400, nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LogType = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    CharactersRead = table.Column<int>(type: "integer", nullable: true),
                    EpisodesWatched = table.Column<int>(type: "integer", nullable: true),
                    EpisodeLengthInMinutes = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => new { x.UserId, x.CreatedAt });
                    table.ForeignKey(
                        name: "FK_Logs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
