using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace STA.Worker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DsPadraoRename",
                schema: "sta",
                table: "tbl_rota_destino",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "tbl_usuario",
                schema: "sta",
                columns: table => new
                {
                    cn_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nm_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nm_display = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ds_senha_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    id_role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Viewer"),
                    fl_ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    nr_tentativas_falhas = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    dt_bloqueado_ate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dt_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    dt_ultimo_login = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_usuario", x => x.cn_usuario);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_usuario_nm_usuario",
                schema: "sta",
                table: "tbl_usuario",
                column: "nm_usuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_usuario",
                schema: "sta");

            migrationBuilder.AlterColumn<string>(
                name: "DsPadraoRename",
                schema: "sta",
                table: "tbl_rota_destino",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
