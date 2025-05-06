using System.Collections.Generic; // Required
using System.Text.Json;          // Required
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json; // Required
using backend.DTO.Calendar;
using backend.DTO.LearningEnvironment;
using backend.Models; // Sørg for at FakePolitiker, FakeParti og PolidleGamemodeTracker er i dette namespace
using backend.Models.Calendar;
using backend.Models.Flashcards;
using backend.Models.LearningEnvironment;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace backend.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options) { }

    // Eksisterende DbSets
    public DbSet<User> Users { get; set; } = null!;

    // --- Learning Environment Setup ---
    public DbSet<Page> Pages { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<AnswerOption> AnswerOptions { get; set; }
    public DbSet<Flashcard> Flashcards { get; set; }
    public DbSet<FlashcardCollection> FlashcardCollections { get; set; }

    // --- /Learning Environment Setup ---

    // --- Calendar Setup ---
    public DbSet<Party> Party { get; set; }
    public DbSet<Aktor> Aktor { get; set; }
    public DbSet<CalendarEvent> CalendarEvents { get; set; }

    // --- /Calendar Setup ---

    public DbSet<Tweet> Tweets { get; set; }

    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<PoliticianTwitterId> PoliticianTwitterIds { get; set; }

    public DbSet<Poll> Polls { get; set; }
    public DbSet<PollOption> PollOptions { get; set; }
    public DbSet<UserVote> UserVotes { get; set; }

    // --- NYE DbSets for Polidle ---
    // Husk at rette modelnavnet til Politician når du skifter fra FakePolitiker
    public DbSet<FakePolitiker> FakePolitikere { get; set; }
    public DbSet<FakeParti> FakePartier { get; set; } // <--- TILFØJET DbSet for FakeParti
    public DbSet<PolidleGamemodeTracker> GameTrackings { get; set; }
    public DbSet<DailySelection> DailySelections { get; set; }
    public DbSet<PoliticianQuote> PoliticianQuotes { get; set; }

    // -----------------------------

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Vigtigt at kalde base metoden

        // --- Calendar Setup ---
        // Index for the CalendarEvents SourceUrl to make syncing events faster
        modelBuilder.Entity<CalendarEvent>().HasIndex(e => e.SourceUrl).IsUnique();

        // --- /Calendar Setup ---

        // --- Learning Environment Setup ---

        // --- EKSISTERENDE RELATIONER ---
        // Configure the self-referencing relationship for Page
        modelBuilder
            .Entity<Page>()
            .HasOne(p => p.ParentPage) // A page has one parent
            .WithMany(p => p.ChildPages) // A parent can have many children
            .HasForeignKey(p => p.ParentPageId) // The foreign key is ParentPageId
            .OnDelete(DeleteBehavior.Cascade); // Cascade deletions. Can be changed

        // Configure Page <-> Question relationship
        modelBuilder
            .Entity<Page>()
            .HasMany(p => p.AssociatedQuestions) // Use the ICollection property in Page
            .WithOne(q => q.Page) // Use the navigation property back to Page in Question
            .HasForeignKey(q => q.PageId);

        // Configure Question <-> AnswerOption relationship
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
        
            
            modelBuilder.Entity<Party>(entity =>
            {
                // Configure Role relationships (as shown previously)
                 entity.HasOne(p => p.chairman)
                      .WithMany()
                      .HasForeignKey(p => p.chairmanId)
                      .OnDelete(DeleteBehavior.SetNull);
                 // ... configure other roles (ViceChairman, Secretary, Spokesman) ...

                 // Configure Stats List conversion (if kept)
                 entity.Property(p => p.stats) // Assuming PascalCase naming
                       .HasConversion(
                           v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                           v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                       );

                // *** Add Configuration for memberIds List ***
                entity.Property(p => p.memberIds) // Use PascalCase property name
                      .HasConversion(
                          // Convert List<int> to JSON string for DB
                          v => JsonSerializer.Serialize(v ?? new List<int>(), (JsonSerializerOptions?)null),
                          // Convert JSON string from DB back to List<int>
                          v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new List<int>(),
                          // Add a ValueComparer to help EF Core detect changes correctly
                          new ValueComparer<List<int>?>(
                              (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                              c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                              c => c == null ? null : c.ToList()))
                      .HasColumnType("text"); // Use jsonb for efficient querying in PostgreSQL if needed, or "text"
                // *** End Configuration for memberIds List ***
            });
                    // ---------------------------------

#region Polidle
        // --- NY KONFIGURATION for Polidle ---

        // --- Learning Environment Seeding ---
        #region Polidle
        // --- NY KONFIGURATION for Polidle ---

        // 1. Konfigurer sammensat primærnøgle for PolidleGamemodeTracker
        modelBuilder.Entity<PolidleGamemodeTracker>()
            .HasKey(pgt => new { pgt.PolitikerId, pgt.GameMode }); // Definerer PK som (PolitikerId, GameMode)

        // 2. Konfigurer En-til-mange relation mellem FakePolitiker og PolidleGamemodeTracker
        modelBuilder
            .Entity<PolidleGamemodeTracker>()
            .HasOne(pgt => pgt.FakePolitiker) // Fra Tracker peg på én Politiker
            .WithMany(p => p.GameTrackings) // Fra Politiker peg på mange Trackings
            .HasForeignKey(pgt => pgt.PolitikerId) // Specificer FK kolonnen i Tracker
            .OnDelete(DeleteBehavior.Cascade); // Eksempel: Slet tracking-rækker hvis politikeren slettes

        // 3. Konfigurer Enum-til-String konvertering for GameMode (Valgfrit, men anbefalet)
        modelBuilder
            .Entity<PolidleGamemodeTracker>()
            .Property(pgt => pgt.GameMode)
            .HasConversion<string>() // Konverter til string
            .HasMaxLength(50); // Sæt en passende max længde

        // --- Konfiguration for DailySelection ---
        modelBuilder.Entity<DailySelection>().HasKey(ds => new { ds.SelectionDate, ds.GameMode }); // Sammensat PK

        // Konfigurer relation til FakePolitiker (En DailySelection har én Politiker)
        modelBuilder
            .Entity<DailySelection>()
            .HasOne(ds => ds.SelectedPolitiker)
            .WithMany() // En politiker kan være valgt mange gange (over tid/gamemodes)
            .HasForeignKey(ds => ds.SelectedPolitikerID)
            .OnDelete(DeleteBehavior.Restrict); // Undgå at slette en politiker hvis de er et dagligt valg?

        // Konfigurer evt. Enum-til-String for GameMode her også, hvis den ikke er globalt konfigureret
        modelBuilder.Entity<DailySelection>()
            .Property(ds => ds.GameMode)
            .HasConversion<string>()
            .HasMaxLength(50);
        // ------------------------------------

        // --- NYT: Konfiguration for FakeParti <-> FakePolitiker relation ---
        // Antagelser:
        // 1. Din `FakePolitiker` model har en `int PartiId` foreign key property.
        // 2. Din `FakePolitiker` model har en `FakeParti FakeParti` navigation property.
        // Juster `.HasForeignKey()` og `.WithOne()` hvis dine properties hedder noget andet.

        modelBuilder.Entity<FakeParti>()
            .HasMany(p => p.FakePolitikers)
            .WithOne(fp => fp.FakeParti) // Matcher navigation property i FakePolitiker
            .HasForeignKey(fp => fp.PartiId) // Matcher foreign key property i FakePolitiker
            .OnDelete(DeleteBehavior.Restrict);


        // === NYT: Konfiguration for FakePolitiker <-> PoliticianQuote relation ===
            modelBuilder.Entity<PoliticianQuote>()
                .HasOne(q => q.FakePolitiker)    // Et citat har én politiker
                .WithMany(p => p.Quotes)        // En politiker har mange citater (via 'Quotes' collection i FakePolitiker)
                .HasForeignKey(q => q.PolitikerId) // Fremmednøglen i PoliticianQuote tabellen
                .OnDelete(DeleteBehavior.Cascade); // Slet citater hvis politikeren slettes
        // -------------------------------------------------------------------
#endregion

        // --- EKSISTERENDE SEED DATA ---
        // Behold al din eksisterende .HasData(...) konfiguration for Pages, Questions, etc.
        modelBuilder.Entity<Page>().HasData( /* ... dine Page data ... */ );
        modelBuilder.Entity<Question>().HasData( /* ... dine Question data ... */ );
        modelBuilder.Entity<AnswerOption>().HasData( /* ... dine AnswerOption data ... */ );
        modelBuilder.Entity<FlashcardCollection>().HasData( /* ... dine FlashcardCollection data ... */ );
        modelBuilder.Entity<Flashcard>().HasData( /* ... dine Flashcard data ... */ );
        // Tilføj evt. seed data for FakeParti og FakePolitiker her, hvis nødvendigt
        // modelBuilder.Entity<FakeParti>().HasData( /* ... dine FakeParti data ... */ );
        // modelBuilder.Entity<FakePolitiker>().HasData( /* ... dine FakePolitiker data ... */ );
        // -----------------------------
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

        // === Konfiguration for PoliticianTwitterId ===
        modelBuilder.Entity<PoliticianTwitterId>(entity =>
        {
            // ... (din eksisterende konfiguration for index, relationer, required fields) ...
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
            entity.Property(p => p.TwitterUserId).IsRequired();
            entity.Property(p => p.Name).IsRequired();
            entity.Property(p => p.TwitterHandle).IsRequired();

            // --- SEED POLITICIAN DATA ---
            entity.HasData(
                new PoliticianTwitterId
                {
                    Id = 1,
                    TwitterUserId = "806068174567460864",
                    Name = "Statsministeriet",
                    TwitterHandle = "Statsmin",
                },
                new PoliticianTwitterId
                {
                    Id = 2,
                    TwitterUserId = "123868861",
                    Name = "Venstre, Danmarks Liberale Parti",
                    TwitterHandle = "venstredk",
                },
                new PoliticianTwitterId
                {
                    Id = 3,
                    TwitterUserId = "2965907578",
                    Name = "Troels Lund Poulsen",
                    TwitterHandle = "troelslundp",
                }
            );
        });

        modelBuilder.Entity<Tweet>(entity =>
        {
            entity.HasIndex(t => new { t.PoliticianTwitterId, t.TwitterTweetId }).IsUnique();
            entity.Property(t => t.TwitterTweetId).IsRequired();
            entity.Property(t => t.Text).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasMany(u => u.Subscriptions).WithOne(s => s.User).HasForeignKey(s => s.UserId);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasIndex(s => s.UserId);
            entity.HasIndex(s => s.PoliticianTwitterId);
            entity.HasData(
                new Subscription
                {
                    Id = 1,
                    UserId = 1,
                    PoliticianTwitterId = 1,
                },
                new Subscription
                {
                    Id = 2,
                    UserId = 1,
                    PoliticianTwitterId = 2,
                },
                new Subscription
                {
                    Id = 3,
                    UserId = 1,
                    PoliticianTwitterId = 3,
                }
            );
        });

        modelBuilder
            .Entity<UserVote>() // Vælg UserVote entiteten
            .HasIndex(uv => new { uv.UserId, uv.PollId }) // Definer et index på disse to kolonner
            .IsUnique(); // Specificer at dette index skal være unikt
        // Konfigurer evt. Enum-til-String for GameMode her også, hvis den ikke er globalt konfigureret
        modelBuilder
            .Entity<DailySelection>()
            .Property(ds => ds.GameMode)
            .HasConversion<string>()
            .HasMaxLength(50);
        // ------------------------------------

        // --- NYT: Konfiguration for FakeParti <-> FakePolitiker relation ---
        // Antagelser:
        // 1. Din `FakePolitiker` model har en `int PartiId` foreign key property.
        // 2. Din `FakePolitiker` model har en `FakeParti FakeParti` navigation property.
        // Juster `.HasForeignKey()` og `.WithOne()` hvis dine properties hedder noget andet.

        modelBuilder
            .Entity<FakeParti>()
            .HasMany(p => p.FakePolitikers)
            .WithOne(fp => fp.FakeParti) // Matcher navigation property i FakePolitiker
            .HasForeignKey(fp => fp.PartiId) // Matcher foreign key property i FakePolitiker
            .OnDelete(DeleteBehavior.Restrict);

        // === NYT: Konfiguration for FakePolitiker <-> PoliticianQuote relation ===
        modelBuilder
            .Entity<PoliticianQuote>()
            .HasOne(q => q.FakePolitiker) // Et citat har én politiker
            .WithMany(p => p.Quotes) // En politiker har mange citater (via 'Quotes' collection i FakePolitiker)
            .HasForeignKey(q => q.PolitikerId) // Fremmednøglen i PoliticianQuote tabellen
            .OnDelete(DeleteBehavior.Cascade); // Slet citater hvis politikeren slettes
        // -------------------------------------------------------------------
        #endregion

        // --- EKSISTERENDE SEED DATA ---
        // Behold al din eksisterende .HasData(...) konfiguration for Pages, Questions, etc.
        modelBuilder
            .Entity<Page>()
            .HasData( /* ... dine Page data ... */
            );
        modelBuilder
            .Entity<Question>()
            .HasData( /* ... dine Question data ... */
            );
        modelBuilder
            .Entity<AnswerOption>()
            .HasData( /* ... dine AnswerOption data ... */
            );
        modelBuilder
            .Entity<FlashcardCollection>()
            .HasData( /* ... dine FlashcardCollection data ... */
            );
        modelBuilder
            .Entity<Flashcard>()
            .HasData( /* ... dine Flashcard data ... */
            );
        // Tilføj evt. seed data for FakeParti og FakePolitiker her, hvis nødvendigt
        // modelBuilder.Entity<FakeParti>().HasData( /* ... dine FakeParti data ... */ );
        // modelBuilder.Entity<FakePolitiker>().HasData( /* ... dine FakePolitiker data ... */ );
        // -----------------------------
    }
}
