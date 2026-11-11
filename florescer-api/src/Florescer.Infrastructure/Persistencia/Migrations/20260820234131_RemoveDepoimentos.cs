using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Florescer.Infrastructure.Persistencia.Migrations
{
    public partial class RemoveDepoimentos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Depoimentos");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Depoimentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    NomeCliente = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Nota = table.Column<int>(type: "int", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Servico = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Texto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depoimentos", x => x.Id);
                });
        }
    }
}
