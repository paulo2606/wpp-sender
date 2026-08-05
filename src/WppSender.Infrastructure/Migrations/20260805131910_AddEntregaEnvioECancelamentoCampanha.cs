using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WppSender.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntregaEnvioECancelamentoCampanha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_campanhas_status",
                table: "campanhas");

            migrationBuilder.DropCheckConstraint(
                name: "ck_campanha_envios_status",
                table: "campanha_envios");

            migrationBuilder.AddColumn<string>(
                name: "whatsapp_mensagem_id",
                table: "campanha_envios",
                type: "text",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_campanhas_status",
                table: "campanhas",
                sql: "status IN ('rascunho','agendada','em_andamento','pausada','concluida','cancelada')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_campanha_envios_status",
                table: "campanha_envios",
                sql: "status IN ('pendente','enviado','entregue','lido','falhou','falhou_entrega')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_campanhas_status",
                table: "campanhas");

            migrationBuilder.DropCheckConstraint(
                name: "ck_campanha_envios_status",
                table: "campanha_envios");

            migrationBuilder.DropColumn(
                name: "whatsapp_mensagem_id",
                table: "campanha_envios");

            migrationBuilder.AddCheckConstraint(
                name: "ck_campanhas_status",
                table: "campanhas",
                sql: "status IN ('rascunho','agendada','em_andamento','pausada','concluida')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_campanha_envios_status",
                table: "campanha_envios",
                sql: "status IN ('pendente','enviado','falhou')");
        }
    }
}
