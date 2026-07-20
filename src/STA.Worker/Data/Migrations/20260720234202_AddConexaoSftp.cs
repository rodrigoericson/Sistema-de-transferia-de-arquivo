using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace STA.Worker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConexaoSftp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_conexao_sftp",
                schema: "sta",
                columns: table => new
                {
                    cn_conexao_sftp = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nm_conexao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ds_host = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    nr_porta = table.Column<int>(type: "integer", nullable: false, defaultValue: 22),
                    ds_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ds_senha_criptografada = table.Column<byte[]>(type: "bytea", nullable: true),
                    ds_caminho_chave_privada = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ds_horarios_execucao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ds_dias_semana = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "seg,ter,qua,qui,sex"),
                    fl_arquivo_obrigatorio = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    nr_tolerancia_minutos = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    fl_ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    dt_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    dt_ultimo_uso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_conexao_sftp", x => x.cn_conexao_sftp);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_conexao_sftp_nm_conexao",
                schema: "sta",
                table: "tbl_conexao_sftp",
                column: "nm_conexao",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_conexao_sftp",
                schema: "sta");
        }
    }
}
