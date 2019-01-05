//#define SQLITE
using California.Creator.Service.Models;
using California.Creator.Service.Models.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace California.Creator.Service.Data
{
    public class CaliforniaDbContext : DbContext
    {
        public CaliforniaDbContext(DbContextOptions<CaliforniaDbContext> options)
            : base(options)
        {
            
        }

        public DbSet<CaliforniaStore> CaliforniaStores { get; set; }
        public DbSet<CaliforniaEventLog> CaliforniaEventLogs { get; set; } // TODO rework for performance
        public DbSet<Webfont> Webfonts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) // TEST SET READ_COMMITTED_SNAPSHOT ON; is default value
        {
            base.OnModelCreating(modelBuilder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
#if !SQLITE
            const string layoutBaseOrderSequence = "LayoutBaseOrder";
            modelBuilder.HasSequence<int>(layoutBaseOrderSequence);
            const string viewOrderSequence = "ViewUnitOrder";
            modelBuilder.HasSequence<int>(viewOrderSequence);
#endif

            modelBuilder.Entity<CaliforniaStore>(b =>
            {
                b.HasOne(s => s.CaliforniaUserDefaults)
                    .WithOne(u => u.CaliforniaStore)
                    .HasForeignKey<CaliforniaUserDefaults>(s => s.CaliforniaStoreId);
            });

            modelBuilder.Entity<CaliforniaProject>(b =>
            {
                b.HasMany(project => project.StyleValues)
                    .WithOne(value => value.CaliforniaProject)
                    .HasForeignKey(value => value.CaliforniaProjectId)
                    .OnDelete(DeleteBehavior.Restrict); // reference for authorization

                b.HasMany(project => project.StyleQuantums)
                    .WithOne(quantum => quantum.CaliforniaProject)
                    .HasForeignKey(quantum => quantum.CaliforniaProjectId)
                    .OnDelete(DeleteBehavior.Restrict); // many-to-many relationship (mapping) creates multiple cascade paths

                b.HasMany(project => project.StyleAtoms)
                    .WithOne(atom => atom.CaliforniaProject)
                    .HasForeignKey(atom => atom.CaliforniaProjectId)
                    .OnDelete(DeleteBehavior.Restrict); // reference for authorization

                b.HasMany(project => project.StyleMolecules)
                    .WithOne(mol => mol.CaliforniaProject)
                    .HasForeignKey(mol => mol.CaliforniaProjectId)
                    .OnDelete(DeleteBehavior.Restrict); // reference for authorization

                b.HasMany(project => project.LayoutMolecules)
                    .WithOne(layoutBase => layoutBase.CaliforniaProject)
                    .HasForeignKey(layoutBase => layoutBase.CaliforniaProjectId)
                    .OnDelete(DeleteBehavior.Restrict); // reference for authorization

                b.HasMany(project => project.LayoutStyleInteractions)
                    .WithOne(mol => mol.CaliforniaProject)
                    .HasForeignKey(mol => mol.CaliforniaProjectId)
                    .OnDelete(DeleteBehavior.Restrict); // reference for client

                b.HasMany(project => project.ResponsiveDevices)
                    .WithOne(device => device.CaliforniaProject)
                    .HasForeignKey(device => device.CaliforniaProjectId)
                    .OnDelete(DeleteBehavior.Restrict); // many-to-many relationship (mapping) creates multiple cascade paths

                b.HasMany(project => project.ContentAtoms)
                    .WithOne(contentAtom => contentAtom.CaliforniaProject)
                    .HasForeignKey(contentAtom => contentAtom.CaliforniaProjectId)
                    .OnDelete(DeleteBehavior.Restrict); // content delete is protected with additional relationship to dbLock

                b.HasMany(project => project.PictureContents)
                    .WithOne(picture => picture.CaliforniaProject)
                    .HasForeignKey(picture => picture.CaliforniaProjectId);

                b.HasMany(project => project.CaliforniaViews)
                    .WithOne(view => view.CaliforniaProject)
                    .HasForeignKey(view => view.CaliforniaProjectId);
            });

            modelBuilder.Entity<StyleAtomQuantumMapping>(b =>
            {
                b.HasOne(mapping => mapping.StyleAtom)
                    .WithMany(atom => atom.MappedQuantums)
                    .HasForeignKey(mapping => mapping.StyleAtomId);

                b.HasOne(mapping => mapping.StyleQuantum)
                    .WithMany(quantum => quantum.MappedToAtoms)
                    .HasForeignKey(mapping => mapping.StyleQuantumId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(mapping => new { mapping.StyleQuantumId, mapping.StyleAtomId })
                    .IsUnique();
            });

            modelBuilder.Entity<StyleValueInteractionMapping>(b =>
            {
                b.HasOne(mapping => mapping.LayoutStyleInteraction)
                    .WithMany(interaction => interaction.StyleValueInteractions)
                    .HasForeignKey(mapping => mapping.LayoutStyleInteractionId);

                b.HasOne(mapping => mapping.StyleValue)
                    .WithMany(value => value.AppliedStyleValueInteractions)
                    .HasForeignKey(mapping => mapping.StyleValueId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(mapping => new { mapping.LayoutStyleInteractionId, mapping.StyleValueId })
                    .IsUnique();
            });

            modelBuilder.Entity<StyleMoleculeAtomMapping>(b =>
            {
                b.HasOne(mapping => mapping.StyleMolecule)
                    .WithMany(molecule => molecule.MappedStyleAtoms)
                    .HasForeignKey(mapping => mapping.StyleMoleculeId);

                b.HasOne(mapping => mapping.StyleAtom)
                    .WithOne(atom => atom.MappedToMolecule)
                    .HasForeignKey<StyleAtom>(atom => atom.MappedToMoleculeId);

                b.HasOne(mapping => mapping.ResponsiveDevice)
                    .WithMany(device => device.AppliedToMappings)
                    .HasForeignKey(mapping => mapping.ResponsiveDeviceId);
            });

            modelBuilder.Entity<CaliforniaView>(b =>
            {
#if !SQLITE
                b.Property(view => view.ViewSortOrderKey)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql($"NEXT VALUE FOR {viewOrderSequence}");
#endif

                b.HasMany(view => view.HostedByLayoutMappings)
                    .WithOne(map => map.CaliforniaView)
                    .HasForeignKey(map => map.CaliforniaViewId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(view => view.PlacedLayoutRows)
                    .WithOne(layoutRow => layoutRow.PlacedOnView)
                    .HasForeignKey(layoutRow => layoutRow.PlacedOnViewId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PictureContent>(b =>
            {
                b.HasMany(pic => pic.DisplayedOnAtoms)
                    .WithOne(contentAtom => contentAtom.PictureContent)
                    .HasForeignKey(contentAtom => contentAtom.PictureContentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StyleMolecule>(b =>
            {
                b.HasOne(styleMol => styleMol.ClonedFromStyle)
                    .WithMany(cloneChildStyleMol => cloneChildStyleMol.CloneOfStyles)
                    .HasForeignKey(styleMol => styleMol.ClonedFromStyleId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(styleMol => styleMol.StyleForLayout)
                    .WithOne(layoutBase => layoutBase.StyleMolecule)
                    .HasForeignKey<StyleMolecule>(styleMol => styleMol.StyleForLayoutId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ContentAtom>(b =>
            {
                b.HasOne(contentAtom => contentAtom.DbContentSafetyLock)
                    .WithOne(contentSafetyLock => contentSafetyLock.ContentAtom)
                    .HasForeignKey<DbContentSafetyLock>(safetyLock => safetyLock.ContentAtomId)
                    .HasConstraintName("FK_ContentAtom_RemoveIsNotSafe_RemoveIsNotSafeId")
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(contentAtom => contentAtom.InstancedOnLayout)
                    .WithOne(layoutAtom => layoutAtom.HostedContentAtom)
                    .HasForeignKey<ContentAtom>(contentAtom => contentAtom.InstancedOnLayoutId)
                    .OnDelete(DeleteBehavior.Restrict);

                // b.HasAlternateKey TEST can we save duplicate content with some ef core function (long key? or hash on text) TODO legal can measure if already exists by speed
                b.HasQueryFilter(contentAtom => !contentAtom.IsDeleted);
            });

            modelBuilder.Entity<ResponsiveDevice>(b =>
            {
                // b.HasIndex(device => { device.$(Every Property) }).IsUnique() TEST can we save tons of duplicate content only question is TEST legal consent of user and TEST storage vs. requests for existing check
            });

            modelBuilder.Entity<LayoutAtom>(b =>
            {
#if !SQLITE
                b.Property(layoutAtom => layoutAtom.LayoutSortOrderKey)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql($"NEXT VALUE FOR {layoutBaseOrderSequence}");
#endif
            });

            modelBuilder.Entity<LayoutBox>(b =>
            {
#if !SQLITE
                b.Property(layoutBox => layoutBox.LayoutSortOrderKey)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql($"NEXT VALUE FOR {layoutBaseOrderSequence}");
#endif

                b.HasMany(box => box.HostedViewMappings)
                    .WithOne(map => map.LayoutBox)
                    .HasForeignKey(map => map.LayoutBoxId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<LayoutRow>(b =>
            {
#if !SQLITE
                b.Property(layoutRow => layoutRow.LayoutSortOrderKey)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql($"NEXT VALUE FOR {layoutBaseOrderSequence}");
#endif
            });

            modelBuilder.Entity<LayoutAtom>(b =>
            {
                b.HasOne(layoutAtom => layoutAtom.PlacedAtomInBox)
                    .WithMany(layoutBox => layoutBox.PlacedInBoxAtoms)
                    .HasForeignKey(layoutAtom => layoutAtom.PlacedAtomInBoxId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<LayoutBox>(b =>
            {
                b.HasOne(layoutBox => layoutBox.PlacedBoxInBox)
                    .WithMany(boxContainer => boxContainer.PlacedInBoxBoxes)
                    .HasForeignKey(layoutBox => layoutBox.PlacedBoxInBoxId)
                    .OnDelete(DeleteBehavior.Restrict); // SetNull seems reasonable but overcontrains

                b.HasOne(layoutBox => layoutBox.BoxOwnerRow)
                    .WithMany(rowContainer => rowContainer.AllBoxesBelowRow)
                    .HasForeignKey(layoutBox => layoutBox.BoxOwnerRowId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SharedProjectInfo>(b =>
            {
                b.HasOne(t => t.CaliforniaProject)
                    .WithMany(t => t.SharedProjectInfos)
                    .HasForeignKey(t => t.CaliforniaProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(t => t.OwnerCaliforniaStore)
                    .WithMany(t => t.OwnedSharedProjectInfos)
                    .HasForeignKey(t => t.OwnerCaliforniaStoreId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(t => t.SharedWithCaliforniaStore)
                    .WithMany(t => t.ForeignSharedProjectInfos)
                    .HasForeignKey(t => t.SharedWithCaliforniaStoreId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.Property(t => t.ShareEnabledTime)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                b.HasIndex(t => new { t.CaliforniaProjectId, t.SharedWithCaliforniaStoreId })
                    .IsUnique();
            });
        }
    }
}
