using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace California.Creator.Service.Migrations
{
    public partial class RestrictQuantumCascade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StyleAtomQuantumMapping_StyleQuantum_StyleQuantumId",
                table: "StyleAtomQuantumMapping");

            migrationBuilder.AddForeignKey(
                name: "FK_StyleAtomQuantumMapping_StyleQuantum_StyleQuantumId",
                table: "StyleAtomQuantumMapping",
                column: "StyleQuantumId",
                principalTable: "StyleQuantum",
                principalColumn: "StyleQuantumId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StyleAtomQuantumMapping_StyleQuantum_StyleQuantumId",
                table: "StyleAtomQuantumMapping");

            migrationBuilder.AddForeignKey(
                name: "FK_StyleAtomQuantumMapping_StyleQuantum_StyleQuantumId",
                table: "StyleAtomQuantumMapping",
                column: "StyleQuantumId",
                principalTable: "StyleQuantum",
                principalColumn: "StyleQuantumId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
