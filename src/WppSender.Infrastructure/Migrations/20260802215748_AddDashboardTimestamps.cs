using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WppSender.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "criado_em",
                table: "leads",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "atualizado_em",
                table: "campanha_envios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_leads_criado_em",
                table: "leads",
                column: "criado_em");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_leads_criado_em",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "criado_em",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "atualizado_em",
                table: "campanha_envios");
        }
    }
}
