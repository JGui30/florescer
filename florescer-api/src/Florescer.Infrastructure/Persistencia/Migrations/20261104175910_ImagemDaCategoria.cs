using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Florescer.Infrastructure.Persistencia.Migrations
{
    public partial class ImagemDaCategoria : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagemUrl",
                table: "Categorias",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagemUrl",
                table: "Categorias");
        }
    }
}
