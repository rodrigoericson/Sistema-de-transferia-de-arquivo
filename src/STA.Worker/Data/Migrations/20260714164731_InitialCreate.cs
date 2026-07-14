using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace STA.Worker.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sta");

            migrationBuilder.CreateTable(
                name: "tbl_sistema",
                schema: "sta",
                columns: table => new
                {
                    cn_sistema = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cd_alias_sistema = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_sistema", x => x.cn_sistema);
                });

            migrationBuilder.CreateTable(
                name: "tbl_log_processo",
                schema: "sta",
                columns: table => new
                {
                    cn_log_processo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cn_sistema = table.Column<int>(type: "integer", nullable: false),
                    cn_processo = table.Column<int>(type: "integer", nullable: false),
                    dt_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    dt_fim_processo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_status_processo = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    qt_registros_processados = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    vl_registros_processados = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    qt_registros_erro = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    vl_registros_erro = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    xml_obs_processo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_log_processo", x => x.cn_log_processo);
                    table.ForeignKey(
                        name: "FK_tbl_log_processo_tbl_sistema_cn_sistema",
                        column: x => x.cn_sistema,
                        principalSchema: "sta",
                        principalTable: "tbl_sistema",
                        principalColumn: "cn_sistema",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_parametro_sistema",
                schema: "sta",
                columns: table => new
                {
                    cn_parametro_sistema = table.Column<int>(type: "integer", nullable: false),
                    cn_sistema = table.Column<int>(type: "integer", nullable: false),
                    cd_parametro_sistema = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_parametro_sistema", x => new { x.cn_parametro_sistema, x.cn_sistema });
                    table.ForeignKey(
                        name: "FK_tbl_parametro_sistema_tbl_sistema_cn_sistema",
                        column: x => x.cn_sistema,
                        principalSchema: "sta",
                        principalTable: "tbl_sistema",
                        principalColumn: "cn_sistema",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_log_processo_cn_sistema_cn_processo",
                schema: "sta",
                table: "tbl_log_processo",
                columns: new[] { "cn_sistema", "cn_processo" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_log_processo_dt_inicio",
                schema: "sta",
                table: "tbl_log_processo",
                column: "dt_inicio");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_parametro_sistema_cn_sistema",
                schema: "sta",
                table: "tbl_parametro_sistema",
                column: "cn_sistema");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_sistema_cd_alias_sistema",
                schema: "sta",
                table: "tbl_sistema",
                column: "cd_alias_sistema",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_log_processo",
                schema: "sta");

            migrationBuilder.DropTable(
                name: "tbl_parametro_sistema",
                schema: "sta");

            migrationBuilder.DropTable(
                name: "tbl_sistema",
                schema: "sta");
        }
    }
}
