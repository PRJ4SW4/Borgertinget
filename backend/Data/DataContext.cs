using backend.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Collections.Generic; 
using System.Text.Json;         
using System; 
using System.Collections.Generic; 
using System.Linq;

namespace backend.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options) { }

        // --- Dine DbSets ---
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Page> Pages { get; set; } = null!;
        public DbSet<Question> Questions { get; set; } = null!; 
        public DbSet<AnswerOption> AnswerOptions { get; set; } = null!; 
        public DbSet<Flashcard> Flashcards { get; set; } = null!; 
        public DbSet<FlashcardCollection> FlashcardCollections { get; set; } = null!; 
        public DbSet<Tweet> Tweets { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; } = null!; 
        public DbSet<PoliticianTwitterId> PoliticianTwitterIds { get; set; } = null!; 
        public DbSet<Poll> Polls { get; set; } = null!; 
        public DbSet<PollOption> PollOptions { get; set; } = null!; 
        public DbSet<UserVote> UserVotes { get; set; } = null!; 
    public DbSet<Aktor> Aktor {get; set;}
    public DbSet<CalendarEvent> CalendarEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CalendarEvent>().HasIndex(e => e.SourceUrl).IsUnique();

            modelBuilder.Entity<Page>().HasOne(p => p.ParentPage).WithMany(p => p.ChildPages).HasForeignKey(p => p.ParentPageId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Page>().HasMany(p => p.AssociatedQuestions).WithOne(q => q.Page).HasForeignKey(q => q.PageId);
            modelBuilder.Entity<Question>().HasMany(q => q.AnswerOptions).WithOne(o => o.Question).HasForeignKey(o => o.QuestionId);
            modelBuilder.Entity<FlashcardCollection>().HasMany(c => c.Flashcards).WithOne(f => f.FlashcardCollection).HasForeignKey(f => f.CollectionId);
         
            modelBuilder.Entity<Aktor>()
                .Property(a => a.Constituencies) 
                .HasConversion(

                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                );

            modelBuilder.Entity<Aktor>()
                .Property(a => a.Nominations)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                );

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

            modelBuilder.Entity<Page>().HasData(/* ... din page seed data ... */);
            modelBuilder.Entity<Question>().HasData(/* ... din question seed data ... */);
            modelBuilder.Entity<AnswerOption>().HasData(/* ... din answer option seed data ... */);
            modelBuilder.Entity<FlashcardCollection>().HasData(/* ... din collection seed data ... */);
            modelBuilder.Entity<Flashcard>().HasData(/* ... din flashcard seed data ... */);

          
           modelBuilder.Entity<PoliticianTwitterId>(entity =>
            {
                entity.HasIndex(p => p.TwitterUserId).IsUnique();
                entity.HasMany(p => p.Tweets)
                      .WithOne(t => t.Politician)
                      .HasForeignKey(t => t.PoliticianTwitterId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(p => p.Subscriptions)
                      .WithOne(s => s.Politician)
                      .HasForeignKey(s => s.PoliticianTwitterId);
                entity.HasMany(p => p.Polls)
                      .WithOne(p => p.Politician)
                      .HasForeignKey(p => p.PoliticianTwitterId);

                entity.Property(p => p.TwitterUserId).IsRequired();
                entity.Property(p => p.Name).IsRequired();
                entity.Property(p => p.TwitterHandle).IsRequired();

                entity.HasOne(politicianTwitter => politicianTwitter.Aktor) 
                      .WithOne() 
                      .HasForeignKey<PoliticianTwitterId>(politicianTwitter => politicianTwitter.AktorId) 
                      .IsRequired(false) 
                     .OnDelete(DeleteBehavior.SetNull);

                     //ved merge skal nedestående være commented, da der ellers vi blive problemer med constraints i databasen
            /*              entity.HasData(
                   new PoliticianTwitterId { Id = 1, TwitterUserId = "806068174567460864", Name = "Statsministeriet", TwitterHandle = "Statsmin", AktorId = 138 }, 
                   new PoliticianTwitterId { Id = 2, TwitterUserId = "123868861", Name = "Venstre, Danmarks Liberale Parti", TwitterHandle = "venstredk", AktorId = null  }, 
                   new PoliticianTwitterId { Id = 3, TwitterUserId = "2965907578", Name = "Troels Lund Poulsen", TwitterHandle = "troelslundp", AktorId = 206  } 
                 );
            });

            */

            
         



            modelBuilder.Entity<Tweet>(entity =>
            {
                entity.HasIndex(t => new { t.PoliticianTwitterId, t.TwitterTweetId }).IsUnique();
                entity.Property(t => t.TwitterTweetId).IsRequired();
                entity.Property(t => t.Text).IsRequired();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasMany(u => u.Subscriptions)
                      .WithOne(s => s.User)
                      .HasForeignKey(s => s.UserId);
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasIndex(s => s.UserId);
                entity.HasIndex(s => s.PoliticianTwitterId);
                
            });


            modelBuilder.Entity<Poll>(entityPoll =>
            {
                entityPoll.HasOne(poll => poll.Politician)
                      .WithMany(politician => politician.Polls) 
                      .HasForeignKey(poll => poll.PoliticianTwitterId); 
            });


            modelBuilder.Entity<UserVote>()
                .HasIndex(uv => new { uv.UserId, uv.PollId })
                .IsUnique();

            }
           /*
        
            const int SeedPoliticianId = 1; 
            const int SeedPollId = 1;      
            const int NewPollId = 2;

            modelBuilder.Entity<Poll>().HasData(
                new Poll { Id = SeedPollId, Question = "Hvad synes du om den nye bro?", PoliticianTwitterId = SeedPoliticianId, CreatedAt = new DateTime(2025, 4, 15, 10, 0, 0, DateTimeKind.Utc), EndedAt = null },
                new Poll { Id = NewPollId, Question = "Skal Danmark øge investeringer i vedvarende energi?", PoliticianTwitterId = SeedPoliticianId, CreatedAt = new DateTime(2025, 4, 28, 14, 30, 0, DateTimeKind.Utc), EndedAt = null }
            );
            modelBuilder.Entity<PollOption>().HasData(
                new PollOption { Id = 1, PollId = SeedPollId, OptionText = "Den er fantastisk!", Votes = 5 },
                new PollOption { Id = 2, PollId = SeedPollId, OptionText = "Den er ok, men dyr.", Votes = 12 },
                new PollOption { Id = 3, PollId = SeedPollId, OptionText = "Den er unødvendig.", Votes = 3 },
                new PollOption { Id = 4, PollId = NewPollId, OptionText = "Ja, meget mere end nu", Votes = 42 },
                new PollOption { Id = 5, PollId = NewPollId, OptionText = "Ja, lidt mere", Votes = 28 },
                new PollOption { Id = 6, PollId = NewPollId, OptionText = "Nej, det nuværende niveau er passende", Votes = 15 },
                new PollOption { Id = 7, PollId = NewPollId, OptionText = "Nej, vi bør investere mindre", Votes = 8 }
                */
            );
             

        } 
    } 
}