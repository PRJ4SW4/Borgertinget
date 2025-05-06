using System.Collections.Generic; // Required
using System.Text.Json; // Required
using backend.DTO.Calendar;
using backend.DTO.LearningEnvironment;
using backend.Models;
using backend.Models.Calendar;
using backend.Models.Flashcards;
using backend.Models.LearningEnvironment;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; } = null!;

    // --- Learning Environment Setup ---
    public DbSet<Page> Pages { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<AnswerOption> AnswerOptions { get; set; }
    public DbSet<Flashcard> Flashcards { get; set; }
    public DbSet<FlashcardCollection> FlashcardCollections { get; set; }

    // --- /Learning Environment Setup ---

    // --- Calendar Setup ---
    public DbSet<CalendarEvent> CalendarEvents { get; set; }

    // --- /Calendar Setup ---

    public DbSet<Aktor> Aktor { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Calendar Setup ---
        // Index for the CalendarEvents SourceUrl to make syncing events faster
        modelBuilder.Entity<CalendarEvent>().HasIndex(e => e.SourceUrl).IsUnique();

        // --- /Calendar Setup ---

        // --- Learning Environment Setup ---

        // Configure the self-referencing relationship
        modelBuilder
            .Entity<Page>()
            .HasOne(p => p.ParentPage) // A page has one parent
            .WithMany(p => p.ChildPages) // A parent can have many children
            .HasForeignKey(p => p.ParentPageId) // The foreign key is ParentPageId
            .OnDelete(DeleteBehavior.Cascade); // Cascade deletions. Can be changed

        // This configuration tells EF Core that one Page can have many Questions,
        // and each Question points back to one Page using the PageId foreign key.
        modelBuilder
            .Entity<Page>()
            .HasMany(p => p.AssociatedQuestions) // Use the ICollection property in Page
            .WithOne(q => q.Page) // Use the navigation property back to Page in Question
            .HasForeignKey(q => q.PageId);

        // Configure AnswerOption relationship (One Question to Many Options)
        modelBuilder
            .Entity<Question>()
            .HasMany(q => q.AnswerOptions)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId);

        // Configure FlashcardCollection <-> Flashcard relationship
        modelBuilder
            .Entity<FlashcardCollection>()
            .HasMany(c => c.Flashcards)
            .WithOne(f => f.FlashcardCollection)
            .HasForeignKey(f => f.CollectionId);

        // --- /Learning Environment Setup ---

        // Configure Constituencies
        modelBuilder
            .Entity<Aktor>()
            .Property(a => a.Constituencies) // Target the List<string> property
            .HasConversion(
                // Convert List<string> to json string for DB
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                // Convert json string from DB back to List<string>
                v =>
                    JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
                    ?? new List<string>()
            );

        // Configure Nominations
        modelBuilder
            .Entity<Aktor>()
            .Property(a => a.Nominations)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v =>
                    JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
                    ?? new List<string>()
            );

        // Add similar .HasConversion calls for Educations and Occupations
        modelBuilder
            .Entity<Aktor>()
            .Property(a => a.Educations)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v =>
                    JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
                    ?? new List<string>()
            );

        modelBuilder
            .Entity<Aktor>()
            .Property(a => a.Occupations)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v =>
                    JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
                    ?? new List<string>()
            );

        modelBuilder
            .Entity<Aktor>()
            .Property(a => a.PublicationTitles)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v =>
                    JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
                    ?? new List<string>()
            );
        modelBuilder
            .Entity<Aktor>()
            .Property(a => a.Ministers)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v =>
                    JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
                    ?? new List<string>()
            );
        modelBuilder
            .Entity<Aktor>()
            .Property(a => a.Spokesmen)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v =>
                    JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
                    ?? new List<string>()
            );

        // --- SEED DATA ---

        // --- Learning Environment Seeding ---

        // 1. Seed Pages
        modelBuilder
            .Entity<Page>()
            .HasData(
                new Page
                {
                    Id = 1,
                    Title = "Politik 101",
                    Content = "Indhold for Politik 101...",
                    ParentPageId = null,
                    DisplayOrder = 1,
                },
                new Page
                {
                    Id = 2,
                    Title = "Den Politiske Akse",
                    Content = "Indhold for Den Politiske Akse...",
                    ParentPageId = 1,
                    DisplayOrder = 1,
                },
                new Page
                {
                    Id = 3,
                    Title = "Venstre vs Højre",
                    Content = "Indhold for Venstre vs Højre...",
                    ParentPageId = 2,
                    DisplayOrder = 1,
                },
                new Page
                {
                    Id = 4,
                    Title = "Højre",
                    Content = "Højre er at være højre...",
                    ParentPageId = 3,
                    DisplayOrder = 1,
                },
                new Page
                {
                    Id = 5,
                    Title = "Venstre",
                    Content = "Venstre er at være venstre...",
                    ParentPageId = 3,
                    DisplayOrder = 2,
                }
            );

        // 2. Seed Questions (Linked to Pages)
        modelBuilder
            .Entity<Question>()
            .HasData(
                // -- Questions for Page 1 --
                new Question
                {
                    QuestionId = 1, // Unique ID for this question
                    PageId = 1, // Links to "Politik 101"
                    QuestionText = "Hvad beskæftiger politologi sig primært med?",
                },
                new Question
                {
                    QuestionId = 2, // Unique ID for this question
                    PageId = 1, // Also links to "Politik 101"
                    QuestionText =
                        "Hvilket begreb dækker over fordelingen af autoritet i et samfund?",
                },
                // -- Question for Page 4 --
                new Question
                {
                    QuestionId = 3, // Unique ID for this question
                    PageId = 4, // Links to "Højre"
                    QuestionText =
                        "Hvilket økonomisk princip forbindes ofte med højreorienteret politik?",
                },
                // -- Question for Page 5 --
                new Question
                {
                    QuestionId = 4, // Unique ID for this question
                    PageId = 5, // Links to "Venstre"
                    QuestionText = "Hvilken værdi vægtes typisk højt i venstreorienteret ideologi?",
                }
            );

        // 3. Seed Answer Options (Linked to Questions)
        modelBuilder
            .Entity<AnswerOption>()
            .HasData(
                // -- Options for Question 1 (Page 1) --
                new AnswerOption
                {
                    AnswerOptionId = 1,
                    QuestionId = 1,
                    OptionText = "Studiet af magtstrukturer og beslutningsprocesser",
                    IsCorrect = true,
                    DisplayOrder = 1,
                },
                new AnswerOption
                {
                    AnswerOptionId = 2,
                    QuestionId = 1,
                    OptionText = "Analyse af internationale handelsaftaler",
                    IsCorrect = false,
                    DisplayOrder = 2,
                },
                new AnswerOption
                {
                    AnswerOptionId = 3,
                    QuestionId = 1,
                    OptionText = "Udforskning af historiske monarkier",
                    IsCorrect = false,
                    DisplayOrder = 3,
                },
                // -- Options for Question 2 (Page 1) --
                new AnswerOption
                {
                    AnswerOptionId = 4,
                    QuestionId = 2,
                    OptionText = "Social mobilitet",
                    IsCorrect = false,
                    DisplayOrder = 1,
                },
                new AnswerOption
                {
                    AnswerOptionId = 5,
                    QuestionId = 2,
                    OptionText = "Magtdeling",
                    IsCorrect = true,
                    DisplayOrder = 2,
                },
                new AnswerOption
                {
                    AnswerOptionId = 6,
                    QuestionId = 2,
                    OptionText = "Kulturel assimilation",
                    IsCorrect = false,
                    DisplayOrder = 3,
                },
                // -- Options for Question 3 (Page 4) --
                new AnswerOption
                {
                    AnswerOptionId = 7,
                    QuestionId = 3,
                    OptionText = "Planøkonomi",
                    IsCorrect = false,
                    DisplayOrder = 1,
                },
                new AnswerOption
                {
                    AnswerOptionId = 8,
                    QuestionId = 3,
                    OptionText = "Høj grad af omfordeling",
                    IsCorrect = false,
                    DisplayOrder = 2,
                },
                new AnswerOption
                {
                    AnswerOptionId = 9,
                    QuestionId = 3,
                    OptionText = "Frit marked og privat ejendomsret",
                    IsCorrect = true,
                    DisplayOrder = 3,
                },
                // -- Options for Question 4 (Page 5) --
                new AnswerOption
                {
                    AnswerOptionId = 10,
                    QuestionId = 4,
                    OptionText = "Individuel konkurrence",
                    IsCorrect = false,
                    DisplayOrder = 1,
                },
                new AnswerOption
                {
                    AnswerOptionId = 11,
                    QuestionId = 4,
                    OptionText = "Social lighed og fællesskabets velfærd",
                    IsCorrect = true,
                    DisplayOrder = 2,
                },
                new AnswerOption
                {
                    AnswerOptionId = 12,
                    QuestionId = 4,
                    OptionText = "Traditionelle hierarkier",
                    IsCorrect = false,
                    DisplayOrder = 3,
                }
            );

        // --- FLASHCARDS ---

        modelBuilder
            .Entity<FlashcardCollection>()
            .HasData(
                new FlashcardCollection
                {
                    CollectionId = 1,
                    Title = "Politikerne og deres navne",
                    DisplayOrder = 1,
                },
                new FlashcardCollection
                {
                    CollectionId = 2,
                    Title = "Politiske begreber",
                    DisplayOrder = 2,
                }
            );

        modelBuilder
            .Entity<Flashcard>()
            .HasData(
                // Cards for Collection 1
                new Flashcard
                {
                    FlashcardId = 1,
                    CollectionId = 1,
                    DisplayOrder = 1,
                    FrontContentType = FlashcardContentType.Image,
                    FrontImagePath = "/uploads/flashcards/mettef.png",
                    BackContentType = FlashcardContentType.Text,
                    BackText = "Mette Frederiksen",
                },
                new Flashcard
                {
                    FlashcardId = 2,
                    CollectionId = 1,
                    DisplayOrder = 2,
                    FrontContentType = FlashcardContentType.Image,
                    FrontImagePath = "/uploads/flashcards/larsl.png",
                    BackContentType = FlashcardContentType.Text,
                    BackText = "Lars Løkke Rasmussen",
                },
                new Flashcard
                {
                    FlashcardId = 3,
                    CollectionId = 1,
                    DisplayOrder = 3,
                    FrontContentType = FlashcardContentType.Text,
                    FrontText = "Hvem er formand for Danmarksdemokraterne?",
                    BackContentType = FlashcardContentType.Text,
                    BackText = "Inger Støjberg",
                },
                // Cards for Collection 2
                new Flashcard
                {
                    FlashcardId = 4,
                    CollectionId = 2,
                    DisplayOrder = 1,
                    FrontContentType = FlashcardContentType.Text,
                    FrontText = "Hvad betyder 'Demokrati'?",
                    BackContentType = FlashcardContentType.Text,
                    BackText = "Folkestyre",
                },
                new Flashcard
                {
                    FlashcardId = 5,
                    CollectionId = 2,
                    DisplayOrder = 2,
                    FrontContentType = FlashcardContentType.Text,
                    FrontText = "Hvad er 'Finansloven'?",
                    BackContentType = FlashcardContentType.Text,
                    BackText = "Statens budget for det kommende år",
                }
            );

        // --- /FLASHCARDS ---

        // --- /Learning Environment Seeding ---

        // --- /SEED DATA ---
    }
}
