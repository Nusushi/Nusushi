﻿// <auto-generated />
using California.Creator.Service.Data;
using California.Creator.Service.Models.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;

namespace California.Creator.Service.Migrations
{
    [DbContext(typeof(CaliforniaDbContext))]
    [Migration("20180202192804_SharedProjects")]
    partial class SharedProjects
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("Relational:Sequence:.LayoutBaseOrder", "'LayoutBaseOrder', '', '1', '1', '', '', 'Int32', 'False'")
                .HasAnnotation("Relational:Sequence:.ViewUnitOrder", "'ViewUnitOrder', '', '1', '1', '', '', 'Int32', 'False'")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("California.Creator.Service.Models.CaliforniaStore", b =>
                {
                    b.Property<string>("CaliforniaStoreId");

                    b.Property<int>("CaliforniaUserDefaultsId");

                    b.HasKey("CaliforniaStoreId");

                    b.ToTable("CaliforniaStores");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.CaliforniaProject", b =>
                {
                    b.Property<int>("CaliforniaProjectId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CaliforniaStoreId")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.HasKey("CaliforniaProjectId");

                    b.HasIndex("CaliforniaStoreId");

                    b.ToTable("CaliforniaProject");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.CaliforniaUserDefaults", b =>
                {
                    b.Property<int>("CaliforniaUserDefaultsId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CaliforniaStoreId")
                        .IsRequired();

                    b.HasKey("CaliforniaUserDefaultsId");

                    b.HasIndex("CaliforniaStoreId")
                        .IsUnique();

                    b.ToTable("CaliforniaUserDefaults");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.CaliforniaView", b =>
                {
                    b.Property<int>("CaliforniaViewId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CaliforniaProjectId");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("QueryUrl")
                        .IsRequired();

                    b.Property<int>("ViewSortOrderKey")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("NEXT VALUE FOR ViewUnitOrder");

                    b.HasKey("CaliforniaViewId");

                    b.HasIndex("CaliforniaProjectId");

                    b.ToTable("CaliforniaView");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.ContentAtom", b =>
                {
                    b.Property<int>("ContentAtomId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CaliforniaProjectId");

                    b.Property<int?>("CaliforniaViewId");

                    b.Property<int?>("ContentAtomType")
                        .IsRequired();

                    b.Property<DateTimeOffset?>("DeletedDate");

                    b.Property<int?>("InstancedOnLayoutId");

                    b.Property<bool>("IsDeleted");

                    b.Property<int?>("PictureContentId");

                    b.Property<string>("TextContent");

                    b.Property<string>("Url");

                    b.HasKey("ContentAtomId");

                    b.HasIndex("CaliforniaProjectId");

                    b.HasIndex("CaliforniaViewId");

                    b.HasIndex("InstancedOnLayoutId")
                        .IsUnique()
                        .HasFilter("[InstancedOnLayoutId] IS NOT NULL");

                    b.HasIndex("PictureContentId");

                    b.ToTable("ContentAtom");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.LayoutBase", b =>
                {
                    b.Property<int>("LayoutBaseId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CaliforniaProjectId");

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<int>("LayoutSortOrderKey")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("NEXT VALUE FOR LayoutBaseOrder");

                    b.HasKey("LayoutBaseId");

                    b.HasIndex("CaliforniaProjectId");

                    b.ToTable("LayoutBase");

                    b.HasDiscriminator<string>("Discriminator").HasValue("LayoutBase");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.PictureContent", b =>
                {
                    b.Property<int>("PictureContentId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CaliforniaProjectId");

                    b.HasKey("PictureContentId");

                    b.HasIndex("CaliforniaProjectId");

                    b.ToTable("PictureContent");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.ResponsiveDevice", b =>
                {
                    b.Property<int>("ResponsiveDeviceId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CaliforniaProjectId");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("NameShort")
                        .IsRequired()
                        .HasMaxLength(5);

                    b.Property<int?>("WidthThreshold")
                        .IsRequired();

                    b.HasKey("ResponsiveDeviceId");

                    b.HasIndex("CaliforniaProjectId");

                    b.ToTable("ResponsiveDevice");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.SharedProjectInfo", b =>
                {
                    b.Property<int>("SharedProjectInfoId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CaliforniaProjectId");

                    b.Property<bool?>("IsEditAllowed")
                        .IsRequired();

                    b.Property<bool?>("IsReshareAllowed")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("OwnerCaliforniaStoreId")
                        .IsRequired();

                    b.Property<DateTimeOffset>("ShareEnabledTime")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("SYSUTCDATETIME()");

                    b.Property<string>("SharedWithCaliforniaStoreId")
                        .IsRequired();

                    b.HasKey("SharedProjectInfoId");

                    b.HasIndex("OwnerCaliforniaStoreId");

                    b.HasIndex("SharedWithCaliforniaStoreId");

                    b.HasIndex("CaliforniaProjectId", "SharedWithCaliforniaStoreId")
                        .IsUnique();

                    b.ToTable("SharedProjectInfo");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.StyleAtom", b =>
                {
                    b.Property<int>("StyleAtomId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CaliforniaProjectId");

                    b.Property<int>("MappedToMoleculeId");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int?>("StyleAtomType")
                        .IsRequired();

                    b.HasKey("StyleAtomId");

                    b.HasIndex("CaliforniaProjectId");

                    b.HasIndex("MappedToMoleculeId")
                        .IsUnique();

                    b.ToTable("StyleAtom");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.StyleAtomQuantumMapping", b =>
                {
                    b.Property<int>("StyleAtomQuantumMappingId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("StyleAtomId");

                    b.Property<int>("StyleQuantumId");

                    b.HasKey("StyleAtomQuantumMappingId");

                    b.HasIndex("StyleAtomId");

                    b.HasIndex("StyleQuantumId", "StyleAtomId")
                        .IsUnique();

                    b.ToTable("StyleAtomQuantumMapping");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.StyleMolecule", b =>
                {
                    b.Property<int>("StyleMoleculeId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CaliforniaProjectId");

                    b.Property<int?>("ClonedFromStyleId");

                    b.Property<string>("HtmlTag")
                        .HasMaxLength(255);

                    b.Property<bool?>("IsListed")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("NameShort")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int>("StyleForLayoutId");

                    b.HasKey("StyleMoleculeId");

                    b.HasIndex("CaliforniaProjectId");

                    b.HasIndex("ClonedFromStyleId");

                    b.HasIndex("StyleForLayoutId")
                        .IsUnique();

                    b.ToTable("StyleMolecule");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.StyleMoleculeAtomMapping", b =>
                {
                    b.Property<int>("StyleMoleculeAtomMappingId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ResponsiveDeviceId");

                    b.Property<string>("StateModifier")
                        .HasMaxLength(255);

                    b.Property<int>("StyleMoleculeId");

                    b.HasKey("StyleMoleculeAtomMappingId");

                    b.HasIndex("ResponsiveDeviceId");

                    b.HasIndex("StyleMoleculeId");

                    b.ToTable("StyleMoleculeAtomMapping");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.StyleQuantum", b =>
                {
                    b.Property<int>("StyleQuantumId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CaliforniaProjectId");

                    b.Property<string>("CssProperty")
                        .IsRequired();

                    b.Property<string>("CssValue")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.HasKey("StyleQuantumId");

                    b.HasIndex("CaliforniaProjectId");

                    b.ToTable("StyleQuantum");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.StyleValue", b =>
                {
                    b.Property<int>("StyleValueId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CaliforniaProjectId");

                    b.Property<string>("CssProperty")
                        .IsRequired();

                    b.Property<string>("CssValue")
                        .IsRequired();

                    b.Property<int>("StyleAtomId");

                    b.HasKey("StyleValueId");

                    b.HasIndex("CaliforniaProjectId");

                    b.HasIndex("StyleAtomId");

                    b.ToTable("StyleValue");
                });

            modelBuilder.Entity("California.Creator.Service.Models.DbContentSafetyLock", b =>
                {
                    b.Property<int>("DbContentSafetyLockId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ContentAtomId");

                    b.HasKey("DbContentSafetyLockId");

                    b.HasIndex("ContentAtomId")
                        .IsUnique();

                    b.ToTable("DbContentSafetyLock");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.LayoutAtom", b =>
                {
                    b.HasBaseType("California.Creator.Service.Models.Core.LayoutBase");

                    b.Property<int>("PlacedAtomInBoxId");

                    b.HasIndex("PlacedAtomInBoxId");

                    b.ToTable("LayoutAtom");

                    b.HasDiscriminator().HasValue("LayoutAtom");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.LayoutBox", b =>
                {
                    b.HasBaseType("California.Creator.Service.Models.Core.LayoutBase");

                    b.Property<int>("BoxOwnerRowId");

                    b.Property<int?>("PlacedBoxInBoxId");

                    b.HasIndex("BoxOwnerRowId");

                    b.HasIndex("PlacedBoxInBoxId");

                    b.ToTable("LayoutBox");

                    b.HasDiscriminator().HasValue("LayoutBox");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.LayoutRow", b =>
                {
                    b.HasBaseType("California.Creator.Service.Models.Core.LayoutBase");

                    b.Property<int?>("ClonedFromLayoutId");

                    b.Property<int?>("HostedViewId");

                    b.Property<bool?>("IsInstanceable")
                        .IsRequired();

                    b.Property<int>("PlacedOnViewId");

                    b.HasIndex("ClonedFromLayoutId");

                    b.HasIndex("HostedViewId");

                    b.HasIndex("PlacedOnViewId");

                    b.ToTable("LayoutRow");

                    b.HasDiscriminator().HasValue("LayoutRow");
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.CaliforniaProject", b =>
                {
                    b.HasOne("California.Creator.Service.Models.CaliforniaStore", "CaliforniaStore")
                        .WithMany("CaliforniaProjects")
                        .HasForeignKey("CaliforniaStoreId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.CaliforniaUserDefaults", b =>
                {
                    b.HasOne("California.Creator.Service.Models.CaliforniaStore", "CaliforniaStore")
                        .WithOne("CaliforniaUserDefaults")
                        .HasForeignKey("California.Creator.Service.Models.Core.CaliforniaUserDefaults", "CaliforniaStoreId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.CaliforniaView", b =>
                {
                    b.HasOne("California.Creator.Service.Models.Core.CaliforniaProject", "CaliforniaProject")
                        .WithMany("CaliforniaViews")
                        .HasForeignKey("CaliforniaProjectId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.ContentAtom", b =>
                {
                    b.HasOne("California.Creator.Service.Models.Core.CaliforniaProject", "CaliforniaProject")
                        .WithMany("ContentAtoms")
                        .HasForeignKey("CaliforniaProjectId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("California.Creator.Service.Models.Core.CaliforniaView", "CaliforniaView")
                        .WithMany("ViewTriggerAtoms")
                        .HasForeignKey("CaliforniaViewId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("California.Creator.Service.Models.Core.LayoutAtom", "InstancedOnLayout")
                        .WithOne("HostedContentAtom")
                        .HasForeignKey("California.Creator.Service.Models.Core.ContentAtom", "InstancedOnLayoutId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("California.Creator.Service.Models.Core.PictureContent", "PictureContent")
                        .WithMany("DisplayedOnAtoms")
                        .HasForeignKey("PictureContentId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.LayoutBase", b =>
                {
                    b.HasOne("California.Creator.Service.Models.Core.CaliforniaProject", "CaliforniaProject")
                        .WithMany("LayoutMolecules")
                        .HasForeignKey("CaliforniaProjectId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.PictureContent", b =>
                {
                    b.HasOne("California.Creator.Service.Models.Core.CaliforniaProject", "CaliforniaProject")
                        .WithMany("PictureContents")
                        .HasForeignKey("CaliforniaProjectId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.ResponsiveDevice", b =>
                {
                    b.HasOne("California.Creator.Service.Models.Core.CaliforniaProject", "CaliforniaProject")
                        .WithMany("ResponsiveDevices")
                        .HasForeignKey("CaliforniaProjectId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.SharedProjectInfo", b =>
                {
                    b.HasOne("California.Creator.Service.Models.Core.CaliforniaProject", "CaliforniaProject")
                        .WithMany("SharedProjectInfos")
                        .HasForeignKey("CaliforniaProjectId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("California.Creator.Service.Models.CaliforniaStore", "OwnerCaliforniaStore")
                        .WithMany("OwnedSharedProjectInfos")
                        .HasForeignKey("OwnerCaliforniaStoreId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("California.Creator.Service.Models.CaliforniaStore", "SharedWithCaliforniaStore")
                        .WithMany("ForeignSharedProjectInfos")
                        .HasForeignKey("SharedWithCaliforniaStoreId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.StyleAtom", b =>
                {
                    b.HasOne("California.Creator.Service.Models.Core.CaliforniaProject", "CaliforniaProject")
                        .WithMany("StyleAtoms")
                        .HasForeignKey("CaliforniaProjectId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("California.Creator.Service.Models.Core.StyleMoleculeAtomMapping", "MappedToMolecule")
                        .WithOne("StyleAtom")
                        .HasForeignKey("California.Creator.Service.Models.Core.StyleAtom", "MappedToMoleculeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.StyleAtomQuantumMapping", b =>
                {
                    b.HasOne("California.Creator.Service.Models.Core.StyleAtom", "StyleAtom")
                        .WithMany("MappedQuantums")
                        .HasForeignKey("StyleAtomId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("California.Creator.Service.Models.Core.StyleQuantum", "StyleQuantum")
                        .WithMany("MappedToAtoms")
                        .HasForeignKey("StyleQuantumId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.StyleMolecule", b =>
                {
                    b.HasOne("California.Creator.Service.Models.Core.CaliforniaProject", "CaliforniaProject")
                        .WithMany("StyleMolecules")
                        .HasForeignKey("CaliforniaProjectId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("California.Creator.Service.Models.Core.StyleMolecule", "ClonedFromStyle")
                        .WithMany("CloneOfStyles")
                        .HasForeignKey("ClonedFromStyleId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("California.Creator.Service.Models.Core.LayoutBase", "StyleForLayout")
                        .WithOne("StyleMolecule")
                        .HasForeignKey("California.Creator.Service.Models.Core.StyleMolecule", "StyleForLayoutId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.StyleMoleculeAtomMapping", b =>
                {
                    b.HasOne("California.Creator.Service.Models.Core.ResponsiveDevice", "ResponsiveDevice")
                        .WithMany("AppliedToMappings")
                        .HasForeignKey("ResponsiveDeviceId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("California.Creator.Service.Models.Core.StyleMolecule", "StyleMolecule")
                        .WithMany("MappedStyleAtoms")
                        .HasForeignKey("StyleMoleculeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.StyleQuantum", b =>
                {
                    b.HasOne("California.Creator.Service.Models.Core.CaliforniaProject", "CaliforniaProject")
                        .WithMany("StyleQuantums")
                        .HasForeignKey("CaliforniaProjectId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.StyleValue", b =>
                {
                    b.HasOne("California.Creator.Service.Models.Core.CaliforniaProject", "CaliforniaProject")
                        .WithMany("StyleValues")
                        .HasForeignKey("CaliforniaProjectId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("California.Creator.Service.Models.Core.StyleAtom", "StyleAtom")
                        .WithMany("AppliedValues")
                        .HasForeignKey("StyleAtomId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("California.Creator.Service.Models.DbContentSafetyLock", b =>
                {
                    b.HasOne("California.Creator.Service.Models.Core.ContentAtom", "ContentAtom")
                        .WithOne("DbContentSafetyLock")
                        .HasForeignKey("California.Creator.Service.Models.DbContentSafetyLock", "ContentAtomId")
                        .HasConstraintName("FK_ContentAtom_RemoveIsNotSafe_RemoveIsNotSafeId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.LayoutAtom", b =>
                {
                    b.HasOne("California.Creator.Service.Models.Core.LayoutBox", "PlacedAtomInBox")
                        .WithMany("PlacedInBoxAtoms")
                        .HasForeignKey("PlacedAtomInBoxId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.LayoutBox", b =>
                {
                    b.HasOne("California.Creator.Service.Models.Core.LayoutRow", "BoxOwnerRow")
                        .WithMany("InsideRowBoxes")
                        .HasForeignKey("BoxOwnerRowId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("California.Creator.Service.Models.Core.LayoutBox", "PlacedBoxInBox")
                        .WithMany("PlacedInBoxBoxes")
                        .HasForeignKey("PlacedBoxInBoxId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("California.Creator.Service.Models.Core.LayoutRow", b =>
                {
                    b.HasOne("California.Creator.Service.Models.Core.LayoutRow", "ClonedFromLayout")
                        .WithMany("RefForClonedLayouts")
                        .HasForeignKey("ClonedFromLayoutId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("California.Creator.Service.Models.Core.CaliforniaView", "HostedView")
                        .WithMany("HostedByLayouts")
                        .HasForeignKey("HostedViewId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("California.Creator.Service.Models.Core.CaliforniaView", "PlacedOnView")
                        .WithMany("PlacedLayoutRows")
                        .HasForeignKey("PlacedOnViewId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
