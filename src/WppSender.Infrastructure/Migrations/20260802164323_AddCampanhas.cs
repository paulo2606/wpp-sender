using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WppSender.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCampanhas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "campanha_envios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lead_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    enviado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    erro = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campanha_envios", x => x.id);
                    table.CheckConstraint("ck_campanha_envios_status", "status IN ('pendente','enviado','falhou')");
                });

            migrationBuilder.CreateTable(
                name: "campanhas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    mensagem = table.Column<string>(type: "text", nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    agendado_para = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    intervalo_min_segundos = table.Column<int>(type: "integer", nullable: false),
                    intervalo_max_segundos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campanhas", x => x.id);
                    table.CheckConstraint("ck_campanhas_status", "status IN ('rascunho','agendada','em_andamento','pausada','concluida')");
                });

            migrationBuilder.CreateTable(
                name: "configuracao_envio",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    limite_diario_envios = table.Column<int>(type: "integer", nullable: false),
                    envios_realizados_hoje = table.Column<int>(type: "integer", nullable: false),
                    data_referencia = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_configuracao_envio", x => x.id);
                    table.CheckConstraint("ck_configuracao_envio_id", "id = 1");
                });

            migrationBuilder.CreateTable(
                name: "whatsapp_sessao",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_whatsapp_sessao", x => x.id);
                    table.CheckConstraint("ck_whatsapp_sessao_status", "status IN ('desconectado','aguardando_qr','conectado')");
                });

            migrationBuilder.InsertData(
                table: "configuracao_envio",
                columns: new[] { "id", "data_referencia", "envios_realizados_hoje", "limite_diario_envios" },
                values: new object[] { (short)1, new DateOnly(2020, 1, 1), 0, 120 });

            migrationBuilder.InsertData(
                table: "whatsapp_sessao",
                columns: new[] { "id", "status" },
                values: new object[] { (short)1, "desconectado" });

            migrationBuilder.CreateIndex(
                name: "ix_campanha_envios_campanha_id_lead_id",
                table: "campanha_envios",
                columns: new[] { "campanha_id", "lead_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_campanha_envios_campanha_id_status",
                table: "campanha_envios",
                columns: new[] { "campanha_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_campanha_envios_lead_id",
                table: "campanha_envios",
                column: "lead_id");

            migrationBuilder.CreateIndex(
                name: "ix_campanhas_grupo_id",
                table: "campanhas",
                column: "grupo_id");

            migrationBuilder.CreateIndex(
                name: "ix_campanhas_status",
                table: "campanhas",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campanha_envios");

            migrationBuilder.DropTable(
                name: "campanhas");

            migrationBuilder.DropTable(
                name: "configuracao_envio");

            migrationBuilder.DropTable(
                name: "whatsapp_sessao");
        }
    }
}
