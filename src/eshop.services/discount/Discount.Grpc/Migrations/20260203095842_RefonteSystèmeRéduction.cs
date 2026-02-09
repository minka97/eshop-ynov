using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discount.Grpc.Migrations
{
    /// <inheritdoc />
    public partial class RefonteSystèmeRéduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "Coupon",
                type: "REAL",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Coupon",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCumulative",
                table: "Coupon",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "MaxCumulativePercentage",
                table: "Coupon",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxRedemptions",
                table: "Coupon",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Percentage",
                table: "Coupon",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Coupon",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Coupon",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Code", "IsCumulative", "MaxCumulativePercentage", "MaxRedemptions", "Percentage", "Type" },
                values: new object[] { null, false, null, null, null, 0 });

            migrationBuilder.UpdateData(
                table: "Coupon",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Code", "IsCumulative", "MaxCumulativePercentage", "MaxRedemptions", "Percentage", "Type" },
                values: new object[] { null, false, null, null, null, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "IsCumulative",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "MaxCumulativePercentage",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "MaxRedemptions",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Coupon");

            migrationBuilder.AlterColumn<double>(
                name: "Amount",
                table: "Coupon",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "REAL",
                oldNullable: true);
        }
    }
}
