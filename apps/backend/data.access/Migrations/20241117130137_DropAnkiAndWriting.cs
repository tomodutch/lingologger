using LingoLogger.Data.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace data.access.Migrations
{
    /// <inheritdoc />
    public partial class DropAnkiAndWriting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Logs""
                SET ""LogType"" = 'Other'
                Where ""LogType"" = 'Writing' OR ""LogType"" = 'Anki'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
