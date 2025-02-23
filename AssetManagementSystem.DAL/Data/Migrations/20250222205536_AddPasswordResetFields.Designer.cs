﻿// <auto-generated />
using System;
using AssetManagementSystem.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AssetManagementSystem.DAL.Data.Migrations
{
    [DbContext(typeof(AssetManagementDbContext))]
    [Migration("20250222205536_AddPasswordResetFields")]
    partial class AddPasswordResetFields
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.Asset", b =>
                {
                    b.Property<string>("AssetTag")
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("AssetClass1")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AssetClass2")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AssetClass3")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AssetDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AssetType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Brand")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("BuildingId")
                        .HasColumnType("int");

                    b.Property<string>("Cluster")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DepartmentId")
                        .HasColumnType("int");

                    b.Property<string>("Details")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("FacilityId")
                        .HasColumnType("int");

                    b.Property<int>("FloorId")
                        .HasColumnType("int");

                    b.Property<DateTime>("InsertDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("InsertUser")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDisposed")
                        .HasColumnType("bit");

                    b.Property<string>("Model")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoomTag")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("SerialNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("AssetTag");

                    b.HasIndex("BuildingId");

                    b.HasIndex("DepartmentId");

                    b.HasIndex("FacilityId");

                    b.HasIndex("FloorId");

                    b.HasIndex("RoomTag");

                    b.HasIndex("UserId");

                    b.ToTable("Assets");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.AssetTransfer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AssetTag")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("FromDepartment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ToDepartment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("TransferDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("TransferType")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AssetTag");

                    b.ToTable("Transfers");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.Building", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("FacilityId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("FacilityId");

                    b.ToTable("Buildings");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.ChangeLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ActionType")
                        .HasColumnType("nvarchar(255)");

                    b.Property<DateTime>("ChangeDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("EntityId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("EntityName")
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("NewValues")
                        .HasColumnType("nvarchar(MAX)");

                    b.Property<string>("OldValues")
                        .HasColumnType("nvarchar(MAX)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("ChangeDate");

                    b.HasIndex("UserId");

                    b.HasIndex("EntityName", "EntityId");

                    b.ToTable("changeLogs");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.Department", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.ToTable("Departments");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.Disposal", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AssetTag")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("DisposalDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("DisposalType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("SaleValue")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("AssetTag");

                    b.ToTable("Disposals");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.Facility", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.ToTable("Facilities");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.Floor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BuildingId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("BuildingId");

                    b.ToTable("Floors");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.Room", b =>
                {
                    b.Property<string>("RoomTag")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("DepartmentId")
                        .HasColumnType("int");

                    b.Property<int>("FloorId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("RoomTag");

                    b.HasIndex("DepartmentId");

                    b.HasIndex("FloorId");

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DepartmentId")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NationalId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("RecipientFileNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResetPasswordToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ResetPasswordTokenExpiry")
                        .HasColumnType("datetime2");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("DepartmentId");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.Asset", b =>
                {
                    b.HasOne("AssetManagementSystem.DAL.Entities.Building", "Building")
                        .WithMany()
                        .HasForeignKey("BuildingId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("AssetManagementSystem.DAL.Entities.Department", "Department")
                        .WithMany()
                        .HasForeignKey("DepartmentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("AssetManagementSystem.DAL.Entities.Facility", "Facility")
                        .WithMany()
                        .HasForeignKey("FacilityId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("AssetManagementSystem.DAL.Entities.Floor", "Floor")
                        .WithMany()
                        .HasForeignKey("FloorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("AssetManagementSystem.DAL.Entities.Room", "Room")
                        .WithMany()
                        .HasForeignKey("RoomTag")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("AssetManagementSystem.DAL.Entities.User", "User")
                        .WithMany("Assets")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Building");

                    b.Navigation("Department");

                    b.Navigation("Facility");

                    b.Navigation("Floor");

                    b.Navigation("Room");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.AssetTransfer", b =>
                {
                    b.HasOne("AssetManagementSystem.DAL.Entities.Asset", "Asset")
                        .WithMany()
                        .HasForeignKey("AssetTag")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Asset");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.Building", b =>
                {
                    b.HasOne("AssetManagementSystem.DAL.Entities.Facility", "Facility")
                        .WithMany("Buildings")
                        .HasForeignKey("FacilityId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Facility");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.ChangeLog", b =>
                {
                    b.HasOne("AssetManagementSystem.DAL.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("User");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.Disposal", b =>
                {
                    b.HasOne("AssetManagementSystem.DAL.Entities.Asset", "Asset")
                        .WithMany()
                        .HasForeignKey("AssetTag")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Asset");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.Floor", b =>
                {
                    b.HasOne("AssetManagementSystem.DAL.Entities.Building", "Building")
                        .WithMany("Floors")
                        .HasForeignKey("BuildingId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Building");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.Room", b =>
                {
                    b.HasOne("AssetManagementSystem.DAL.Entities.Department", "Department")
                        .WithMany("Rooms")
                        .HasForeignKey("DepartmentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("AssetManagementSystem.DAL.Entities.Floor", "Floor")
                        .WithMany("Rooms")
                        .HasForeignKey("FloorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Department");

                    b.Navigation("Floor");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.User", b =>
                {
                    b.HasOne("AssetManagementSystem.DAL.Entities.Department", "Department")
                        .WithMany()
                        .HasForeignKey("DepartmentId");

                    b.Navigation("Department");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("AssetManagementSystem.DAL.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("AssetManagementSystem.DAL.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AssetManagementSystem.DAL.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("AssetManagementSystem.DAL.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.Building", b =>
                {
                    b.Navigation("Floors");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.Department", b =>
                {
                    b.Navigation("Rooms");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.Facility", b =>
                {
                    b.Navigation("Buildings");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.Floor", b =>
                {
                    b.Navigation("Rooms");
                });

            modelBuilder.Entity("AssetManagementSystem.DAL.Entities.User", b =>
                {
                    b.Navigation("Assets");
                });
#pragma warning restore 612, 618
        }
    }
}
