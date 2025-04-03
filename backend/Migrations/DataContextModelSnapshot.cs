﻿// <auto-generated />
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using backend.Data;

#nullable disable

namespace backend.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Page", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("integer");

                    b.Property<int?>("ParentPageId")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.HasKey("Id");

                    b.HasIndex("ParentPageId");

                    b.ToTable("Pages");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Content = "# Politik 101\n\nPolitik handler om...",
                            DisplayOrder = 1,
                            Title = "Politik 101"
                        },
                        new
                        {
                            Id = 2,
                            Content = "## Den Politiske Akse...",
                            DisplayOrder = 1,
                            ParentPageId = 1,
                            Title = "Den Politiske Akse"
                        },
                        new
                        {
                            Id = 3,
                            Content = "### Venstre vs Højre...",
                            DisplayOrder = 1,
                            ParentPageId = 2,
                            Title = "Venstre vs Højre"
                        },
                        new
                        {
                            Id = 4,
                            Content = "# Højre \n\n Højre er at være højre...",
                            DisplayOrder = 1,
                            ParentPageId = 3,
                            Title = "Højre"
                        },
                        new
                        {
                            Id = 5,
                            Content = "# Venstre \n\n Venstre er at være venstre...",
                            DisplayOrder = 2,
                            ParentPageId = 3,
                            Title = "Venstre"
                        });
                });

            modelBuilder.Entity("backend.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsVerified")
                        .HasColumnType("boolean");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.PrimitiveCollection<List<string>>("Roles")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("VerificationToken")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Page", b =>
                {
                    b.HasOne("Page", "ParentPage")
                        .WithMany("ChildPages")
                        .HasForeignKey("ParentPageId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("ParentPage");
                });

            modelBuilder.Entity("Page", b =>
                {
                    b.Navigation("ChildPages");
                });
#pragma warning restore 612, 618
        }
    }
}
