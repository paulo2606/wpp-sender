using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WppSender.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConcluidaComFalhas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_campanhas_status",
                table: "campanhas");

            migrationBuilder.AddCheckConstraint(
                name: "ck_campanhas_status",
                table: "campanhas",
                sql: "status IN ('rascunho','agendada','em_andamento','pausada','concluida','concluida_com_falhas','cancelada')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_campanhas_status",
                table: "campanhas");

            migrationBuilder.AddCheckConstraint(
                name: "ck_campanhas_status",
                table: "campanhas",
                sql: "status IN ('rascunho','agendada','em_andamento','pausada','concluida','cancelada')");
        }
    }
}
