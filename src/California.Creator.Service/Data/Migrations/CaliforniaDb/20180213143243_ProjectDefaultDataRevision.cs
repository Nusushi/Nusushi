using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace California.Creator.Service.Migrations
{
    public partial class ProjectDefaultDataRevision : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInstanceable",
                table: "LayoutBase");

            migrationBuilder.AddColumn<int>(
                name: "ProjectDefaultsRevision",
                table: "CaliforniaProject",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectDefaultsRevision",
                table: "CaliforniaProject");

            migrationBuilder.AddColumn<bool>(
                name: "IsInstanceable",
                table: "LayoutBase",
                nullable: true);
        }
    }
}
