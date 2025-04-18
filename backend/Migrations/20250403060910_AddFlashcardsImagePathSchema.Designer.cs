﻿// <auto-generated />
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
    [Migration("20250403060910_AddFlashcardsImagePathSchema")]
    partial class AddFlashcardsImagePathSchema
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

                    b.HasData(
                        new
                        {
                            AnswerOptionId = 1,
                            DisplayOrder = 1,
                            IsCorrect = true,
                            OptionText = "Studiet af magtstrukturer og beslutningsprocesser",
                            QuestionId = 1
                        },
                        new
                        {
                            AnswerOptionId = 2,
                            DisplayOrder = 2,
                            IsCorrect = false,
                            OptionText = "Analyse af internationale handelsaftaler",
                            QuestionId = 1
                        },
                        new
                        {
                            AnswerOptionId = 3,
                            DisplayOrder = 3,
                            IsCorrect = false,
                            OptionText = "Udforskning af historiske monarkier",
                            QuestionId = 1
                        },
                        new
                        {
                            AnswerOptionId = 4,
                            DisplayOrder = 1,
                            IsCorrect = false,
                            OptionText = "Social mobilitet",
                            QuestionId = 2
                        },
                        new
                        {
                            AnswerOptionId = 5,
                            DisplayOrder = 2,
                            IsCorrect = true,
                            OptionText = "Magtdeling",
                            QuestionId = 2
                        },
                        new
                        {
                            AnswerOptionId = 6,
                            DisplayOrder = 3,
                            IsCorrect = false,
                            OptionText = "Kulturel assimilation",
                            QuestionId = 2
                        },
                        new
                        {
                            AnswerOptionId = 7,
                            DisplayOrder = 1,
                            IsCorrect = false,
                            OptionText = "Planøkonomi",
                            QuestionId = 3
                        },
                        new
                        {
                            AnswerOptionId = 8,
                            DisplayOrder = 2,
                            IsCorrect = false,
                            OptionText = "Høj grad af omfordeling",
                            QuestionId = 3
                        },
                        new
                        {
                            AnswerOptionId = 9,
                            DisplayOrder = 3,
                            IsCorrect = true,
                            OptionText = "Frit marked og privat ejendomsret",
                            QuestionId = 3
                        },
                        new
                        {
                            AnswerOptionId = 10,
                            DisplayOrder = 1,
                            IsCorrect = false,
                            OptionText = "Individuel konkurrence",
                            QuestionId = 4
                        },
                        new
                        {
                            AnswerOptionId = 11,
                            DisplayOrder = 2,
                            IsCorrect = true,
                            OptionText = "Social lighed og fællesskabets velfærd",
                            QuestionId = 4
                        },
                        new
                        {
                            AnswerOptionId = 12,
                            DisplayOrder = 3,
                            IsCorrect = false,
                            OptionText = "Traditionelle hierarkier",
                            QuestionId = 4
                        });
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

                    b.HasData(
                        new
                        {
                            FlashcardId = 1,
                            BackContentType = 0,
                            BackText = "Mette Frederiksen",
                            CollectionId = 1,
                            DisplayOrder = 1,
                            FrontContentType = 1,
                            FrontImagePath = "/uploads/flashcards/mettef.png"
                        },
                        new
                        {
                            FlashcardId = 2,
                            BackContentType = 0,
                            BackText = "Lars Løkke Rasmussen",
                            CollectionId = 1,
                            DisplayOrder = 2,
                            FrontContentType = 1,
                            FrontImagePath = "/uploads/flashcards/larsl.png"
                        },
                        new
                        {
                            FlashcardId = 3,
                            BackContentType = 0,
                            BackText = "Inger Støjberg",
                            CollectionId = 1,
                            DisplayOrder = 3,
                            FrontContentType = 0,
                            FrontText = "Hvem er formand for Danmarksdemokraterne?"
                        },
                        new
                        {
                            FlashcardId = 4,
                            BackContentType = 0,
                            BackText = "Folkestyre",
                            CollectionId = 2,
                            DisplayOrder = 1,
                            FrontContentType = 0,
                            FrontText = "Hvad betyder 'Demokrati'?"
                        },
                        new
                        {
                            FlashcardId = 5,
                            BackContentType = 0,
                            BackText = "Statens budget for det kommende år",
                            CollectionId = 2,
                            DisplayOrder = 2,
                            FrontContentType = 0,
                            FrontText = "Hvad er 'Finansloven'?"
                        });
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

                    b.HasData(
                        new
                        {
                            CollectionId = 1,
                            DisplayOrder = 1,
                            Title = "Politikerne og deres navne"
                        },
                        new
                        {
                            CollectionId = 2,
                            DisplayOrder = 2,
                            Title = "Politiske begreber"
                        });
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

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Content = "Indhold for Politik 101...",
                            DisplayOrder = 1,
                            Title = "Politik 101"
                        },
                        new
                        {
                            Id = 2,
                            Content = "Indhold for Den Politiske Akse...",
                            DisplayOrder = 1,
                            ParentPageId = 1,
                            Title = "Den Politiske Akse"
                        },
                        new
                        {
                            Id = 3,
                            Content = "Indhold for Venstre vs Højre...",
                            DisplayOrder = 1,
                            ParentPageId = 2,
                            Title = "Venstre vs Højre"
                        },
                        new
                        {
                            Id = 4,
                            Content = "Højre er at være højre...",
                            DisplayOrder = 1,
                            ParentPageId = 3,
                            Title = "Højre"
                        },
                        new
                        {
                            Id = 5,
                            Content = "Venstre er at være venstre...",
                            DisplayOrder = 2,
                            ParentPageId = 3,
                            Title = "Venstre"
                        });
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

                    b.HasData(
                        new
                        {
                            QuestionId = 1,
                            PageId = 1,
                            QuestionText = "Hvad beskæftiger politologi sig primært med?"
                        },
                        new
                        {
                            QuestionId = 2,
                            PageId = 1,
                            QuestionText = "Hvilket begreb dækker over fordelingen af autoritet i et samfund?"
                        },
                        new
                        {
                            QuestionId = 3,
                            PageId = 4,
                            QuestionText = "Hvilket økonomisk princip forbindes ofte med højreorienteret politik?"
                        },
                        new
                        {
                            QuestionId = 4,
                            PageId = 5,
                            QuestionText = "Hvilken værdi vægtes typisk højt i venstreorienteret ideologi?"
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

            modelBuilder.Entity("Question", b =>
                {
                    b.HasOne("Page", "Page")
                        .WithMany("AssociatedQuestions")
                        .HasForeignKey("PageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Page");
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
