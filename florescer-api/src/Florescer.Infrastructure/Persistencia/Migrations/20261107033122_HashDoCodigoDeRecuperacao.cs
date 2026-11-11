using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Florescer.Infrastructure.Persistencia.Migrations
{
    public partial class HashDoCodigoDeRecuperacao : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CodigosRecuperacao_UsuarioId_Codigo",
                table: "CodigosRecuperacao");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "CodigosRecuperacao");

            migrationBuilder.AddColumn<string>(
                name: "CodigoHash",
                table: "CodigosRecuperacao",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CodigosRecuperacao_UsuarioId_UsadoEm",
                table: "CodigosRecuperacao",
                columns: new[] { "UsuarioId", "UsadoEm" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CodigosRecuperacao_UsuarioId_UsadoEm",
                table: "CodigosRecuperacao");

            migrationBuilder.DropColumn(
                name: "CodigoHash",
                table: "CodigosRecuperacao");

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "CodigosRecuperacao",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CodigosRecuperacao_UsuarioId_Codigo",
                table: "CodigosRecuperacao",
                columns: new[] { "UsuarioId", "Codigo" });
        }
    }
}
