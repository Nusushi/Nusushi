using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace California.Creator.Service.Migrations
{
    public partial class WebfontsUserCss : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserDefinedCss",
                table: "CaliforniaView",
                maxLength: 12500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserDefinedCss",
                table: "CaliforniaProject",
                maxLength: 12500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Webfonts",
                columns: table => new
                {
                    WebfontId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Family = table.Column<string>(nullable: false),
                    Version = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Webfonts", x => x.WebfontId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Webfonts");

            migrationBuilder.DropColumn(
                name: "UserDefinedCss",
                table: "CaliforniaView");

            migrationBuilder.DropColumn(
                name: "UserDefinedCss",
                table: "CaliforniaProject");
        }
    }
}
