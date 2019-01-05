using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace California.Creator.Service.Migrations
{
    public partial class SharedProjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SharedProjectInfo",
                columns: table => new
                {
                    SharedProjectInfoId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CaliforniaProjectId = table.Column<int>(nullable: false),
                    IsEditAllowed = table.Column<bool>(nullable: false),
                    IsReshareAllowed = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    OwnerCaliforniaStoreId = table.Column<string>(nullable: false),
                    ShareEnabledTime = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    SharedWithCaliforniaStoreId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedProjectInfo", x => x.SharedProjectInfoId);
                    table.ForeignKey(
                        name: "FK_SharedProjectInfo_CaliforniaProject_CaliforniaProjectId",
                        column: x => x.CaliforniaProjectId,
                        principalTable: "CaliforniaProject",
                        principalColumn: "CaliforniaProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SharedProjectInfo_CaliforniaStores_OwnerCaliforniaStoreId",
                        column: x => x.OwnerCaliforniaStoreId,
                        principalTable: "CaliforniaStores",
                        principalColumn: "CaliforniaStoreId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedProjectInfo_CaliforniaStores_SharedWithCaliforniaStoreId",
                        column: x => x.SharedWithCaliforniaStoreId,
                        principalTable: "CaliforniaStores",
                        principalColumn: "CaliforniaStoreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedProjectInfo_OwnerCaliforniaStoreId",
                table: "SharedProjectInfo",
                column: "OwnerCaliforniaStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedProjectInfo_SharedWithCaliforniaStoreId",
                table: "SharedProjectInfo",
                column: "SharedWithCaliforniaStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedProjectInfo_CaliforniaProjectId_SharedWithCaliforniaStoreId",
                table: "SharedProjectInfo",
                columns: new[] { "CaliforniaProjectId", "SharedWithCaliforniaStoreId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedProjectInfo");
        }
    }
}
