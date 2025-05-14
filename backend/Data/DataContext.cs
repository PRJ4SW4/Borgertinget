using System;
using System.Collections.Generic; // Required
using System.Linq;
using System.Text.Json; // Required
using backend.DTO.Calendar;
using backend.DTO.LearningEnvironment;
using backend.Models;
using backend.Models.Calendar;
using backend.Models.Flashcards;
using backend.Models.LearningEnvironment;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace backend.Data
{
    public class DataContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options) { }

        // --- DbSets ---
        // --- Learning Environment Setup ---
        public DbSet<Page> Pages { get; set; } = null!;
        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<AnswerOption> AnswerOptions { get; set; } = null!;
        public DbSet<Flashcard> Flashcards { get; set; } = null!;
        public DbSet<FlashcardCollection> FlashcardCollections { get; set; } = null!;

        // --- /Learning Environment Setup ---

        // --- Twitter Setup ---
        public DbSet<Tweet> Tweets { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; } = null!;
        public DbSet<PoliticianTwitterId> PoliticianTwitterIds { get; set; } = null!;
        public DbSet<Poll> Polls { get; set; } = null!;
        public DbSet<PollOption> PollOptions { get; set; } = null!;
        public DbSet<UserVote> UserVotes { get; set; } = null!;

        // --- /Twitter Setup ---



        // --- Calendar Setup ---
        public DbSet<CalendarEvent> CalendarEvents { get; set; }

        public DbSet<Party> Party { get; set; }

        public DbSet<EventInterest> EventInterests { get; set; }

        // --- /Calendar Setup ---

        public DbSet<Aktor> Aktor { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Calendar Setup ---
            modelBuilder.Entity<CalendarEvent>().HasIndex(e => e.SourceUrl).IsUnique();

            modelBuilder.Entity<EventInterest>(entity =>
            {
                entity.HasIndex(ei => new {ei.CalendarEventId, ei.UserId}).IsUnique();

                entity.HasOne(ei => ei.User)
                .WithMany()
                .HasForeignKey(ei => ei.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
                entity.HasOne(ei => ei.CalendarEvent)
                .WithMany(ce => ce.InterestedUsers)
                .HasForeignKey(ei => ei.CalendarEventId)
                .OnDelete(DeleteBehavior.Cascade);
            });


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
                )
                .Metadata.SetValueComparer(
                    new ValueComparer<List<string>>(
                        (c1, c2) =>
                            (c1 == null && c2 == null)
                            || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    )
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
                )
                .Metadata.SetValueComparer(
                    new ValueComparer<List<string>>(
                        (c1, c2) =>
                            (c1 == null && c2 == null)
                            || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    )
                );

            // Add similar .HasConversion calls AND .Metadata.SetValueComparer(...) for Educations and Occupations
            modelBuilder
                .Entity<Aktor>()
                .Property(a => a.Educations)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v =>
                        JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
                        ?? new List<string>()
                )
                .Metadata.SetValueComparer(
                    new ValueComparer<List<string>>(
                        (c1, c2) =>
                            (c1 == null && c2 == null)
                            || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    )
                );

            modelBuilder
                .Entity<Aktor>()
                .Property(a => a.Occupations)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v =>
                        JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
                        ?? new List<string>()
                )
                .Metadata.SetValueComparer(
                    new ValueComparer<List<string>>(
                        (c1, c2) =>
                            (c1 == null && c2 == null)
                            || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    )
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

            modelBuilder.Entity<Party>(entity =>
            {
                // Configure Role relationships (as shown previously)
                entity
                    .HasOne(p => p.chairman)
                    .WithMany()
                    .HasForeignKey(p => p.chairmanId)
                    .OnDelete(DeleteBehavior.SetNull);
                // ... configure other roles (ViceChairman, Secretary, Spokesman) ...

                // Configure Stats List conversion (if kept)
                entity
                    .Property(p => p.stats) // Assuming PascalCase naming
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v =>
                            JsonSerializer.Deserialize<List<string>>(
                                v,
                                (JsonSerializerOptions?)null
                            ) ?? new List<string>()
                    );

                // *** Add Configuration for memberIds List ***
                entity
                    .Property(p => p.memberIds) // Use PascalCase property name
                    .HasConversion(
                        // Convert List<int> to JSON string for DB
                        v =>
                            JsonSerializer.Serialize(
                                v ?? new List<int>(),
                                (JsonSerializerOptions?)null
                            ),
                        // Convert JSON string from DB back to List<int>
                        v =>
                            JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null)
                            ?? new List<int>(),
                        // Add a ValueComparer to help EF Core detect changes correctly
                        new ValueComparer<List<int>?>(
                            (c1, c2) =>
                                (c1 == null && c2 == null)
                                || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                            c =>
                                c == null
                                    ? 0
                                    : c.Aggregate(
                                        0,
                                        (a, v) => HashCode.Combine(a, v.GetHashCode())
                                    ),
                            c => c == null ? null : c.ToList()
                        )
                    )
                    .HasColumnType("text"); // Use jsonb for efficient querying in PostgreSQL if needed, or "text"
                // *** End Configuration for memberIds List ***
            });

            modelBuilder.Entity<PoliticianTwitterId>(entity =>
            {
                entity.HasIndex(p => p.TwitterUserId).IsUnique();
                entity
                    .HasMany(p => p.Tweets)
                    .WithOne(t => t.Politician)
                    .HasForeignKey(t => t.PoliticianTwitterId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasMany(p => p.Subscriptions)
                    .WithOne(s => s.Politician)
                    .HasForeignKey(s => s.PoliticianTwitterId);
                entity
                    .HasMany(p => p.Polls)
                    .WithOne(p => p.Politician)
                    .HasForeignKey(p => p.PoliticianTwitterId);

                entity.Property(p => p.TwitterUserId).IsRequired();
                entity.Property(p => p.Name).IsRequired();
                entity.Property(p => p.TwitterHandle).IsRequired();

                entity
                    .HasOne(politicianTwitter => politicianTwitter.Aktor)
                    .WithOne()
                    .HasForeignKey<PoliticianTwitterId>(politicianTwitter =>
                        politicianTwitter.AktorId
                    )
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                //ved merge skal nedestående være commented, da der ellers vi blive problemer med constraints i databasen
                entity.HasData(
                    new PoliticianTwitterId
                    {
                        Id = 1,
                        TwitterUserId = "806068174567460864",
                        Name = "Statsministeriet",
                        TwitterHandle = "Statsmin",
                        AktorId = null,
                    },
                    new PoliticianTwitterId
                    {
                        Id = 2,
                        TwitterUserId = "123868861",
                        Name = "Venstre, Danmarks Liberale Parti",
                        TwitterHandle = "venstredk",
                        AktorId = null,
                    },
                    new PoliticianTwitterId
                    {
                        Id = 3,
                        TwitterUserId = "2965907578",
                        Name = "Troels Lund Poulsen",
                        TwitterHandle = "troelslundp",
                        AktorId = null,
                    }
                );
            });

            modelBuilder.Entity<Tweet>(entity =>
            {
                entity.HasIndex(t => new { t.PoliticianTwitterId, t.TwitterTweetId }).IsUnique();
                entity.Property(t => t.TwitterTweetId).IsRequired();
                entity.Property(t => t.Text).IsRequired();
            });

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<IdentityRole<int>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasIndex(s => s.UserId);
                entity.HasIndex(s => s.PoliticianTwitterId);
            });

            modelBuilder.Entity<Poll>(entityPoll =>
            {
                entityPoll
                    .HasOne(poll => poll.Politician)
                    .WithMany(politician => politician.Polls)
                    .HasForeignKey(poll => poll.PoliticianTwitterId);
            });

            modelBuilder.Entity<UserVote>().HasIndex(uv => new { uv.UserId, uv.PollId }).IsUnique();

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
                        QuestionText =
                            "Hvilken værdi vægtes typisk højt i venstreorienteret ideologi?",
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


            const int SeedPoliticianId = 1;
            const int SeedPollId = 1;
            const int NewPollId = 2;

            modelBuilder
                .Entity<Poll>()
                .HasData(
                    new Poll
                    {
                        Id = SeedPollId,
                        Question = "Hvad synes du om den nye bro?",
                        PoliticianTwitterId = SeedPoliticianId,
                        CreatedAt = new DateTime(2025, 4, 15, 10, 0, 0, DateTimeKind.Utc),
                        EndedAt = null,
                    },
                    new Poll
                    {
                        Id = NewPollId,
                        Question = "Skal Danmark øge investeringer i vedvarende energi?",
                        PoliticianTwitterId = SeedPoliticianId,
                        CreatedAt = new DateTime(2025, 4, 28, 14, 30, 0, DateTimeKind.Utc),
                        EndedAt = null,
                    }
                );
            modelBuilder
                .Entity<PollOption>()
                .HasData(
                    new PollOption
                    {
                        Id = 1,
                        PollId = SeedPollId,
                        OptionText = "Den er fantastisk!",
                        Votes = 5,
                    },
                    new PollOption
                    {
                        Id = 2,
                        PollId = SeedPollId,
                        OptionText = "Den er ok, men dyr.",
                        Votes = 12,
                    },
                    new PollOption
                    {
                        Id = 3,
                        PollId = SeedPollId,
                        OptionText = "Den er unødvendig.",
                        Votes = 3,
                    },
                    new PollOption
                    {
                        Id = 4,
                        PollId = NewPollId,
                        OptionText = "Ja, meget mere end nu",
                        Votes = 42,
                    },
                    new PollOption
                    {
                        Id = 5,
                        PollId = NewPollId,
                        OptionText = "Ja, lidt mere",
                        Votes = 28,
                    },
                    new PollOption
                    {
                        Id = 6,
                        PollId = NewPollId,
                        OptionText = "Nej, det nuværende niveau er passende",
                        Votes = 15,
                    },
                    new PollOption
                    {
                        Id = 7,
                        PollId = NewPollId,
                        OptionText = "Nej, vi bør investere mindre",
                        Votes = 8,
                    }
                );
        }
    }
}
