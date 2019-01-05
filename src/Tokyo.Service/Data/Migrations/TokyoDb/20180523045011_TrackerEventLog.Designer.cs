﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using Tokyo.Service.Data;
using Tokyo.Service.Models.Core;

namespace Tokyo.Service.Migrations
{
    [DbContext(typeof(TokyoDbContext))]
    [Migration("20180523045011_TrackerEventLog")]
    partial class TrackerEventLog
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("Relational:Sequence:.TimeNormOrder", "'TimeNormOrder', '', '1', '1', '', '', 'Int32', 'False'")
                .HasAnnotation("Relational:Sequence:.WorkUnitOrder", "'WorkUnitOrder', '', '1', '1', '', '', 'Int32', 'False'")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Tokyo.Service.Models.Core.Plot", b =>
                {
                    b.Property<int>("PlotId")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("ImageData");

                    b.Property<string>("MediaType");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int>("TimeTableId");

                    b.Property<byte[]>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("PlotId");

                    b.HasIndex("TimeTableId")
                        .IsUnique();

                    b.ToTable("Plot");
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.ProductivityRating", b =>
                {
                    b.Property<int>("ProductivityRatingId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<double>("ProductivityPercentage")
                        .HasColumnType("float(53)");

                    b.Property<int>("TimeNormId");

                    b.HasKey("ProductivityRatingId");

                    b.HasIndex("TimeNormId")
                        .IsUnique();

                    b.ToTable("ProductivityRating");
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.SharedTimeTableInfo", b =>
                {
                    b.Property<int>("SharedTimeTableInfoId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool?>("IsEditAllowed")
                        .IsRequired();

                    b.Property<bool?>("IsReshareAllowed")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("OwnerTrackerStoreId")
                        .IsRequired();

                    b.Property<DateTimeOffset>("ShareEnabledTime")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("SYSUTCDATETIME()");

                    b.Property<string>("SharedWithTrackerStoreId")
                        .IsRequired();

                    b.Property<int>("TimeTableId");

                    b.HasKey("SharedTimeTableInfoId");

                    b.HasIndex("OwnerTrackerStoreId");

                    b.HasIndex("SharedWithTrackerStoreId");

                    b.HasIndex("TimeTableId", "SharedWithTrackerStoreId")
                        .IsUnique();

                    b.ToTable("SharedTimeTableInfo");
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.TimeNorm", b =>
                {
                    b.Property<int>("TimeNormId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ColorB");

                    b.Property<int>("ColorG");

                    b.Property<int>("ColorR");

                    b.Property<int?>("EndTimeId");

                    b.Property<int>("ManualSortOrderKey")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("NEXT VALUE FOR TimeNormOrder");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int?>("ProductivityRatingId");

                    b.Property<int?>("StartTimeId");

                    b.Property<int>("WorkUnitId");

                    b.HasKey("TimeNormId");

                    b.HasIndex("EndTimeId")
                        .IsUnique()
                        .HasFilter("[EndTimeId] IS NOT NULL");

                    b.HasIndex("StartTimeId")
                        .IsUnique()
                        .HasFilter("[StartTimeId] IS NOT NULL");

                    b.HasIndex("WorkUnitId");

                    b.ToTable("TimeNorm");
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.TimeNormTag", b =>
                {
                    b.Property<int>("TimeNormTagId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("TrackerStoreId")
                        .IsRequired();

                    b.HasKey("TimeNormTagId");

                    b.HasIndex("TrackerStoreId");

                    b.ToTable("TimeNormTag");
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.TimeNormTagMapping", b =>
                {
                    b.Property<int>("TimeNormTagMappingId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("TimeNormId");

                    b.Property<int>("TimeNormTagId");

                    b.HasKey("TimeNormTagMappingId");

                    b.HasIndex("TimeNormTagId");

                    b.HasIndex("TimeNormId", "TimeNormTagId")
                        .IsUnique();

                    b.ToTable("TimeNormTagMapping");
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.TimeRange", b =>
                {
                    b.Property<int>("TimeRangeId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Days");

                    b.Property<int>("TimeTableId");

                    b.Property<TimeSpan>("WithinDayTimeSpan");

                    b.HasKey("TimeRangeId");

                    b.HasIndex("TimeTableId")
                        .IsUnique();

                    b.ToTable("TimeRange");
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.TimeStamp", b =>
                {
                    b.Property<int>("TimeStampId")
                        .ValueGeneratedOnAdd();

                    b.Property<TimeSpan>("AbsoluteOfUtcOffset");

                    b.Property<bool>("IsNegativeUtcOffset");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("TimeZoneIdAtCreation")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("'UTC'")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("TrackedTimeUTC")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("SYSDATETIMEOFFSET() AT TIME ZONE 'UTC'");

                    b.Property<string>("TrackerStoreId")
                        .IsRequired();

                    b.HasKey("TimeStampId");

                    b.HasIndex("TrackerStoreId");

                    b.ToTable("TimeStamp");
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.TimeTable", b =>
                {
                    b.Property<int>("TimeTableId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsFrozen");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int?>("TargetWeeklyTimeId");

                    b.Property<string>("TrackerStoreId")
                        .IsRequired();

                    b.HasKey("TimeTableId");

                    b.HasIndex("TrackerStoreId");

                    b.ToTable("TimeTable");
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.TrackerInsights", b =>
                {
                    b.Property<int>("TrackerInsightsId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset?>("NextSuggestedEndTime");

                    b.Property<DateTimeOffset?>("NextSuggestedStartTime");

                    b.Property<string>("TrackerStoreId")
                        .IsRequired();

                    b.HasKey("TrackerInsightsId");

                    b.HasIndex("TrackerStoreId")
                        .IsUnique();

                    b.ToTable("TrackerInsights");
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.TrackerUserDefaults", b =>
                {
                    b.Property<int>("TrackerUserDefaultsId")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("TargetWorkUnitId");

                    b.Property<string>("TimeZoneId")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue("UTC");

                    b.Property<string>("TrackerStoreId")
                        .IsRequired();

                    b.HasKey("TrackerUserDefaultsId");

                    b.HasIndex("TargetWorkUnitId")
                        .IsUnique()
                        .HasFilter("[TargetWorkUnitId] IS NOT NULL");

                    b.HasIndex("TrackerStoreId")
                        .IsUnique();

                    b.ToTable("TrackerUserDefaults");
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.WorkUnit", b =>
                {
                    b.Property<int>("WorkUnitId")
                        .ValueGeneratedOnAdd();

                    b.Property<TimeSpan>("AbsoluteOfUtcOffsetAgenda");

                    b.Property<int?>("ActiveTrackerUserDefaultsId");

                    b.Property<bool>("IsDisplayAgendaTimeZone");

                    b.Property<bool>("IsNegativeUtcOffsetAgenda");

                    b.Property<int>("ManualSortOrderKey")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("NEXT VALUE FOR WorkUnitOrder");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int>("TimeTableId");

                    b.Property<string>("TimeZoneIdAgenda")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.HasKey("WorkUnitId");

                    b.HasIndex("TimeTableId");

                    b.ToTable("WorkUnit");
                });

            modelBuilder.Entity("Tokyo.Service.Models.TrackerEventLog", b =>
                {
                    b.Property<int>("TrackerEventLogId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("NTimesCalled");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<int>("TrackerEvent");

                    b.HasKey("TrackerEventLogId");

                    b.ToTable("TrackerEventLogs");
                });

            modelBuilder.Entity("Tokyo.Service.Models.TrackerStore", b =>
                {
                    b.Property<string>("TrackerStoreId");

                    b.Property<int>("TrackerInsightsId");

                    b.Property<int>("TrackerUserDefaultsId");

                    b.HasKey("TrackerStoreId");

                    b.ToTable("TrackerStores");
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.Plot", b =>
                {
                    b.HasOne("Tokyo.Service.Models.Core.TimeTable", "TimeTable")
                        .WithOne("Plot")
                        .HasForeignKey("Tokyo.Service.Models.Core.Plot", "TimeTableId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.ProductivityRating", b =>
                {
                    b.HasOne("Tokyo.Service.Models.Core.TimeNorm", "TimeNorm")
                        .WithOne("ProductivityRating")
                        .HasForeignKey("Tokyo.Service.Models.Core.ProductivityRating", "TimeNormId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.SharedTimeTableInfo", b =>
                {
                    b.HasOne("Tokyo.Service.Models.TrackerStore", "OwnerTrackerStore")
                        .WithMany("OwnedSharedTimeTableInfos")
                        .HasForeignKey("OwnerTrackerStoreId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Tokyo.Service.Models.TrackerStore", "SharedWithTrackerStore")
                        .WithMany("ForeignSharedTimeTableInfos")
                        .HasForeignKey("SharedWithTrackerStoreId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Tokyo.Service.Models.Core.TimeTable", "TimeTable")
                        .WithMany("SharedTimeTableInfos")
                        .HasForeignKey("TimeTableId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.TimeNorm", b =>
                {
                    b.HasOne("Tokyo.Service.Models.Core.TimeStamp", "EndTime")
                        .WithOne("BoundNormEnd")
                        .HasForeignKey("Tokyo.Service.Models.Core.TimeNorm", "EndTimeId");

                    b.HasOne("Tokyo.Service.Models.Core.TimeStamp", "StartTime")
                        .WithOne("BoundNormStart")
                        .HasForeignKey("Tokyo.Service.Models.Core.TimeNorm", "StartTimeId");

                    b.HasOne("Tokyo.Service.Models.Core.WorkUnit", "WorkUnit")
                        .WithMany("TimeNorms")
                        .HasForeignKey("WorkUnitId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.TimeNormTag", b =>
                {
                    b.HasOne("Tokyo.Service.Models.TrackerStore", "TrackerStore")
                        .WithMany("TimeNormTags")
                        .HasForeignKey("TrackerStoreId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.TimeNormTagMapping", b =>
                {
                    b.HasOne("Tokyo.Service.Models.Core.TimeNorm", "TimeNorm")
                        .WithMany("TimeNormTagMappings")
                        .HasForeignKey("TimeNormId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Tokyo.Service.Models.Core.TimeNormTag", "TimeNormTag")
                        .WithMany("TimeNormTagMappings")
                        .HasForeignKey("TimeNormTagId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.TimeRange", b =>
                {
                    b.HasOne("Tokyo.Service.Models.Core.TimeTable", "TimeTable")
                        .WithOne("TargetWeeklyTime")
                        .HasForeignKey("Tokyo.Service.Models.Core.TimeRange", "TimeTableId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.TimeStamp", b =>
                {
                    b.HasOne("Tokyo.Service.Models.TrackerStore")
                        .WithMany("TimeStamps")
                        .HasForeignKey("TrackerStoreId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.TimeTable", b =>
                {
                    b.HasOne("Tokyo.Service.Models.TrackerStore", "TrackerStore")
                        .WithMany("TimeTables")
                        .HasForeignKey("TrackerStoreId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.TrackerInsights", b =>
                {
                    b.HasOne("Tokyo.Service.Models.TrackerStore", "TrackerStore")
                        .WithOne("TrackerInsights")
                        .HasForeignKey("Tokyo.Service.Models.Core.TrackerInsights", "TrackerStoreId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.TrackerUserDefaults", b =>
                {
                    b.HasOne("Tokyo.Service.Models.Core.WorkUnit", "TargetWorkUnit")
                        .WithOne("ActiveTrackerUserDefaults")
                        .HasForeignKey("Tokyo.Service.Models.Core.TrackerUserDefaults", "TargetWorkUnitId");

                    b.HasOne("Tokyo.Service.Models.TrackerStore", "TrackerStore")
                        .WithOne("TrackerUserDefaults")
                        .HasForeignKey("Tokyo.Service.Models.Core.TrackerUserDefaults", "TrackerStoreId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Tokyo.Service.Models.Core.WorkUnit", b =>
                {
                    b.HasOne("Tokyo.Service.Models.Core.TimeTable", "TimeTable")
                        .WithMany("WorkUnits")
                        .HasForeignKey("TimeTableId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
