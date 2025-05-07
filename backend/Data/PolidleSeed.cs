// backend/Data/PolidleSeed.cs
using backend.Data; 
using backend.Models; // Should contain Aktor, Party, PoliticianQuote, PolidleGamemodeTracker
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
// Removed System.Text.RegularExpressions as it's not used in this simplified version

namespace backend.Data
{
    public class PolidleSeed
    {
        // Portrait file names (still used for seeding byte[])
        private static readonly List<string> MalePortraits = new List<string> { "mand1.png", "mand2.png", "mand3.png", "mand4.png" };
        private static readonly List<string> FemalePortraits = new List<string> { "kvinde1.png", "kvinde2.png", "kvinde3.png" };
        private static readonly string DefaultPortrait = "default_avatar.png";

        // Sample quotes (still used for seeding)
        private static readonly List<string> SampleQuotes = new List<string> {
            "Vi skal styrke fællesskabet og sikre velfærd for alle.",
            "Lavere skatter og mindre bureaukrati er vejen frem for Danmark.",
            "Den grønne omstilling kræver modige beslutninger nu.",
            "Vi må aldrig gå på kompromis med retssikkerheden.",
            "Uddannelse er nøglen til fremtiden for vores unge.",
            "Et stærkt erhvervsliv skaber arbejdspladser og vækst.",
            "Vi skal passe bedre på vores ældre og udsatte borgere.",
            "Danmark skal tage ansvar i en globaliseret verden.",
            "Frihed under ansvar er et grundlæggende princip.",
            "Klimaforandringerne er vor tids største udfordring."
        };

        private static readonly Random random = new Random();

        // Helper function to get a random element
        private static T? GetRandomElement<T>(List<T>? list) 
        {
            if (list == null || list.Count == 0) { return default; } 
            return list[random.Next(list.Count)];
        }

        // Helper function to get portrait image data as byte array
        private static async Task<byte[]> GetPortraitBytesAsync(string? gender, string imageBasePath) // Made gender nullable
        {
            string selectedFileName = DefaultPortrait;
            // Use a default gender if not provided, for selecting a portrait type
            string effectiveGender = gender ?? (random.Next(0, 2) == 0 ? "Mand" : "Kvinde");

            if (effectiveGender == "Mand" && MalePortraits.Any()) { selectedFileName = GetRandomElement(MalePortraits) ?? DefaultPortrait; } 
            else if (effectiveGender == "Kvinde" && FemalePortraits.Any()) { selectedFileName = GetRandomElement(FemalePortraits) ?? DefaultPortrait; } 

            string fullImagePath = Path.Combine(imageBasePath, selectedFileName);

            if (!File.Exists(fullImagePath) && selectedFileName != DefaultPortrait)
            {
                Console.WriteLine($"ADVARSEL: Portrætfil ikke fundet: {fullImagePath}. Prøver default.");
                fullImagePath = Path.Combine(imageBasePath, DefaultPortrait);
            }

            if (File.Exists(fullImagePath))
            {
                try { return await File.ReadAllBytesAsync(fullImagePath); }
                catch (Exception ex) { Console.WriteLine($"FEJL: Kunne ikke læse billedfil {fullImagePath}: {ex.Message}"); return Array.Empty<byte>(); }
            }
            else
            {
                if(selectedFileName == DefaultPortrait) Console.WriteLine($"ADVARSEL: Default portrætfil ikke fundet: {fullImagePath}");
                return Array.Empty<byte>();
            }
        }

        // SeedDataAsync - Updated to only seed photos and quotes for existing Aktors.
        // Assumes Aktors and Parties are populated from API calls.
        public static async Task SeedPolidleDataAsync(DataContext context) 
        {
            string baseDirectory = AppContext.BaseDirectory;
            string imageBasePath = Path.Combine(baseDirectory, "SeedData", "Images");
            Console.WriteLine($"INFO: PolidleSeed: Searching for seed images in: {imageBasePath}");

            // --- Seed/Update Aktor Photos and Quotes ---
            // This part now assumes Aktors (typeid=5) ALREADY EXIST from your API fetch.
            // It will add sample photos and quotes to them if they don't have them.
            Console.WriteLine("INFO: PolidleSeed: Checking existing Aktors (typeid=5) for missing photos and quotes...");
            
            var existingPoliticians = await context.Aktor
                                                .Where(a => a.typeid == 5) // Ensure we are only working with politicians
                                                .Include(a => a.Quotes) 
                                                .ToListAsync(); 

            if (!existingPoliticians.Any())
            {
                Console.WriteLine("INFO: PolidleSeed: No existing politicians (typeid=5) found to seed photos/quotes for. This is okay if API data hasn't been fetched yet or no politicians exist.");
                return;
            }

            int photosAdded = 0;
            int quotesAddedCount = 0;
            bool changesMade = false;

            foreach(var politiker in existingPoliticians)
            {
                // Seed Photo (Portraet byte[]) if it's missing or empty
                // This is useful if your API provides a URL (PictureMiRes) but you want to cache/store the bytes locally.
                // The actual download from PictureMiRes to Portraet should happen in your Aktor import logic.
                // This seed step can be a fallback or for local development if API images aren't fetched.
                if (politiker.Portraet == null || politiker.Portraet.Length == 0)
                {
                    // Use politiker.Sex if available, otherwise make a random guess for a portrait.
                    politiker.Portraet = await GetPortraitBytesAsync(politiker.Sex, imageBasePath);
                    if (politiker.Portraet.Length > 0)
                    {
                        context.Aktor.Update(politiker); 
                        photosAdded++;
                        changesMade = true;
                    }
                }

                // Seed Quotes if none exist for this politician
                if (politiker.Quotes == null || !politiker.Quotes.Any())
                {
                    int numberOfQuotes = random.Next(1, 4); 
                    politiker.Quotes ??= new List<PoliticianQuote>(); 
                    for (int q = 0; q < numberOfQuotes; q++)
                    {
                        string? quoteText = GetRandomElement(SampleQuotes);
                        if (!string.IsNullOrEmpty(quoteText))
                        {
                            // Create new quote and associate it with the politiker's ID
                            var newQuote = new PoliticianQuote { QuoteText = quoteText, PolitikerId = politiker.Id };
                            context.PoliticianQuotes.Add(newQuote); 
                            // politiker.Quotes.Add(newQuote); // EF Core should handle this if newQuote.PolitikerId is set
                                                            // and politiker entity is tracked. Adding to context directly is safer.
                            quotesAddedCount++;
                            changesMade = true;
                        }
                    }
                }
            }

            if (changesMade)
            {
                try
                {
                    await context.SaveChangesAsync();
                    Console.WriteLine($"INFO: PolidleSeed: Update complete. Added {photosAdded} photos and {quotesAddedCount} quotes to existing Aktors.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: PolidleSeed: Failed to save changes for photos/quotes. {ex.Message}");
                }
            }
            else
            {
                 Console.WriteLine("INFO: PolidleSeed: No missing photos or quotes found for existing Aktors, or no changes made.");
            }
        }
    }
}
