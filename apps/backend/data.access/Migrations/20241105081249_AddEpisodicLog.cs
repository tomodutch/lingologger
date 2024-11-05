using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace data.access.Migrations
{
    /// <inheritdoc />
    public partial class AddEpisodicLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EpisodesWatched",
                table: "Logs",
                newName: "Episodes");

            migrationBuilder.RenameColumn(
                name: "EpisodeLengthInMinutes",
                table: "Logs",
                newName: "EpisodeLengthInSeconds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Episodes",
                table: "Logs",
                newName: "EpisodesWatched");

            migrationBuilder.RenameColumn(
                name: "EpisodeLengthInSeconds",
                table: "Logs",
                newName: "EpisodeLengthInMinutes");
        }
    }
}
