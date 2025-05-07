// backend/Data/DataContext.cs
using System.Collections.Generic; 
// using System.Text.Json; // Removed duplicate
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json; 
using backend.DTO.Calendar;
using backend.DTO.LearningEnvironment;
using backend.Models; 
using backend.Models.Calendar;
using backend.Models.Flashcards;
using backend.Models.LearningEnvironment;
// Removed BCrypt.Net using as it's not used here
using Microsoft.EntityFrameworkCore;


namespace backend.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Page> Pages { get; set; } = null!; // Added = null!
    public DbSet<Question> Questions { get; set; } = null!; // Added = null!
    public DbSet<AnswerOption> AnswerOptions { get; set; } = null!; // Added = null!
    public DbSet<Flashcard> Flashcards { get; set; } = null!; // Added = null!
    public DbSet<FlashcardCollection> FlashcardCollections { get; set; } = null!; // Added = null!
    public DbSet<CalendarEvent> CalendarEvents { get; set; } = null!; // Added = null!
    public DbSet<Party> Party {get; set;} = null!; // Added = null!
    public DbSet<Aktor> Aktor { get; set; } = null!; // Added = null!

    // DbSets for Polidle - these should now use Aktor and Party
    // public DbSet<FakePolitiker> FakePolitikere { get; set; } // REMOVE OR COMMENT OUT
    // public DbSet<FakeParti> FakePartier { get; set; } // REMOVE OR COMMENT OUT
    public DbSet<PolidleGamemodeTracker> GameTrackings { get; set; } = null!; // Added = null!
    public DbSet<DailySelection> DailySelections { get; set; } = null!; // Added = null!
    public DbSet<PoliticianQuote> PoliticianQuotes { get; set; } = null!; // Added = null!
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); 

        modelBuilder.Entity<CalendarEvent>().HasIndex(e => e.SourceUrl).IsUnique();

        modelBuilder
            .Entity<Page>()
            .HasOne(p => p.ParentPage) 
            .WithMany(p => p.ChildPages) 
            .HasForeignKey(p => p.ParentPageId) 
            .OnDelete(DeleteBehavior.Cascade); 

        modelBuilder
            .Entity<Page>()
            .HasMany(p => p.AssociatedQuestions) 
            .WithOne(q => q.Page) 
            .HasForeignKey(q => q.PageId);

        modelBuilder
            .Entity<Question>()
            .HasMany(q => q.AnswerOptions)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId);

        modelBuilder
            .Entity<FlashcardCollection>()
            .HasMany(c => c.Flashcards)
            .WithOne(f => f.FlashcardCollection)
            .HasForeignKey(f => f.CollectionId);

        // Aktor JSON property conversions
         modelBuilder.Entity<Aktor>().Property(a => a.Constituencies).HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
            modelBuilder.Entity<Aktor>().Property(a => a.Nominations).HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
            modelBuilder.Entity<Aktor>().Property(a => a.Educations).HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
            modelBuilder.Entity<Aktor>().Property(a => a.Occupations).HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
            modelBuilder.Entity<Aktor>().Property(a => a.PublicationTitles).HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
            modelBuilder.Entity<Aktor>().Property(a => a.Ministers).HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
            modelBuilder.Entity<Aktor>().Property(a => a.Spokesmen).HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()); 
            modelBuilder.Entity<Aktor>().Property(a => a.ParliamentaryPositionsOfTrust).HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()); 

            // Party Configurations
            modelBuilder.Entity<Party>(entity =>
            {
                entity.Property(p => p.stats) 
                       .HasConversion(
                           v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                           v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                       );

                entity.Property(p => p.memberIds) 
                      .HasConversion(
                          v => JsonSerializer.Serialize(v ?? new List<int>(), (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new List<int>(),
                          new ValueComparer<List<int>?>(
                              (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                              c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                              c => c == null ? null : c.ToList()))
                      .HasColumnType("text"); 
            });
        
            // Relationship between Aktor and Party
            modelBuilder.Entity<Aktor>()
                .HasOne(a => a.MembersOfParty)       // Navigation property in Aktor model
                .WithMany()                         // Assuming Party does not have an ICollection<Aktor> Members
                .HasForeignKey(a => a.partyId)      // Foreign key in Aktor model
                .OnDelete(DeleteBehavior.Restrict); // Or SetNull if an Aktor can exist without a party

            #region Polidle Configurations
            modelBuilder.Entity<PolidleGamemodeTracker>()
                .HasKey(pgt => new { pgt.PolitikerId, pgt.GameMode }); 

            modelBuilder.Entity<PolidleGamemodeTracker>()
                .HasOne(pgt => pgt.Aktor)      
                .WithMany(a => a.GameTrackings) 
                .HasForeignKey(pgt => pgt.PolitikerId) 
                .OnDelete(DeleteBehavior.Cascade);     

            modelBuilder.Entity<PolidleGamemodeTracker>()
               .Property(pgt => pgt.GameMode)
               .HasConversion<string>() 
               .HasMaxLength(50);      

            modelBuilder.Entity<DailySelection>()
                .HasKey(ds => new { ds.SelectionDate, ds.GameMode }); 

            modelBuilder.Entity<DailySelection>()
                .HasOne(ds => ds.SelectedPolitiker) 
                .WithMany() 
                .HasForeignKey(ds => ds.SelectedPolitikerID) 
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<DailySelection>()
                .Property(ds => ds.GameMode)
                .HasConversion<string>()
                .HasMaxLength(50);
            
            modelBuilder.Entity<PoliticianQuote>()
                .HasKey(q => q.QuoteId); 

            modelBuilder.Entity<PoliticianQuote>()
                .HasOne(q => q.Aktor)    
                .WithMany(a => a.Quotes) 
                .HasForeignKey(q => q.PolitikerId) 
                .OnDelete(DeleteBehavior.Cascade); 
            #endregion

        // Seeding for Learning Environment (if you still have this static data)
        // Example:
        // modelBuilder.Entity<Page>().HasData( /* ... your Page data ... */ );
        // modelBuilder.Entity<Question>().HasData( /* ... your Question data ... */ );
        // modelBuilder.Entity<AnswerOption>().HasData( /* ... your AnswerOption data ... */ );
        // modelBuilder.Entity<FlashcardCollection>().HasData( /* ... your FlashcardCollection data ... */ );
        // modelBuilder.Entity<Flashcard>().HasData( /* ... your Flashcard data ... */ );
    }
}
