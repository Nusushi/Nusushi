//#define SQLITE
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Tokyo.Service.Models;
using Tokyo.Service.Models.Core;

namespace Tokyo.Service.Data
{
    public class TokyoDbContext : DbContext
    {
        public TokyoDbContext(DbContextOptions<TokyoDbContext> options)
            : base(options)
        {

        }

        public DbSet<TrackerStore> TrackerStores { get; set; }
        public DbSet<TrackerEventLog> TrackerEventLogs { get; set; } // TODO rework for performance

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
#if !SQLITE
            const string workUnitOrderSequence = "WorkUnitOrder";
            modelBuilder.HasSequence<int>(workUnitOrderSequence);
            const string timeNormOrderSequence = "TimeNormOrder";
            modelBuilder.HasSequence<int>(timeNormOrderSequence);
#endif

            modelBuilder.Entity<TrackerUserDefaults>(b =>
            {
                b.Property(t => t.TimeZoneId)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValue("UTC");
            });

            modelBuilder.Entity<Plot>(b =>
            {
                b.HasOne(p => p.TimeTable)
                    .WithOne(t => t.Plot)
                    .HasForeignKey<Plot>(p => p.TimeTableId);
            });
            modelBuilder.Entity<TimeStamp>(b =>
            {
                // TODO make a calculation for expected number of entries => bigint? or just GUID

                b.HasOne(t => t.BoundNormStart)
                    .WithOne(t => t.StartTime)
                    .HasForeignKey<TimeNorm>(t => t.StartTimeId);

                b.HasOne(t => t.BoundNormEnd)
                    .WithOne(t => t.EndTime)
                    .HasForeignKey<TimeNorm>(t => t.EndTimeId);
#if !SQLITE
                b.Property(t => t.TrackedTimeUTC)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("SYSDATETIMEOFFSET() AT TIME ZONE 'UTC'");
#endif

                b.Property(t => t.TimeZoneIdAtCreation)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("'UTC'");
            });
            modelBuilder.Entity<WorkUnit>(b =>
            {
#if !SQLITE
                b.Property(t => t.ManualSortOrderKey)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql($"NEXT VALUE FOR {workUnitOrderSequence}");
#endif
                b.HasOne(t => t.ActiveTrackerUserDefaults)
                    .WithOne(t => t.TargetWorkUnit)
                    .HasForeignKey<TrackerUserDefaults>(t => t.TargetWorkUnitId);
            });
            modelBuilder.Entity<SharedTimeTableInfo>(b =>
            {
                b.HasOne(t => t.TimeTable)
                    .WithMany(t => t.SharedTimeTableInfos)
                    .HasForeignKey(t => t.TimeTableId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(t => t.OwnerTrackerStore)
                    .WithMany(t => t.OwnedSharedTimeTableInfos)
                    .HasForeignKey(t => t.OwnerTrackerStoreId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(t => t.SharedWithTrackerStore)
                    .WithMany(t => t.ForeignSharedTimeTableInfos)
                    .HasForeignKey(t => t.SharedWithTrackerStoreId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.Property(t => t.ShareEnabledTime)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                b.HasIndex(t => new { t.TimeTableId, t.SharedWithTrackerStoreId })
                    .IsUnique();
            });

            modelBuilder.Entity<TimeNormTagMapping>(b =>
            {
                b.HasOne(t => t.TimeNorm)
                    .WithMany(t => t.TimeNormTagMappings)
                    .HasForeignKey(t => t.TimeNormId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(t => t.TimeNormTag)
                    .WithMany(t => t.TimeNormTagMappings)
                    .HasForeignKey(t => t.TimeNormTagId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(t => new { t.TimeNormId, t.TimeNormTagId })
                    .IsUnique();
            });

            modelBuilder.Entity<TimeNorm>(b =>
            {
#if !SQLITE
                b.Property(t => t.ManualSortOrderKey)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql($"NEXT VALUE FOR {timeNormOrderSequence}");
#endif

                b.HasOne(t => t.ProductivityRating)
                    .WithOne(t => t.TimeNorm)
                    .HasForeignKey<ProductivityRating>(t => t.TimeNormId);
            });

            modelBuilder.Entity<TimeTable>(b =>
            {
                b.HasOne(t => t.TargetWeeklyTime)
                    .WithOne(w => w.TimeTable)
                    .HasForeignKey<TimeRange>(t => t.TimeTableId);
            });

            modelBuilder.Entity<TrackerStore>(b =>
            {
                b.HasOne(t => t.TrackerUserDefaults)
                    .WithOne(t => t.TrackerStore)
                    .HasForeignKey<TrackerUserDefaults>(t => t.TrackerStoreId);

                b.HasOne(t => t.TrackerInsights)
                    .WithOne(t => t.TrackerStore)
                    .HasForeignKey<TrackerInsights>(t => t.TrackerStoreId)
                    .OnDelete(DeleteBehavior.Restrict); // TODO valuable data => cleanup job

                b.HasMany(t => t.TimeNormTags)
                    .WithOne(t => t.TrackerStore)
                    .HasForeignKey(t => t.TrackerStoreId)
                    .OnDelete(DeleteBehavior.Restrict); // TODO set because multiple cascade paths, must be deleted manually!?
            });
        }
    }
}
