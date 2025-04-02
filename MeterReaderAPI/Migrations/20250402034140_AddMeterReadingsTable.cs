using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeterReaderAPI.Migrations {
    /// <inheritdoc />
    public partial class AddMeterReadingsTable : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "MeterReading",
                columns: table => new {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    MeterReadingDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MeterReadingValue = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_MeterReading", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeterReading_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeterReading_AccountId_MeterReadingDateTime_MeterReadingVal~",
                table: "MeterReading",
                columns: new[] { "AccountId", "MeterReadingDateTime", "MeterReadingValue" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "MeterReading");
        }
    }
}
