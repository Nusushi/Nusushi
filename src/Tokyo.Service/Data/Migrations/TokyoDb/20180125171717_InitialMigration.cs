using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Tokyo.Service.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "TimeNormOrder");

            migrationBuilder.CreateSequence<int>(
                name: "WorkUnitOrder");

            migrationBuilder.CreateTable(
                name: "TrackerStores",
                columns: table => new
                {
                    TrackerStoreId = table.Column<string>(nullable: false),
                    TrackerInsightsId = table.Column<int>(nullable: false),
                    TrackerUserDefaultsId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackerStores", x => x.TrackerStoreId);
                });

            migrationBuilder.CreateTable(
                name: "TimeNormTag",
                columns: table => new
                {
                    TimeNormTagId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Color = table.Column<string>(maxLength: 255, nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    TrackerStoreId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeNormTag", x => x.TimeNormTagId);
                    table.ForeignKey(
                        name: "FK_TimeNormTag_TrackerStores_TrackerStoreId",
                        column: x => x.TrackerStoreId,
                        principalTable: "TrackerStores",
                        principalColumn: "TrackerStoreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TimeStamp",
                columns: table => new
                {
                    TimeStampId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AbsoluteOfUtcOffset = table.Column<TimeSpan>(nullable: false),
                    IsNegativeUtcOffset = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    TimeZoneIdAtCreation = table.Column<string>(maxLength: 255, nullable: false, defaultValueSql: "'UTC'"),
                    TrackedTimeUTC = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "SYSDATETIMEOFFSET() AT TIME ZONE 'UTC'"),
                    TrackerStoreId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeStamp", x => x.TimeStampId);
                    table.ForeignKey(
                        name: "FK_TimeStamp_TrackerStores_TrackerStoreId",
                        column: x => x.TrackerStoreId,
                        principalTable: "TrackerStores",
                        principalColumn: "TrackerStoreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeTable",
                columns: table => new
                {
                    TimeTableId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsFrozen = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    TargetWeeklyTimeId = table.Column<int>(nullable: true),
                    TrackerStoreId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeTable", x => x.TimeTableId);
                    table.ForeignKey(
                        name: "FK_TimeTable_TrackerStores_TrackerStoreId",
                        column: x => x.TrackerStoreId,
                        principalTable: "TrackerStores",
                        principalColumn: "TrackerStoreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackerInsights",
                columns: table => new
                {
                    TrackerInsightsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    NextSuggestedEndTime = table.Column<DateTimeOffset>(nullable: true),
                    NextSuggestedStartTime = table.Column<DateTimeOffset>(nullable: true),
                    TrackerStoreId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackerInsights", x => x.TrackerInsightsId);
                    table.ForeignKey(
                        name: "FK_TrackerInsights_TrackerStores_TrackerStoreId",
                        column: x => x.TrackerStoreId,
                        principalTable: "TrackerStores",
                        principalColumn: "TrackerStoreId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Plot",
                columns: table => new
                {
                    PlotId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ImageData = table.Column<byte[]>(nullable: true),
                    MediaType = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    TimeTableId = table.Column<int>(nullable: false),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plot", x => x.PlotId);
                    table.ForeignKey(
                        name: "FK_Plot_TimeTable_TimeTableId",
                        column: x => x.TimeTableId,
                        principalTable: "TimeTable",
                        principalColumn: "TimeTableId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SharedTimeTableInfo",
                columns: table => new
                {
                    SharedTimeTableInfoId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsEditAllowed = table.Column<bool>(nullable: false),
                    IsReshareAllowed = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    OwnerTrackerStoreId = table.Column<string>(nullable: false),
                    ShareEnabledTime = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    SharedWithTrackerStoreId = table.Column<string>(nullable: false),
                    TimeTableId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedTimeTableInfo", x => x.SharedTimeTableInfoId);
                    table.ForeignKey(
                        name: "FK_SharedTimeTableInfo_TrackerStores_OwnerTrackerStoreId",
                        column: x => x.OwnerTrackerStoreId,
                        principalTable: "TrackerStores",
                        principalColumn: "TrackerStoreId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedTimeTableInfo_TrackerStores_SharedWithTrackerStoreId",
                        column: x => x.SharedWithTrackerStoreId,
                        principalTable: "TrackerStores",
                        principalColumn: "TrackerStoreId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedTimeTableInfo_TimeTable_TimeTableId",
                        column: x => x.TimeTableId,
                        principalTable: "TimeTable",
                        principalColumn: "TimeTableId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeRange",
                columns: table => new
                {
                    TimeRangeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Days = table.Column<int>(nullable: false),
                    TimeTableId = table.Column<int>(nullable: false),
                    WithinDayTimeSpan = table.Column<TimeSpan>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeRange", x => x.TimeRangeId);
                    table.ForeignKey(
                        name: "FK_TimeRange_TimeTable_TimeTableId",
                        column: x => x.TimeTableId,
                        principalTable: "TimeTable",
                        principalColumn: "TimeTableId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkUnit",
                columns: table => new
                {
                    WorkUnitId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AbsoluteOfUtcOffsetAgenda = table.Column<TimeSpan>(nullable: false),
                    ActiveTrackerUserDefaultsId = table.Column<int>(nullable: true),
                    IsDisplayAgendaTimeZone = table.Column<bool>(nullable: false),
                    IsNegativeUtcOffsetAgenda = table.Column<bool>(nullable: false),
                    ManualSortOrderKey = table.Column<int>(nullable: false, defaultValueSql: "NEXT VALUE FOR WorkUnitOrder"),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    TimeTableId = table.Column<int>(nullable: false),
                    TimeZoneIdAgenda = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkUnit", x => x.WorkUnitId);
                    table.ForeignKey(
                        name: "FK_WorkUnit_TimeTable_TimeTableId",
                        column: x => x.TimeTableId,
                        principalTable: "TimeTable",
                        principalColumn: "TimeTableId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeNorm",
                columns: table => new
                {
                    TimeNormId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ColorB = table.Column<int>(nullable: false),
                    ColorG = table.Column<int>(nullable: false),
                    ColorR = table.Column<int>(nullable: false),
                    EndTimeId = table.Column<int>(nullable: true),
                    ManualSortOrderKey = table.Column<int>(nullable: false, defaultValueSql: "NEXT VALUE FOR TimeNormOrder"),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    ProductivityRatingId = table.Column<int>(nullable: true),
                    StartTimeId = table.Column<int>(nullable: true),
                    WorkUnitId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeNorm", x => x.TimeNormId);
                    table.ForeignKey(
                        name: "FK_TimeNorm_TimeStamp_EndTimeId",
                        column: x => x.EndTimeId,
                        principalTable: "TimeStamp",
                        principalColumn: "TimeStampId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeNorm_TimeStamp_StartTimeId",
                        column: x => x.StartTimeId,
                        principalTable: "TimeStamp",
                        principalColumn: "TimeStampId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeNorm_WorkUnit_WorkUnitId",
                        column: x => x.WorkUnitId,
                        principalTable: "WorkUnit",
                        principalColumn: "WorkUnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackerUserDefaults",
                columns: table => new
                {
                    TrackerUserDefaultsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TargetWorkUnitId = table.Column<int>(nullable: true),
                    TimeZoneId = table.Column<string>(nullable: false, defaultValue: "UTC"),
                    TrackerStoreId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackerUserDefaults", x => x.TrackerUserDefaultsId);
                    table.ForeignKey(
                        name: "FK_TrackerUserDefaults_WorkUnit_TargetWorkUnitId",
                        column: x => x.TargetWorkUnitId,
                        principalTable: "WorkUnit",
                        principalColumn: "WorkUnitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrackerUserDefaults_TrackerStores_TrackerStoreId",
                        column: x => x.TrackerStoreId,
                        principalTable: "TrackerStores",
                        principalColumn: "TrackerStoreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductivityRating",
                columns: table => new
                {
                    ProductivityRatingId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    ProductivityPercentage = table.Column<double>(type: "float(53)", nullable: false),
                    TimeNormId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductivityRating", x => x.ProductivityRatingId);
                    table.ForeignKey(
                        name: "FK_ProductivityRating_TimeNorm_TimeNormId",
                        column: x => x.TimeNormId,
                        principalTable: "TimeNorm",
                        principalColumn: "TimeNormId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeNormTagMapping",
                columns: table => new
                {
                    TimeNormTagMappingId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TimeNormId = table.Column<int>(nullable: false),
                    TimeNormTagId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeNormTagMapping", x => x.TimeNormTagMappingId);
                    table.ForeignKey(
                        name: "FK_TimeNormTagMapping_TimeNorm_TimeNormId",
                        column: x => x.TimeNormId,
                        principalTable: "TimeNorm",
                        principalColumn: "TimeNormId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimeNormTagMapping_TimeNormTag_TimeNormTagId",
                        column: x => x.TimeNormTagId,
                        principalTable: "TimeNormTag",
                        principalColumn: "TimeNormTagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Plot_TimeTableId",
                table: "Plot",
                column: "TimeTableId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductivityRating_TimeNormId",
                table: "ProductivityRating",
                column: "TimeNormId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SharedTimeTableInfo_OwnerTrackerStoreId",
                table: "SharedTimeTableInfo",
                column: "OwnerTrackerStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedTimeTableInfo_SharedWithTrackerStoreId",
                table: "SharedTimeTableInfo",
                column: "SharedWithTrackerStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedTimeTableInfo_TimeTableId_SharedWithTrackerStoreId",
                table: "SharedTimeTableInfo",
                columns: new[] { "TimeTableId", "SharedWithTrackerStoreId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeNorm_EndTimeId",
                table: "TimeNorm",
                column: "EndTimeId",
                unique: true,
                filter: "[EndTimeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TimeNorm_StartTimeId",
                table: "TimeNorm",
                column: "StartTimeId",
                unique: true,
                filter: "[StartTimeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TimeNorm_WorkUnitId",
                table: "TimeNorm",
                column: "WorkUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeNormTag_TrackerStoreId",
                table: "TimeNormTag",
                column: "TrackerStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeNormTagMapping_TimeNormTagId",
                table: "TimeNormTagMapping",
                column: "TimeNormTagId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeNormTagMapping_TimeNormId_TimeNormTagId",
                table: "TimeNormTagMapping",
                columns: new[] { "TimeNormId", "TimeNormTagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeRange_TimeTableId",
                table: "TimeRange",
                column: "TimeTableId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeStamp_TrackerStoreId",
                table: "TimeStamp",
                column: "TrackerStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTable_TrackerStoreId",
                table: "TimeTable",
                column: "TrackerStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackerInsights_TrackerStoreId",
                table: "TrackerInsights",
                column: "TrackerStoreId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackerUserDefaults_TargetWorkUnitId",
                table: "TrackerUserDefaults",
                column: "TargetWorkUnitId",
                unique: true,
                filter: "[TargetWorkUnitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrackerUserDefaults_TrackerStoreId",
                table: "TrackerUserDefaults",
                column: "TrackerStoreId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkUnit_TimeTableId",
                table: "WorkUnit",
                column: "TimeTableId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Plot");

            migrationBuilder.DropTable(
                name: "ProductivityRating");

            migrationBuilder.DropTable(
                name: "SharedTimeTableInfo");

            migrationBuilder.DropTable(
                name: "TimeNormTagMapping");

            migrationBuilder.DropTable(
                name: "TimeRange");

            migrationBuilder.DropTable(
                name: "TrackerInsights");

            migrationBuilder.DropTable(
                name: "TrackerUserDefaults");

            migrationBuilder.DropTable(
                name: "TimeNorm");

            migrationBuilder.DropTable(
                name: "TimeNormTag");

            migrationBuilder.DropTable(
                name: "TimeStamp");

            migrationBuilder.DropTable(
                name: "WorkUnit");

            migrationBuilder.DropTable(
                name: "TimeTable");

            migrationBuilder.DropTable(
                name: "TrackerStores");

            migrationBuilder.DropSequence(
                name: "TimeNormOrder");

            migrationBuilder.DropSequence(
                name: "WorkUnitOrder");
        }
    }
}
