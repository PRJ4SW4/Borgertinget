using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<Page> Pages { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<AnswerOption> AnswerOptions { get; set; }
    public DbSet<Flashcard> Flashcards { get; set; }
    public DbSet<FlashcardCollection> FlashcardCollections { get; set; }


    public DbSet<Tweet> Tweets { get; set; }

    public DbSet<Subscription> Subscriptions { get; set; } 
    public DbSet<PoliticianTwitterId> PoliticianTwitterIds { get; set; }  

    
    public DbSet<Poll> Polls { get; set; }
    public DbSet<PollOption> PollOptions { get; set; }
    public DbSet<UserVote> UserVotes { get; set; }




    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the self-referencing relationship
        modelBuilder
            .Entity<Page>()
            .HasOne(p => p.ParentPage) // A page has one parent
            .WithMany(p => p.ChildPages) // A parent can have many children
            .HasForeignKey(p => p.ParentPageId) // The foreign key is ParentPageId
            .OnDelete(DeleteBehavior.Restrict); // Or Cascade, SetNull, etc. depending on desired behavior when a parent is deleted

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

        // --- SEED DATA ---

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

            
          // === Konfiguration for PoliticianTwitterId ===
        modelBuilder.Entity<PoliticianTwitterId>(entity =>
        {
            // ... (din eksisterende konfiguration for index, relationer, required fields) ...
            entity.HasIndex(p => p.TwitterUserId).IsUnique();
            entity.HasMany(p => p.Tweets)
                  .WithOne(t => t.Politician)
                  .HasForeignKey(t => t.PoliticianTwitterId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(p => p.Subscriptions)
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
                    TwitterHandle = "Statsmin"
                },
                new PoliticianTwitterId
                {
                    Id = 2, 
                    TwitterUserId = "123868861",
                    Name = "Venstre, Danmarks Liberale Parti",
                    TwitterHandle = "venstredk"
                },
                new PoliticianTwitterId
                {
                    Id = 3,
                    TwitterUserId = "2965907578",
                    Name = "Troels Lund Poulsen",
                    TwitterHandle = "troelslundp"
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
            
                entity.HasMany(u => u.Subscriptions)        
                    .WithOne(s => s.User)               
                    .HasForeignKey(s => s.UserId);     
                

            
             
           
        });

        
            
            modelBuilder.Entity<Subscription>(entity =>
            {
                
                

                entity.HasIndex(s => s.UserId);
                entity.HasIndex(s => s.PoliticianTwitterId);
            entity.HasData(
            
             new Subscription { Id = 1, UserId = 1, PoliticianTwitterId = 1 },
             
             new Subscription { Id = 2, UserId = 1, PoliticianTwitterId = 2 },
             
             new Subscription { Id = 3, UserId = 1, PoliticianTwitterId = 3 }
             );

            });

              modelBuilder.Entity<UserVote>() // Vælg UserVote entiteten
            .HasIndex(uv => new { uv.UserId, uv.PollId }) // Definer et index på disse to kolonner
            .IsUnique(); // Specificer at dette index skal være unikt
        }
}