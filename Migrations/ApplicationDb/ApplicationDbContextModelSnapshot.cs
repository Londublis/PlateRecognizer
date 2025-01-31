﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlateRecognizer.Data;

#nullable disable

namespace PlateRecognizer.Migrations.ApplicationDb
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("PlateRecognizer.Models.Car", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool?>("IsScraped")
                        .HasColumnType("bit");

                    b.Property<string>("Matricula")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PrCount")
                        .HasColumnType("int");

                    b.Property<string>("PrPlate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("PrScore")
                        .HasColumnType("float");

                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Cars");
                });

            modelBuilder.Entity("TableImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CarID")
                        .HasColumnType("int");

                    b.Property<byte[]>("ImageData")
                        .HasColumnType("varbinary(max)");

                    b.Property<bool?>("IsFullpage")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("CarID");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("TableImage", b =>
                {
                    b.HasOne("PlateRecognizer.Models.Car", "Car")
                        .WithMany()
                        .HasForeignKey("CarID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Car");
                });
#pragma warning restore 612, 618
        }
    }
}
