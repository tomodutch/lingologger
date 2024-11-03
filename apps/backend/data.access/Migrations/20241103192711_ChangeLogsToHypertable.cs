using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace data.access.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLogsToHypertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SELECT create_hypertable('timescaledb.tablename', 'time', 'params', 4, if_not_exists => TRUE, chunk_time_interval => interval '1 day');
            migrationBuilder.Sql("SELECT create_hypertable('\"Logs\"', by_range('CreatedAt', INTERVAL '1 day'))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
