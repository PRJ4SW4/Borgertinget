using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO; // <-- TILFØJET for fil-operationer
using System.Linq;
using System.Threading.Tasks;

namespace backend.Data
{
    public class PolidleSeed
    {
        // Lister til navne, uddannelser, regioner (uændret)
        private static readonly List<string> FirstNames = new List<string> {
            "Anders", "Mette", "Lars", "Pia", "Sofie", "Jakob", "Ida", "Morten", "Pernille", "Kristian",
            "Camilla", "Peter", "Signe", "Jens", "Louise", "Søren", "Laura", "Mikkel", "Anna", "Rasmus"
        };
        private static readonly List<string> LastNames = new List<string> {
            "Nielsen", "Jensen", "Hansen", "Pedersen", "Andersen", "Christensen", "Larsen", "Sørensen", "Rasmussen", "Jørgensen",
            "Olsen", "Thomsen", "Kristiansen", "Poulsen", "Johansen", "Knudsen", "Mortensen", "Møller", "Madsen", "Bruun"
        };

        private static readonly List<string> Educations = new List<string> {
            "Cand.polit.", "Cand.merc.", "Jurist", "Folkeskolelærer", "Sygeplejerske", "Ingeniør", "Pædagog",
            "Statskundskab", "Historiker", "Journalist", "Landmand", "Håndværker", "Sociolog", "Økonomi", "IT-uddannet"
        };
        private static readonly List<string> Regions = new List<string> {
            "Region Hovedstaden", "Region Sjælland", "Region Syddanmark", "Region Midtjylland", "Region Nordjylland",
            "København", "Aarhus", "Odense", "Aalborg", "Esbjerg", "Randers", "Kolding", "Vejle", "Horsens", "Roskilde"
        };

        // --- NYT: Lister med portræt-filnavne ---
        private static readonly List<string> MalePortraits = new List<string> { "mand1.png", "mand2.png", "mand3.png", "mand4.png" }; // Tilføj flere filnavne her
        private static readonly List<string> FemalePortraits = new List<string> { "kvinde1.png", "kvinde2.png", "kvinde3.png" }; // Tilføj flere filnavne her
        private static readonly string DefaultPortrait = "default_avatar.png"; // Valgfrit: Et fallback-billede

        private static readonly Random random = new Random();

        private static T GetRandomElement<T>(List<T> list)
        {
             if (list == null || list.Count == 0)
            {
                // Returner en default værdi eller kast en exception, afhængig af kontekst
                 return default(T); // Eller throw new InvalidOperationException("Listen må ikke være tom.");
            }
            return list[random.Next(list.Count)];
        }

        private static string GenerateRandomName()
        {
            return $"{GetRandomElement(FirstNames)} {GetRandomElement(LastNames)}";
        }

        // --- NYT: Helper til at læse billedfil ---
        private static async Task<byte[]> GetPortraitBytesAsync(string gender, string imageBasePath)
        {
            string selectedFileName = DefaultPortrait; // Start med fallback

            // Vælg en tilfældig fil baseret på køn
            if (gender == "Mand" && MalePortraits.Any())
            {
                selectedFileName = GetRandomElement(MalePortraits);
            }
            else if (gender == "Kvinde" && FemalePortraits.Any())
            {
                selectedFileName = GetRandomElement(FemalePortraits);
            }

            string fullImagePath = Path.Combine(imageBasePath, selectedFileName);

            // Tjek om filen eksisterer, ellers prøv fallback (hvis det ikke allerede er den)
            if (!File.Exists(fullImagePath) && selectedFileName != DefaultPortrait)
            {
                 Console.WriteLine($"WARNING: Portrait file not found: {fullImagePath}. Trying default.");
                 fullImagePath = Path.Combine(imageBasePath, DefaultPortrait);
            }

            // Læs filen hvis den findes
            if (File.Exists(fullImagePath))
            {
                try
                {
                    return await File.ReadAllBytesAsync(fullImagePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Could not read image file {fullImagePath}: {ex.Message}");
                    return Array.Empty<byte>(); // Returner tom ved fejl
                }
            }
            else
            {
                // Hvis selv fallback-billedet ikke findes
                if(selectedFileName == DefaultPortrait)
                    Console.WriteLine($"WARNING: Default portrait file not found: {fullImagePath}");
                return Array.Empty<byte>(); // Returner tom hvis intet billede findes
            }
        }


        public static async Task SeedDataAsync(DataContext context, int numberOfPoliticians = 50)
        {
             // --- Bestem base path for billeder ---
            // AppContext.BaseDirectory peger på output-mappen (f.eks. bin/Debug/netX.X)
            string baseDirectory = AppContext.BaseDirectory;
            string imageBasePath = Path.Combine(baseDirectory, "SeedData", "Images"); // Stien til vores billedmappe
             Console.WriteLine($"INFO: Looking for seed images in: {imageBasePath}");


            // --- Seed Partier (uændret) ---
            if (!await context.FakePartier.AnyAsync())
            {
                var partier = new List<FakeParti> { /* ... */ };
                await context.FakePartier.AddRangeAsync(partier);
                await context.SaveChangesAsync();
                Console.WriteLine("INFO: Danske partier seeded.");
            }
            else
            {
                Console.WriteLine("INFO: Partier findes allerede i databasen.");
            }

            // --- Seed Politikere (opdateret med portræt) ---
            if (!await context.FakePolitikere.AnyAsync())
            {
                var seededPartier = await context.FakePartier.ToListAsync();
                if (!seededPartier.Any())
                {
                    Console.WriteLine("ERROR: Ingen partier fundet at tilknytte politikere til.");
                    return;
                }

                var politikere = new List<FakePolitiker>();
                for (int i = 0; i < numberOfPoliticians; i++)
                {
                    var randomParti = GetRandomElement(seededPartier);
                    var køn = random.Next(0, 2) == 0 ? "Mand" : "Kvinde";

                    // --- NYT: Hent portræt baseret på køn ---
                    byte[] portrætBytes = await GetPortraitBytesAsync(køn, imageBasePath);
                    // -------------

                    politikere.Add(new FakePolitiker
                    {
                        PolitikerNavn = GenerateRandomName(),
                        Alder = random.Next(25, 75),
                        Køn = køn,
                        Uddannelse = GetRandomElement(Educations),
                        Region = GetRandomElement(Regions),
                        Portræt = portrætBytes, // <-- Brug de indlæste bytes
                        PartiId = randomParti.PartiId // <-- Rettet: Brug Id fra FakeParti objektet
                        // FakeParti navigation property sættes automatisk af EF Core
                    });
                }

                await context.FakePolitikere.AddRangeAsync(politikere);
                await context.SaveChangesAsync();
                Console.WriteLine($"INFO: {numberOfPoliticians} tilfældige politikere seeded (med portrætter).");
            }
            else
            {
                Console.WriteLine("INFO: Politikere findes allerede i databasen.");
            }
        }
    }
}