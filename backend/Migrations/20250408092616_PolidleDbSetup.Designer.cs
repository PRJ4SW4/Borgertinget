﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using backend.Data;

#nullable disable

namespace backend.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20250408092616_PolidleDbSetup")]
    partial class PolidleDbSetup
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AnswerOption", b =>
                {
                    b.Property<int>("AnswerOptionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("AnswerOptionId"));

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("integer");

                    b.Property<bool>("IsCorrect")
                        .HasColumnType("boolean");

                    b.Property<string>("OptionText")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("QuestionId")
                        .HasColumnType("integer");

                    b.HasKey("AnswerOptionId");

                    b.HasIndex("QuestionId");

                    b.ToTable("AnswerOptions");
                });

            modelBuilder.Entity("FakePolitiker", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Alder")
                        .HasColumnType("integer");

                    b.Property<string>("Køn")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PolitikerNavn")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.Property<string>("Uddannelse")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("Id");

                    b.ToTable("FakePolitikere");
                });

            modelBuilder.Entity("Flashcard", b =>
                {
                    b.Property<int>("FlashcardId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("FlashcardId"));

                    b.Property<int>("BackContentType")
                        .HasColumnType("integer");

                    b.Property<string>("BackImagePath")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("BackText")
                        .HasColumnType("text");

                    b.Property<int>("CollectionId")
                        .HasColumnType("integer");

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("integer");

                    b.Property<int>("FrontContentType")
                        .HasColumnType("integer");

                    b.Property<string>("FrontImagePath")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("FrontText")
                        .HasColumnType("text");

                    b.HasKey("FlashcardId");

                    b.HasIndex("CollectionId");

                    b.ToTable("Flashcards");
                });

            modelBuilder.Entity("FlashcardCollection", b =>
                {
                    b.Property<int>("CollectionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CollectionId"));

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.HasKey("CollectionId");

                    b.ToTable("FlashcardCollections");
                });

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
                });

            modelBuilder.Entity("PolidleGamemodeTracker", b =>
                {
                    b.Property<int>("PolitikerId")
                        .HasColumnType("integer")
                        .HasColumnName("politiker_id");

                    b.Property<string>("GameMode")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("gamemode");

                    b.Property<int?>("AlgoWeight")
                        .HasColumnType("integer")
                        .HasColumnName("algovægt");

                    b.Property<DateOnly?>("LastSelectedDate")
                        .HasColumnType("date")
                        .HasColumnName("lastselecteddate");

                    b.HasKey("PolitikerId", "GameMode");

                    b.ToTable("GameTrackings");
                });

            modelBuilder.Entity("Question", b =>
                {
                    b.Property<int>("QuestionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("QuestionId"));

                    b.Property<int>("PageId")
                        .HasColumnType("integer");

                    b.Property<string>("QuestionText")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("QuestionId");

                    b.HasIndex("PageId");

                    b.ToTable("Questions");
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

            modelBuilder.Entity("AnswerOption", b =>
                {
                    b.HasOne("Question", "Question")
                        .WithMany("AnswerOptions")
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Question");
                });

            modelBuilder.Entity("Flashcard", b =>
                {
                    b.HasOne("FlashcardCollection", "FlashcardCollection")
                        .WithMany("Flashcards")
                        .HasForeignKey("CollectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FlashcardCollection");
                });

            modelBuilder.Entity("Page", b =>
                {
                    b.HasOne("Page", "ParentPage")
                        .WithMany("ChildPages")
                        .HasForeignKey("ParentPageId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("ParentPage");
                });

            modelBuilder.Entity("PolidleGamemodeTracker", b =>
                {
                    b.HasOne("FakePolitiker", "FakePolitiker")
                        .WithMany("GameTrackings")
                        .HasForeignKey("PolitikerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FakePolitiker");
                });

            modelBuilder.Entity("Question", b =>
                {
                    b.HasOne("Page", "Page")
                        .WithMany("AssociatedQuestions")
                        .HasForeignKey("PageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Page");
                });

            modelBuilder.Entity("FakePolitiker", b =>
                {
                    b.Navigation("GameTrackings");
                });

            modelBuilder.Entity("FlashcardCollection", b =>
                {
                    b.Navigation("Flashcards");
                });

            modelBuilder.Entity("Page", b =>
                {
                    b.Navigation("AssociatedQuestions");

                    b.Navigation("ChildPages");
                });

            modelBuilder.Entity("Question", b =>
                {
                    b.Navigation("AnswerOptions");
                });
#pragma warning restore 612, 618
        }
    }
}
