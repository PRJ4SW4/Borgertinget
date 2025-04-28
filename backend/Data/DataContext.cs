using backend.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Collections.Generic; // Required
using System.Text.Json;          // Required
using System; // Til DateTimeKind
using System.Collections.Generic; // Til List<> i modeller
using System.Linq; // Bruges ikke direkte her, men god at have

namespace backend.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options) { }

        // --- Dine DbSets ---
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Page> Pages { get; set; } = null!; // Tilføjet '= null!' for konsistens
        public DbSet<Question> Questions { get; set; } = null!; // Tilføjet '= null!'
        public DbSet<AnswerOption> AnswerOptions { get; set; } = null!; // Tilføjet '= null!'
        public DbSet<Flashcard> Flashcards { get; set; } = null!; // Tilføjet '= null!'
        public DbSet<FlashcardCollection> FlashcardCollections { get; set; } = null!; // Tilføjet '= null!'
        public DbSet<Tweet> Tweets { get; set; } = null!; // Tilføjet '= null!'
        public DbSet<Subscription> Subscriptions { get; set; } = null!; // Tilføjet '= null!'
        public DbSet<PoliticianTwitterId> PoliticianTwitterIds { get; set; } = null!; // Tilføjet '= null!'
        public DbSet<Poll> Polls { get; set; } = null!; // Tilføjet '= null!'
        public DbSet<PollOption> PollOptions { get; set; } = null!; // Tilføjet '= null!'
        public DbSet<UserVote> UserVotes { get; set; } = null!; // Tilføjet '= null!'
        // '= null!' undertrykker en compile-warning om non-nullable properties uden constructor initialization
    public DbSet<Aktor> Aktor {get; set;}
    public DbSet<CalendarEvent> CalendarEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // KALD ALTID DENNE FØRST
            base.OnModelCreating(modelBuilder);

        // Index for the CalendarEvents SourceUrl to make syncing events faster
        modelBuilder.Entity<CalendarEvent>().HasIndex(e => e.SourceUrl).IsUnique();

            // --- Dine Eksisterende Konfigurationer (Page, Question, AnswerOption, Flashcard) ---
            modelBuilder.Entity<Page>().HasOne(p => p.ParentPage).WithMany(p => p.ChildPages).HasForeignKey(p => p.ParentPageId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Page>().HasMany(p => p.AssociatedQuestions).WithOne(q => q.Page).HasForeignKey(q => q.PageId);
            modelBuilder.Entity<Question>().HasMany(q => q.AnswerOptions).WithOne(o => o.Question).HasForeignKey(o => o.QuestionId);
            modelBuilder.Entity<FlashcardCollection>().HasMany(c => c.Flashcards).WithOne(f => f.FlashcardCollection).HasForeignKey(f => f.CollectionId);
         
        // Configure Constituencies
            modelBuilder.Entity<Aktor>()
                .Property(a => a.Constituencies) // Target the List<string> property
                .HasConversion(
                    // Convert List<string> to json string for DB
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    // Convert json string from DB back to List<string>
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                );

            // Configure Nominations
            modelBuilder.Entity<Aktor>()
                .Property(a => a.Nominations)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                );

            // Add similar .HasConversion calls for Educations and Occupations
            modelBuilder.Entity<Aktor>()
                .Property(a => a.Educations)
                .HasConversion(
                     v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                     v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                 );

            modelBuilder.Entity<Aktor>()
                .Property(a => a.Occupations)
                .HasConversion(
                     v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                     v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                 );

            modelBuilder.Entity<Aktor>()
                 .Property(a => a.PublicationTitles)
                 .HasConversion(
                     v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                     v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                 );
            modelBuilder.Entity<Aktor>()
                 .Property(a => a.Ministers)
                 .HasConversion(
                     v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                     v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                 );
            modelBuilder.Entity<Aktor>()
                 .Property(a => a.Spokesmen)
                 .HasConversion(
                     v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                     v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                 ); 

            // --- Dine Eksisterende Seeding (Page, Question, AnswerOption, Flashcard) ---
            modelBuilder.Entity<Page>().HasData(/* ... din page seed data ... */);
            modelBuilder.Entity<Question>().HasData(/* ... din question seed data ... */);
            modelBuilder.Entity<AnswerOption>().HasData(/* ... din answer option seed data ... */);
            modelBuilder.Entity<FlashcardCollection>().HasData(/* ... din collection seed data ... */);
            modelBuilder.Entity<Flashcard>().HasData(/* ... din flashcard seed data ... */);

            // === Konfiguration for PoliticianTwitterId ===
            modelBuilder.Entity<PoliticianTwitterId>(entity =>
            {
                entity.HasIndex(p => p.TwitterUserId).IsUnique();
                // Tweet relation
                entity.HasMany(p => p.Tweets)
                      .WithOne(t => t.Politician)
                      .HasForeignKey(t => t.PoliticianTwitterId)
                      .OnDelete(DeleteBehavior.Cascade); // Beholder Cascade som du havde
                // Subscription relation
                entity.HasMany(p => p.Subscriptions)
                      .WithOne(s => s.Politician)
                      .HasForeignKey(s => s.PoliticianTwitterId); // Standard er Restrict
                // Poll relation (Inverse side)
                entity.HasMany(p => p.Polls) // Sørg for List<Poll> Polls findes i PoliticianTwitterId.cs
                      .WithOne(p => p.Politician)
                      .HasForeignKey(p => p.PoliticianTwitterId); // Matcher FK i Poll

                // Required Properties
                entity.Property(p => p.TwitterUserId).IsRequired();
                entity.Property(p => p.Name).IsRequired();
                entity.Property(p => p.TwitterHandle).IsRequired();

                // --- SEED POLITICIAN DATA --- (Beholder din seed data)
                entity.HasData(
                     new PoliticianTwitterId { Id = 1, TwitterUserId = "806068174567460864", Name = "Statsministeriet", TwitterHandle = "Statsmin" },
                     new PoliticianTwitterId { Id = 2, TwitterUserId = "123868861", Name = "Venstre, Danmarks Liberale Parti", TwitterHandle = "venstredk" },
                     new PoliticianTwitterId { Id = 3, TwitterUserId = "2965907578", Name = "Troels Lund Poulsen", TwitterHandle = "troelslundp" }
                );
                // FJERNET: Det ekstra base.OnModelCreating(modelBuilder); kald herfra
            });


            // Configure Tweet (Index, Required)
            modelBuilder.Entity<Tweet>(entity =>
            {
                entity.HasIndex(t => new { t.PoliticianTwitterId, t.TwitterTweetId }).IsUnique();
                entity.Property(t => t.TwitterTweetId).IsRequired();
                entity.Property(t => t.Text).IsRequired();
            });

            // Configure User (Relations)
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasMany(u => u.Subscriptions)
                      .WithOne(s => s.User)
                      .HasForeignKey(s => s.UserId);
            });

            // Configure Subscription (Indexes, Seeding)
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasIndex(s => s.UserId);
                entity.HasIndex(s => s.PoliticianTwitterId);
                // Beholder din seed data
                entity.HasData(
                 new Subscription { Id = 1, UserId = 1, PoliticianTwitterId = 1 },
                 new Subscription { Id = 2, UserId = 1, PoliticianTwitterId = 2 },
                 new Subscription { Id = 3, UserId = 1, PoliticianTwitterId = 3 }
                );
            });


            // === Eksplicit Poll <-> Politician configuration (FLYTET HERUD) ===
            modelBuilder.Entity<Poll>(entityPoll =>
            {
                entityPoll.HasOne(poll => poll.Politician)
                      .WithMany(politician => politician.Polls) // Kræver List<Poll> Polls i PoliticianTwitterId.cs
                      .HasForeignKey(poll => poll.PoliticianTwitterId); // Peger på FK i Poll.cs
            });
            // ==================================================================


            // Configure UserVote (Unique Index)
            modelBuilder.Entity<UserVote>()
                .HasIndex(uv => new { uv.UserId, uv.PollId })
                .IsUnique();


            // --- SEEDING AF POLL OG POLLOPTIONS --- (Beholder din seed data)
            const int SeedPoliticianId = 1; // Skal matche et eksisterende Politician ID (f.eks. fra seed ovenfor)
            const int SeedPollId = 1;       // Unikt ID for denne Poll

            modelBuilder.Entity<Poll>().HasData(
                new Poll { Id = SeedPollId, Question = "Hvad synes du om den nye bro?", PoliticianTwitterId = SeedPoliticianId, CreatedAt = new DateTime(2025, 4, 15, 10, 0, 0, DateTimeKind.Utc), EndedAt = null }
            );
            modelBuilder.Entity<PollOption>().HasData(
                new PollOption { Id = 1, PollId = SeedPollId, OptionText = "Den er fantastisk!", Votes = 5 },
                new PollOption { Id = 2, PollId = SeedPollId, OptionText = "Den er ok, men dyr.", Votes = 12 },
                new PollOption { Id = 3, PollId = SeedPollId, OptionText = "Den er unødvendig.", Votes = 3 }
            );
             // ---------------------------------------

        } // Slut på OnModelCreating
    } // Slut på DataContext
} // Slut på namespace