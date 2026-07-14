using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STA.Worker.Data.Migrations
{
    public partial class SeedSistemaEParametros : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO sta.tbl_sistema (cd_alias_sistema)
                VALUES ('STA')
                ON CONFLICT (cd_alias_sistema) DO NOTHING;

                INSERT INTO sta.tbl_parametro_sistema (cn_parametro_sistema, cn_sistema, cd_parametro_sistema)
                SELECT 1, s.cn_sistema, '08:00:00'
                FROM sta.tbl_sistema s WHERE s.cd_alias_sistema = 'STA'
                ON CONFLICT DO NOTHING;

                INSERT INTO sta.tbl_parametro_sistema (cn_parametro_sistema, cn_sistema, cd_parametro_sistema)
                SELECT 2, s.cn_sistema, '23:59:00'
                FROM sta.tbl_sistema s WHERE s.cd_alias_sistema = 'STA'
                ON CONFLICT DO NOTHING;

                INSERT INTO sta.tbl_parametro_sistema (cn_parametro_sistema, cn_sistema, cd_parametro_sistema)
                SELECT 3, s.cn_sistema, '5'
                FROM sta.tbl_sistema s WHERE s.cd_alias_sistema = 'STA'
                ON CONFLICT DO NOTHING;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM sta.tbl_parametro_sistema
                WHERE cn_sistema = (SELECT cn_sistema FROM sta.tbl_sistema WHERE cd_alias_sistema = 'STA')
                  AND cn_parametro_sistema IN (1, 2, 3);

                DELETE FROM sta.tbl_sistema WHERE cd_alias_sistema = 'STA';
            ");
        }
    }
}
