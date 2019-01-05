using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace California.Creator.Service.Migrations
{
    public partial class SpecialLayoutBoxType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpecialLayoutBoxType",
                table: "LayoutBase",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecialLayoutBoxType",
                table: "LayoutBase");
        }
    }
}
