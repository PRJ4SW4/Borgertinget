using backend.Models; // Sørg for at FakePolitiker, FakeParti og PolidleGamemodeTracker er i dette namespace
using Microsoft.EntityFrameworkCore;

// Tilføj using for de nye modeller, hvis de er i et andet namespace
// using YourProject.Models; // Eksempel

namespace backend.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options) { }

    // Eksisterende DbSets
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Page> Pages { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<AnswerOption> AnswerOptions { get; set; }
    public DbSet<Flashcard> Flashcards { get; set; }
    public DbSet<FlashcardCollection> FlashcardCollections { get; set; }

    // --- NYE DbSets for Polidle ---
    // Husk at rette modelnavnet til Politician når du skifter fra FakePolitiker
    public DbSet<FakePolitiker> FakePolitikere { get; set; }
    public DbSet<FakeParti> FakePartier { get; set; } // <--- TILFØJET DbSet for FakeParti
    public DbSet<PolidleGamemodeTracker> GameTrackings { get; set; }
    public DbSet<DailySelection> DailySelections { get; set; }
    // -----------------------------

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Vigtigt at kalde base metoden

        // --- EKSISTERENDE RELATIONER ---
        // Configure the self-referencing relationship for Page
        modelBuilder
            .Entity<Page>()
            .HasOne(p => p.ParentPage) // A page has one parent
            .WithMany(p => p.ChildPages) // A parent can have many children
            .HasForeignKey(p => p.ParentPageId) // The foreign key is ParentPageId
            .OnDelete(DeleteBehavior.Restrict); // Or Cascade, SetNull, etc.

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
        // ---------------------------------


        // --- NY KONFIGURATION for Polidle ---

        // 1. Konfigurer sammensat primærnøgle for PolidleGamemodeTracker
        modelBuilder.Entity<PolidleGamemodeTracker>()
            .HasKey(pgt => new { pgt.PolitikerId, pgt.GameMode }); // Definerer PK som (PolitikerId, GameMode)

        // 2. Konfigurer En-til-mange relation mellem FakePolitiker og PolidleGamemodeTracker
        modelBuilder.Entity<PolidleGamemodeTracker>()
            .HasOne(pgt => pgt.FakePolitiker)      // Fra Tracker peg på én Politiker
            .WithMany(p => p.GameTrackings)        // Fra Politiker peg på mange Trackings
            .HasForeignKey(pgt => pgt.PolitikerId) // Specificer FK kolonnen i Tracker
            .OnDelete(DeleteBehavior.Cascade);     // Eksempel: Slet tracking-rækker hvis politikeren slettes

        // 3. Konfigurer Enum-til-String konvertering for GameMode (Valgfrit, men anbefalet)
        modelBuilder.Entity<PolidleGamemodeTracker>()
           .Property(pgt => pgt.GameMode)
           .HasConversion<string>() // Konverter til string
           .HasMaxLength(50);      // Sæt en passende max længde

        // --- Konfiguration for DailySelection ---
        modelBuilder.Entity<DailySelection>()
            .HasKey(ds => new { ds.SelectionDate, ds.GameMode }); // Sammensat PK

        // Konfigurer relation til FakePolitiker (En DailySelection har én Politiker)
        modelBuilder.Entity<DailySelection>()
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
        // -------------------------------------------------------------------


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
    }
}