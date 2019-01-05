using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace California.Creator.Service.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "LayoutBaseOrder");

            migrationBuilder.CreateSequence<int>(
                name: "ViewUnitOrder");

            migrationBuilder.CreateTable(
                name: "CaliforniaStores",
                columns: table => new
                {
                    CaliforniaStoreId = table.Column<string>(nullable: false),
                    CaliforniaUserDefaultsId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaliforniaStores", x => x.CaliforniaStoreId);
                });

            migrationBuilder.CreateTable(
                name: "CaliforniaProject",
                columns: table => new
                {
                    CaliforniaProjectId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CaliforniaStoreId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaliforniaProject", x => x.CaliforniaProjectId);
                    table.ForeignKey(
                        name: "FK_CaliforniaProject_CaliforniaStores_CaliforniaStoreId",
                        column: x => x.CaliforniaStoreId,
                        principalTable: "CaliforniaStores",
                        principalColumn: "CaliforniaStoreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaliforniaUserDefaults",
                columns: table => new
                {
                    CaliforniaUserDefaultsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CaliforniaStoreId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaliforniaUserDefaults", x => x.CaliforniaUserDefaultsId);
                    table.ForeignKey(
                        name: "FK_CaliforniaUserDefaults_CaliforniaStores_CaliforniaStoreId",
                        column: x => x.CaliforniaStoreId,
                        principalTable: "CaliforniaStores",
                        principalColumn: "CaliforniaStoreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaliforniaView",
                columns: table => new
                {
                    CaliforniaViewId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CaliforniaProjectId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    QueryUrl = table.Column<string>(nullable: false),
                    ViewSortOrderKey = table.Column<int>(nullable: false, defaultValueSql: "NEXT VALUE FOR ViewUnitOrder")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaliforniaView", x => x.CaliforniaViewId);
                    table.ForeignKey(
                        name: "FK_CaliforniaView_CaliforniaProject_CaliforniaProjectId",
                        column: x => x.CaliforniaProjectId,
                        principalTable: "CaliforniaProject",
                        principalColumn: "CaliforniaProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PictureContent",
                columns: table => new
                {
                    PictureContentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CaliforniaProjectId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PictureContent", x => x.PictureContentId);
                    table.ForeignKey(
                        name: "FK_PictureContent_CaliforniaProject_CaliforniaProjectId",
                        column: x => x.CaliforniaProjectId,
                        principalTable: "CaliforniaProject",
                        principalColumn: "CaliforniaProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResponsiveDevice",
                columns: table => new
                {
                    ResponsiveDeviceId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CaliforniaProjectId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    NameShort = table.Column<string>(maxLength: 5, nullable: false),
                    WidthThreshold = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsiveDevice", x => x.ResponsiveDeviceId);
                    table.ForeignKey(
                        name: "FK_ResponsiveDevice_CaliforniaProject_CaliforniaProjectId",
                        column: x => x.CaliforniaProjectId,
                        principalTable: "CaliforniaProject",
                        principalColumn: "CaliforniaProjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StyleQuantum",
                columns: table => new
                {
                    StyleQuantumId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CaliforniaProjectId = table.Column<int>(nullable: false),
                    CssProperty = table.Column<string>(nullable: false),
                    CssValue = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StyleQuantum", x => x.StyleQuantumId);
                    table.ForeignKey(
                        name: "FK_StyleQuantum_CaliforniaProject_CaliforniaProjectId",
                        column: x => x.CaliforniaProjectId,
                        principalTable: "CaliforniaProject",
                        principalColumn: "CaliforniaProjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LayoutBase",
                columns: table => new
                {
                    PlacedAtomInBoxId = table.Column<int>(nullable: true),
                    LayoutBaseId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CaliforniaProjectId = table.Column<int>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    LayoutSortOrderKey = table.Column<int>(nullable: false, defaultValueSql: "NEXT VALUE FOR LayoutBaseOrder"),
                    BoxOwnerRowId = table.Column<int>(nullable: true),
                    PlacedBoxInBoxId = table.Column<int>(nullable: true),
                    ClonedFromLayoutId = table.Column<int>(nullable: true),
                    HostedViewId = table.Column<int>(nullable: true),
                    IsInstanceable = table.Column<bool>(nullable: true),
                    PlacedOnViewId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LayoutBase", x => x.LayoutBaseId);
                    table.ForeignKey(
                        name: "FK_LayoutBase_LayoutBase_PlacedAtomInBoxId",
                        column: x => x.PlacedAtomInBoxId,
                        principalTable: "LayoutBase",
                        principalColumn: "LayoutBaseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LayoutBase_CaliforniaProject_CaliforniaProjectId",
                        column: x => x.CaliforniaProjectId,
                        principalTable: "CaliforniaProject",
                        principalColumn: "CaliforniaProjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LayoutBase_LayoutBase_BoxOwnerRowId",
                        column: x => x.BoxOwnerRowId,
                        principalTable: "LayoutBase",
                        principalColumn: "LayoutBaseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LayoutBase_LayoutBase_PlacedBoxInBoxId",
                        column: x => x.PlacedBoxInBoxId,
                        principalTable: "LayoutBase",
                        principalColumn: "LayoutBaseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LayoutBase_LayoutBase_ClonedFromLayoutId",
                        column: x => x.ClonedFromLayoutId,
                        principalTable: "LayoutBase",
                        principalColumn: "LayoutBaseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LayoutBase_CaliforniaView_HostedViewId",
                        column: x => x.HostedViewId,
                        principalTable: "CaliforniaView",
                        principalColumn: "CaliforniaViewId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LayoutBase_CaliforniaView_PlacedOnViewId",
                        column: x => x.PlacedOnViewId,
                        principalTable: "CaliforniaView",
                        principalColumn: "CaliforniaViewId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentAtom",
                columns: table => new
                {
                    ContentAtomId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CaliforniaProjectId = table.Column<int>(nullable: false),
                    CaliforniaViewId = table.Column<int>(nullable: true),
                    ContentAtomType = table.Column<int>(nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(nullable: true),
                    InstancedOnLayoutId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    PictureContentId = table.Column<int>(nullable: true),
                    TextContent = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentAtom", x => x.ContentAtomId);
                    table.ForeignKey(
                        name: "FK_ContentAtom_CaliforniaProject_CaliforniaProjectId",
                        column: x => x.CaliforniaProjectId,
                        principalTable: "CaliforniaProject",
                        principalColumn: "CaliforniaProjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContentAtom_CaliforniaView_CaliforniaViewId",
                        column: x => x.CaliforniaViewId,
                        principalTable: "CaliforniaView",
                        principalColumn: "CaliforniaViewId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContentAtom_LayoutBase_InstancedOnLayoutId",
                        column: x => x.InstancedOnLayoutId,
                        principalTable: "LayoutBase",
                        principalColumn: "LayoutBaseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContentAtom_PictureContent_PictureContentId",
                        column: x => x.PictureContentId,
                        principalTable: "PictureContent",
                        principalColumn: "PictureContentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StyleMolecule",
                columns: table => new
                {
                    StyleMoleculeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CaliforniaProjectId = table.Column<int>(nullable: false),
                    ClonedFromStyleId = table.Column<int>(nullable: true),
                    HtmlTag = table.Column<string>(maxLength: 255, nullable: true),
                    IsListed = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    NameShort = table.Column<string>(maxLength: 255, nullable: false),
                    StyleForLayoutId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StyleMolecule", x => x.StyleMoleculeId);
                    table.ForeignKey(
                        name: "FK_StyleMolecule_CaliforniaProject_CaliforniaProjectId",
                        column: x => x.CaliforniaProjectId,
                        principalTable: "CaliforniaProject",
                        principalColumn: "CaliforniaProjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StyleMolecule_StyleMolecule_ClonedFromStyleId",
                        column: x => x.ClonedFromStyleId,
                        principalTable: "StyleMolecule",
                        principalColumn: "StyleMoleculeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StyleMolecule_LayoutBase_StyleForLayoutId",
                        column: x => x.StyleForLayoutId,
                        principalTable: "LayoutBase",
                        principalColumn: "LayoutBaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DbContentSafetyLock",
                columns: table => new
                {
                    DbContentSafetyLockId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ContentAtomId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbContentSafetyLock", x => x.DbContentSafetyLockId);
                    table.ForeignKey(
                        name: "FK_ContentAtom_RemoveIsNotSafe_RemoveIsNotSafeId",
                        column: x => x.ContentAtomId,
                        principalTable: "ContentAtom",
                        principalColumn: "ContentAtomId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StyleMoleculeAtomMapping",
                columns: table => new
                {
                    StyleMoleculeAtomMappingId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ResponsiveDeviceId = table.Column<int>(nullable: false),
                    StateModifier = table.Column<string>(maxLength: 255, nullable: true),
                    StyleMoleculeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StyleMoleculeAtomMapping", x => x.StyleMoleculeAtomMappingId);
                    table.ForeignKey(
                        name: "FK_StyleMoleculeAtomMapping_ResponsiveDevice_ResponsiveDeviceId",
                        column: x => x.ResponsiveDeviceId,
                        principalTable: "ResponsiveDevice",
                        principalColumn: "ResponsiveDeviceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StyleMoleculeAtomMapping_StyleMolecule_StyleMoleculeId",
                        column: x => x.StyleMoleculeId,
                        principalTable: "StyleMolecule",
                        principalColumn: "StyleMoleculeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StyleAtom",
                columns: table => new
                {
                    StyleAtomId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CaliforniaProjectId = table.Column<int>(nullable: false),
                    MappedToMoleculeId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    StyleAtomType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StyleAtom", x => x.StyleAtomId);
                    table.ForeignKey(
                        name: "FK_StyleAtom_CaliforniaProject_CaliforniaProjectId",
                        column: x => x.CaliforniaProjectId,
                        principalTable: "CaliforniaProject",
                        principalColumn: "CaliforniaProjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StyleAtom_StyleMoleculeAtomMapping_MappedToMoleculeId",
                        column: x => x.MappedToMoleculeId,
                        principalTable: "StyleMoleculeAtomMapping",
                        principalColumn: "StyleMoleculeAtomMappingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StyleAtomQuantumMapping",
                columns: table => new
                {
                    StyleAtomQuantumMappingId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    StyleAtomId = table.Column<int>(nullable: false),
                    StyleQuantumId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StyleAtomQuantumMapping", x => x.StyleAtomQuantumMappingId);
                    table.ForeignKey(
                        name: "FK_StyleAtomQuantumMapping_StyleAtom_StyleAtomId",
                        column: x => x.StyleAtomId,
                        principalTable: "StyleAtom",
                        principalColumn: "StyleAtomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StyleAtomQuantumMapping_StyleQuantum_StyleQuantumId",
                        column: x => x.StyleQuantumId,
                        principalTable: "StyleQuantum",
                        principalColumn: "StyleQuantumId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StyleValue",
                columns: table => new
                {
                    StyleValueId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CaliforniaProjectId = table.Column<int>(nullable: false),
                    CssProperty = table.Column<string>(nullable: false),
                    CssValue = table.Column<string>(nullable: false),
                    StyleAtomId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StyleValue", x => x.StyleValueId);
                    table.ForeignKey(
                        name: "FK_StyleValue_CaliforniaProject_CaliforniaProjectId",
                        column: x => x.CaliforniaProjectId,
                        principalTable: "CaliforniaProject",
                        principalColumn: "CaliforniaProjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StyleValue_StyleAtom_StyleAtomId",
                        column: x => x.StyleAtomId,
                        principalTable: "StyleAtom",
                        principalColumn: "StyleAtomId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaliforniaProject_CaliforniaStoreId",
                table: "CaliforniaProject",
                column: "CaliforniaStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_CaliforniaUserDefaults_CaliforniaStoreId",
                table: "CaliforniaUserDefaults",
                column: "CaliforniaStoreId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaliforniaView_CaliforniaProjectId",
                table: "CaliforniaView",
                column: "CaliforniaProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentAtom_CaliforniaProjectId",
                table: "ContentAtom",
                column: "CaliforniaProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentAtom_CaliforniaViewId",
                table: "ContentAtom",
                column: "CaliforniaViewId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentAtom_InstancedOnLayoutId",
                table: "ContentAtom",
                column: "InstancedOnLayoutId",
                unique: true,
                filter: "[InstancedOnLayoutId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ContentAtom_PictureContentId",
                table: "ContentAtom",
                column: "PictureContentId");

            migrationBuilder.CreateIndex(
                name: "IX_DbContentSafetyLock_ContentAtomId",
                table: "DbContentSafetyLock",
                column: "ContentAtomId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LayoutBase_PlacedAtomInBoxId",
                table: "LayoutBase",
                column: "PlacedAtomInBoxId");

            migrationBuilder.CreateIndex(
                name: "IX_LayoutBase_CaliforniaProjectId",
                table: "LayoutBase",
                column: "CaliforniaProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LayoutBase_BoxOwnerRowId",
                table: "LayoutBase",
                column: "BoxOwnerRowId");

            migrationBuilder.CreateIndex(
                name: "IX_LayoutBase_PlacedBoxInBoxId",
                table: "LayoutBase",
                column: "PlacedBoxInBoxId");

            migrationBuilder.CreateIndex(
                name: "IX_LayoutBase_ClonedFromLayoutId",
                table: "LayoutBase",
                column: "ClonedFromLayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_LayoutBase_HostedViewId",
                table: "LayoutBase",
                column: "HostedViewId");

            migrationBuilder.CreateIndex(
                name: "IX_LayoutBase_PlacedOnViewId",
                table: "LayoutBase",
                column: "PlacedOnViewId");

            migrationBuilder.CreateIndex(
                name: "IX_PictureContent_CaliforniaProjectId",
                table: "PictureContent",
                column: "CaliforniaProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsiveDevice_CaliforniaProjectId",
                table: "ResponsiveDevice",
                column: "CaliforniaProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StyleAtom_CaliforniaProjectId",
                table: "StyleAtom",
                column: "CaliforniaProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StyleAtom_MappedToMoleculeId",
                table: "StyleAtom",
                column: "MappedToMoleculeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StyleAtomQuantumMapping_StyleAtomId",
                table: "StyleAtomQuantumMapping",
                column: "StyleAtomId");

            migrationBuilder.CreateIndex(
                name: "IX_StyleAtomQuantumMapping_StyleQuantumId_StyleAtomId",
                table: "StyleAtomQuantumMapping",
                columns: new[] { "StyleQuantumId", "StyleAtomId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StyleMolecule_CaliforniaProjectId",
                table: "StyleMolecule",
                column: "CaliforniaProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StyleMolecule_ClonedFromStyleId",
                table: "StyleMolecule",
                column: "ClonedFromStyleId");

            migrationBuilder.CreateIndex(
                name: "IX_StyleMolecule_StyleForLayoutId",
                table: "StyleMolecule",
                column: "StyleForLayoutId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StyleMoleculeAtomMapping_ResponsiveDeviceId",
                table: "StyleMoleculeAtomMapping",
                column: "ResponsiveDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_StyleMoleculeAtomMapping_StyleMoleculeId",
                table: "StyleMoleculeAtomMapping",
                column: "StyleMoleculeId");

            migrationBuilder.CreateIndex(
                name: "IX_StyleQuantum_CaliforniaProjectId",
                table: "StyleQuantum",
                column: "CaliforniaProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StyleValue_CaliforniaProjectId",
                table: "StyleValue",
                column: "CaliforniaProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StyleValue_StyleAtomId",
                table: "StyleValue",
                column: "StyleAtomId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaliforniaUserDefaults");

            migrationBuilder.DropTable(
                name: "DbContentSafetyLock");

            migrationBuilder.DropTable(
                name: "StyleAtomQuantumMapping");

            migrationBuilder.DropTable(
                name: "StyleValue");

            migrationBuilder.DropTable(
                name: "ContentAtom");

            migrationBuilder.DropTable(
                name: "StyleQuantum");

            migrationBuilder.DropTable(
                name: "StyleAtom");

            migrationBuilder.DropTable(
                name: "PictureContent");

            migrationBuilder.DropTable(
                name: "StyleMoleculeAtomMapping");

            migrationBuilder.DropTable(
                name: "ResponsiveDevice");

            migrationBuilder.DropTable(
                name: "StyleMolecule");

            migrationBuilder.DropTable(
                name: "LayoutBase");

            migrationBuilder.DropTable(
                name: "CaliforniaView");

            migrationBuilder.DropTable(
                name: "CaliforniaProject");

            migrationBuilder.DropTable(
                name: "CaliforniaStores");

            migrationBuilder.DropSequence(
                name: "LayoutBaseOrder");

            migrationBuilder.DropSequence(
                name: "ViewUnitOrder");
        }
    }
}
