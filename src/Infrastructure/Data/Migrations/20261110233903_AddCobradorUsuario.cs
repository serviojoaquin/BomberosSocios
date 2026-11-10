using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseApp.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCobradorUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: "Cobradores",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cobradores_UsuarioId",
                table: "Cobradores",
                column: "UsuarioId",
                unique: true,
                filter: "[UsuarioId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cobradores_UsuarioId",
                table: "Cobradores");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Cobradores");
        }
    }
}
