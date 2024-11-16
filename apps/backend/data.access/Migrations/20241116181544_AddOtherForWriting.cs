using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace data.access.Migrations
{
    /// <inheritdoc />
    public partial class AddOtherForWriting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "MediumId",
                table: "Logs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Media",
                columns: new[] { "Id", "DeletedAt", "GuildId", "LogType", "Name" },
                values: new object[] { new Guid("ca56bf69-0f46-44a5-a052-cd878ebdf757"), null, null, "Writing", "Other" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Media",
                keyColumn: "Id",
                keyValue: new Guid("ca56bf69-0f46-44a5-a052-cd878ebdf757"));

            migrationBuilder.AlterColumn<Guid>(
                name: "MediumId",
                table: "Logs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }
    }
}
