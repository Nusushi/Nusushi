using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace California.Creator.Service.Migrations
{
    public partial class Interactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LayoutStyleInteraction",
                columns: table => new
                {
                    LayoutStyleInteractionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CaliforniaProjectId = table.Column<int>(nullable: false),
                    LayoutAtomId = table.Column<int>(nullable: false),
                    LayoutStyleInteractionType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LayoutStyleInteraction", x => x.LayoutStyleInteractionId);
                    table.ForeignKey(
                        name: "FK_LayoutStyleInteraction_CaliforniaProject_CaliforniaProjectId",
                        column: x => x.CaliforniaProjectId,
                        principalTable: "CaliforniaProject",
                        principalColumn: "CaliforniaProjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LayoutStyleInteraction_LayoutBase_LayoutAtomId",
                        column: x => x.LayoutAtomId,
                        principalTable: "LayoutBase",
                        principalColumn: "LayoutBaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StyleValueInteractionMapping",
                columns: table => new
                {
                    StyleValueInteractionMappingId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CssValue = table.Column<string>(nullable: false),
                    LayoutStyleInteractionId = table.Column<int>(nullable: false),
                    StyleValueId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StyleValueInteractionMapping", x => x.StyleValueInteractionMappingId);
                    table.ForeignKey(
                        name: "FK_StyleValueInteractionMapping_LayoutStyleInteraction_LayoutStyleInteractionId",
                        column: x => x.LayoutStyleInteractionId,
                        principalTable: "LayoutStyleInteraction",
                        principalColumn: "LayoutStyleInteractionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StyleValueInteractionMapping_StyleValue_StyleValueId",
                        column: x => x.StyleValueId,
                        principalTable: "StyleValue",
                        principalColumn: "StyleValueId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LayoutStyleInteraction_CaliforniaProjectId",
                table: "LayoutStyleInteraction",
                column: "CaliforniaProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LayoutStyleInteraction_LayoutAtomId",
                table: "LayoutStyleInteraction",
                column: "LayoutAtomId");

            migrationBuilder.CreateIndex(
                name: "IX_StyleValueInteractionMapping_StyleValueId",
                table: "StyleValueInteractionMapping",
                column: "StyleValueId");

            migrationBuilder.CreateIndex(
                name: "IX_StyleValueInteractionMapping_LayoutStyleInteractionId_StyleValueId",
                table: "StyleValueInteractionMapping",
                columns: new[] { "LayoutStyleInteractionId", "StyleValueId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StyleValueInteractionMapping");

            migrationBuilder.DropTable(
                name: "LayoutStyleInteraction");
        }
    }
}
