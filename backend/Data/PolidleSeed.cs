using backend.Data; // Namespace for DataContext
using backend.Models; // Namespace for FakePolitiker, FakeParti etc.
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Data
{
    public class PolidleSeed
    {
        // Lister til navne, uddannelser, regioner
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

        // Portræt-filnavne
        private static readonly List<string> MalePortraits = new List<string> { "mand1.png", "mand2.png", "mand3.png", "mand4.png" };
        private static readonly List<string> FemalePortraits = new List<string> { "kvinde1.png", "kvinde2.png", "kvinde3.png" };
        private static readonly string DefaultPortrait = "default_avatar.png";

        // === NYT: Liste med eksempel-citater ===
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
            // Tilføj gerne flere eller mere specifikke (men stadig generiske) citater
        };
        // ========================================

        private static readonly Random random = new Random();

        // Hjælpefunktion til at vælge et tilfældigt element fra en liste
        private static T GetRandomElement<T>(List<T> list)
        {
            // Returner default (null for reference types) hvis listen er tom eller null
             if (list == null || list.Count == 0) { return default(T); }
            return list[random.Next(list.Count)];
        }

        // Hjælpefunktion til at generere et tilfældigt navn
        private static string GenerateRandomName()
        {
            return $"{GetRandomElement(FirstNames)} {GetRandomElement(LastNames)}";
        }

        // Hjælpefunktion til at hente portræt-billeddata som byte array
        private static async Task<byte[]> GetPortraitBytesAsync(string gender, string imageBasePath)
        {
            string selectedFileName = DefaultPortrait;
            if (gender == "Mand" && MalePortraits.Any()) { selectedFileName = GetRandomElement(MalePortraits); }
            else if (gender == "Kvinde" && FemalePortraits.Any()) { selectedFileName = GetRandomElement(FemalePortraits); }

            string fullImagePath = Path.Combine(imageBasePath, selectedFileName);

            // Tjek om den valgte fil findes, ellers prøv default
            if (!File.Exists(fullImagePath) && selectedFileName != DefaultPortrait)
            {
                Console.WriteLine($"ADVARSEL: Portrætfil ikke fundet: {fullImagePath}. Prøver default.");
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
                    Console.WriteLine($"FEJL: Kunne ikke læse billedfil {fullImagePath}: {ex.Message}");
                    return Array.Empty<byte>(); // Returner tomt array ved fejl
                }
            }
            else
            {
                 // Skriv kun advarsel hvis selv default-filen mangler
                if(selectedFileName == DefaultPortrait) Console.WriteLine($"ADVARSEL: Default portrætfil ikke fundet: {fullImagePath}");
                return Array.Empty<byte>(); // Returner tomt array hvis ingen fil findes
            }
        }

        // --- SeedDataAsync - Opdateret til at inkludere Citater ---
        public static async Task SeedDataAsync(DataContext context, int numberOfPoliticians = 50)
        {
            string baseDirectory = AppContext.BaseDirectory;
            string imageBasePath = Path.Combine(baseDirectory, "SeedData", "Images");
            Console.WriteLine($"INFO: Leder efter seed-billeder i: {imageBasePath}");

            // --- Seed Partier (uændret logik) ---
            if (!await context.FakePartier.AnyAsync())
            {
                var partier = new List<FakeParti> { /* ... dine partier ... */ };
                await context.FakePartier.AddRangeAsync(partier);
                await context.SaveChangesAsync();
                Console.WriteLine("INFO: Danske partier seeded.");
            }
            else { Console.WriteLine("INFO: Partier findes allerede i databasen."); }


            // --- Seed Politikere (OPDATERET til at tilføje citater) ---
            if (!await context.FakePolitikere.AnyAsync())
            {
                var seededPartier = await context.FakePartier.ToListAsync();
                if (!seededPartier.Any()) { Console.WriteLine("FEJL: Ingen partier fundet..."); return; }

                var politikere = new List<FakePolitiker>();
                var today = DateTime.Today;

                for (int i = 0; i < numberOfPoliticians; i++)
                {
                    var randomParti = GetRandomElement(seededPartier);
                    if (randomParti == null) { continue; }

                    var køn = random.Next(0, 2) == 0 ? "Mand" : "Kvinde";
                    byte[] portrætBytes = await GetPortraitBytesAsync(køn, imageBasePath);

                    // Generer fødselsdato (uændret)
                    int randomAge = random.Next(25, 75);
                    int birthYear = today.Year - randomAge;
                    int maxDayOfYear = DateTime.IsLeapYear(birthYear) ? 366 : 365;
                    int dayOfYear = random.Next(1, maxDayOfYear + 1);
                    DateOnly birthDate = new DateOnly(birthYear, 1, 1).AddDays(dayOfYear - 1);

                    // Opret politiker-objektet FØRST
                    var politiker = new FakePolitiker
                    {
                        PolitikerNavn = GenerateRandomName(),
                        DateOfBirth = birthDate.ToDateTime(TimeOnly.MinValue),
                        Køn = køn,
                        Uddannelse = GetRandomElement(Educations),
                        Region = GetRandomElement(Regions),
                        Portræt = portrætBytes,
                        PartiId = randomParti.PartiId
                        // Quotes collection er initialiseret som tom List i modellen
                    };

                    // === NYT: Tilføj et par tilfældige citater til politikeren ===
                    int numberOfQuotes = random.Next(1, 4); // Tilføj 1-3 citater per politiker
                    for(int q = 0; q < numberOfQuotes; q++)
                    {
                        // Opret et nyt citat objekt
                        var newQuote = new PoliticianQuote
                        {
                            QuoteText = GetRandomElement(SampleQuotes)
                            // Sæt IKKE PolitikerId her, EF Core klarer det via navigation property
                            // Sæt IKKE FakePolitiker her, det klares ved at add'e til collection
                        };
                        // Tilføj citatet til politikerens Quotes collection
                        politiker.Quotes.Add(newQuote);
                    }
                    // =========================================================

                    // Tilføj den færdige politiker (med citater) til listen
                    politikere.Add(politiker);
                }

                await context.FakePolitikere.AddRangeAsync(politikere);
                // Når SaveChangesAsync kaldes her, vil EF Core:
                // 1. Indsætte politikerne og få deres auto-genererede IDs.
                // 2. Indsætte citaterne og automatisk sætte PolitikerId FK baseret
                //    på hvilken politikers Quotes collection de blev tilføjet til.
                await context.SaveChangesAsync();
                // OPDATERET log besked
                Console.WriteLine($"INFO: {numberOfPoliticians} tilfældige politikere seeded (med portrætter, partiId, DateOfBirth og citater).");
            }
            else
            {
                Console.WriteLine("INFO: Politikere findes allerede i databasen.");
            }
        }
    }
}