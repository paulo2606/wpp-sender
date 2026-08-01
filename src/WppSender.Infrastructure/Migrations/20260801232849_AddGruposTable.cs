using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WppSender.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGruposTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "grupo_id",
                table: "leads",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "grupos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grupos", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_leads_grupo_id",
                table: "leads",
                column: "grupo_id");

            migrationBuilder.AddForeignKey(
                name: "fk_leads_grupos_grupo_id",
                table: "leads",
                column: "grupo_id",
                principalTable: "grupos",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_leads_grupos_grupo_id",
                table: "leads");

            migrationBuilder.DropTable(
                name: "grupos");

            migrationBuilder.DropIndex(
                name: "ix_leads_grupo_id",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "grupo_id",
                table: "leads");
        }
    }
}
