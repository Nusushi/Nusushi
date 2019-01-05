using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace California.Creator.Service.Migrations
{
    public partial class ViewBoxMapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentAtom_CaliforniaView_CaliforniaViewId",
                table: "ContentAtom");

            migrationBuilder.DropForeignKey(
                name: "FK_LayoutBase_LayoutBase_ClonedFromLayoutId",
                table: "LayoutBase");

            migrationBuilder.DropForeignKey(
                name: "FK_LayoutBase_CaliforniaView_HostedViewId",
                table: "LayoutBase");

            migrationBuilder.DropForeignKey(
                name: "FK_LayoutBase_CaliforniaView_PlacedOnViewId",
                table: "LayoutBase");

            migrationBuilder.DropIndex(
                name: "IX_LayoutBase_ClonedFromLayoutId",
                table: "LayoutBase");

            migrationBuilder.DropIndex(
                name: "IX_LayoutBase_HostedViewId",
                table: "LayoutBase");

            migrationBuilder.DropIndex(
                name: "IX_ContentAtom_CaliforniaViewId",
                table: "ContentAtom");

            migrationBuilder.DropColumn(
                name: "ClonedFromLayoutId",
                table: "LayoutBase");

            migrationBuilder.DropColumn(
                name: "HostedViewId",
                table: "LayoutBase");

            migrationBuilder.DropColumn(
                name: "CaliforniaViewId",
                table: "ContentAtom");

            migrationBuilder.CreateTable(
                name: "QueryViewLayoutBoxMapping",
                columns: table => new
                {
                    QueryViewLayoutBoxMappingId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CaliforniaViewId = table.Column<int>(nullable: false),
                    LayoutBoxId = table.Column<int>(nullable: false),
                    QueryString = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryViewLayoutBoxMapping", x => x.QueryViewLayoutBoxMappingId);
                    table.ForeignKey(
                        name: "FK_QueryViewLayoutBoxMapping_CaliforniaView_CaliforniaViewId",
                        column: x => x.CaliforniaViewId,
                        principalTable: "CaliforniaView",
                        principalColumn: "CaliforniaViewId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QueryViewLayoutBoxMapping_LayoutBase_LayoutBoxId",
                        column: x => x.LayoutBoxId,
                        principalTable: "LayoutBase",
                        principalColumn: "LayoutBaseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QueryViewLayoutBoxMapping_CaliforniaViewId",
                table: "QueryViewLayoutBoxMapping",
                column: "CaliforniaViewId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryViewLayoutBoxMapping_LayoutBoxId",
                table: "QueryViewLayoutBoxMapping",
                column: "LayoutBoxId");

            migrationBuilder.AddForeignKey(
                name: "FK_LayoutBase_CaliforniaView_PlacedOnViewId",
                table: "LayoutBase",
                column: "PlacedOnViewId",
                principalTable: "CaliforniaView",
                principalColumn: "CaliforniaViewId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LayoutBase_CaliforniaView_PlacedOnViewId",
                table: "LayoutBase");

            migrationBuilder.DropTable(
                name: "QueryViewLayoutBoxMapping");

            migrationBuilder.AddColumn<int>(
                name: "ClonedFromLayoutId",
                table: "LayoutBase",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HostedViewId",
                table: "LayoutBase",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CaliforniaViewId",
                table: "ContentAtom",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LayoutBase_ClonedFromLayoutId",
                table: "LayoutBase",
                column: "ClonedFromLayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_LayoutBase_HostedViewId",
                table: "LayoutBase",
                column: "HostedViewId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentAtom_CaliforniaViewId",
                table: "ContentAtom",
                column: "CaliforniaViewId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentAtom_CaliforniaView_CaliforniaViewId",
                table: "ContentAtom",
                column: "CaliforniaViewId",
                principalTable: "CaliforniaView",
                principalColumn: "CaliforniaViewId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LayoutBase_LayoutBase_ClonedFromLayoutId",
                table: "LayoutBase",
                column: "ClonedFromLayoutId",
                principalTable: "LayoutBase",
                principalColumn: "LayoutBaseId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LayoutBase_CaliforniaView_HostedViewId",
                table: "LayoutBase",
                column: "HostedViewId",
                principalTable: "CaliforniaView",
                principalColumn: "CaliforniaViewId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LayoutBase_CaliforniaView_PlacedOnViewId",
                table: "LayoutBase",
                column: "PlacedOnViewId",
                principalTable: "CaliforniaView",
                principalColumn: "CaliforniaViewId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
