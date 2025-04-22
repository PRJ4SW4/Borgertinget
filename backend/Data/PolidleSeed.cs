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

        // --- SeedDataAsync - Hovedmetoden til at seede data ---
        public static async Task SeedDataAsync(DataContext context, int numberOfPoliticians = 50)
        {
            // Find stien til billederne
            string baseDirectory = AppContext.BaseDirectory;
            // Antager billederne ligger i /SeedData/Images/ relativt til output mappen
            string imageBasePath = Path.Combine(baseDirectory, "SeedData", "Images");
            Console.WriteLine($"INFO: Leder efter seed-billeder i: {imageBasePath}");

            // --- Seed Partier ---
            if (!await context.FakePartier.AnyAsync()) // Tjek om der allerede findes partier
            {
                // Liste af danske partier (tilpas efter behov)
                var partier = new List<FakeParti>
                {
                    new FakeParti { PartiNavn = "Socialdemokratiet" },
                    new FakeParti { PartiNavn = "Venstre" },
                    new FakeParti { PartiNavn = "Moderaterne" },
                    new FakeParti { PartiNavn = "SF - Socialistisk Folkeparti" },
                    new FakeParti { PartiNavn = "Danmarksdemokraterne" },
                    new FakeParti { PartiNavn = "Liberal Alliance" },
                    new FakeParti { PartiNavn = "Det Konservative Folkeparti" },
                    new FakeParti { PartiNavn = "Enhedslisten" },
                    new FakeParti { PartiNavn = "Radikale Venstre" },
                    new FakeParti { PartiNavn = "Dansk Folkeparti" },
                    new FakeParti { PartiNavn = "Alternativet" },
                    new FakeParti { PartiNavn = "Nye Borgerlige" }
                };

                await context.FakePartier.AddRangeAsync(partier);
                await context.SaveChangesAsync(); // Gem partier FØR politikere!
                Console.WriteLine("INFO: Danske partier seeded.");
            }
            else
            {
                Console.WriteLine("INFO: Partier findes allerede i databasen.");
            }

            // --- Seed Politikere ---
            // RETTET: Brug det korrekte DbSet navn 'FakePolitikere'
            if (!await context.FakePolitikere.AnyAsync()) // Tjek om der allerede findes politikere
            {
                // Hent de nu gemte partier fra DB (nu med PartiId)
                var seededPartier = await context.FakePartier.ToListAsync();
                if (!seededPartier.Any()) // Sikkerhedstjek
                {
                    Console.WriteLine("FEJL: Ingen partier fundet i DB at tilknytte politikere til. Seeding af politikere afbrydes.");
                    return;
                }

                var politikere = new List<FakePolitiker>();
                // Brug nutid som reference til aldersberegning for seed data
                var today = DateTime.Today; // Brug DateTime.Today for DateOnly kompatibilitet

                for (int i = 0; i < numberOfPoliticians; i++)
                {
                    var randomParti = GetRandomElement(seededPartier);
                    if (randomParti == null) { continue; } // Skip hvis parti ikke kunne vælges

                    var køn = random.Next(0, 2) == 0 ? "Mand" : "Kvinde";
                    byte[] portrætBytes = await GetPortraitBytesAsync(køn, imageBasePath);

                    // --- START: Generer tilfældig fødselsdato ---
                    int randomAge = random.Next(25, 75); // Vælg en tilfældig alder mellem 25 og 74
                    int birthYear = today.Year - randomAge; // Beregn ca. fødselsår
                    // Generer en tilfældig dag i fødselsåret
                    int maxDayOfYear = DateTime.IsLeapYear(birthYear) ? 366 : 365;
                    int dayOfYear = random.Next(1, maxDayOfYear + 1); // +1 da øvre grænse er eksklusiv
                    // Opret DateOnly objekt
                    DateOnly birthDate = new DateOnly(birthYear, 1, 1).AddDays(dayOfYear - 1);
                    // --- SLUT: Generer tilfældig fødselsdato ---

                    politikere.Add(new FakePolitiker
                    {
                        PolitikerNavn = GenerateRandomName(),
                        // Alder = random.Next(25, 75), // <-- FJERN DENNE LINJE
                        DateOfBirth = birthDate.ToDateTime(TimeOnly.MinValue),         // <-- TILFØJET DENNE LINJE
                        Køn = køn,
                        Uddannelse = GetRandomElement(Educations),
                        Region = GetRandomElement(Regions),
                        Portræt = portrætBytes,
                        PartiId = randomParti.PartiId // Sæt FK til det valgte partis ID
                    });
                }

                // RETTET: Brug det korrekte DbSet navn 'FakePolitikere'
                await context.FakePolitikere.AddRangeAsync(politikere);
                await context.SaveChangesAsync(); // Gem politikerne
                Console.WriteLine($"INFO: {numberOfPoliticians} tilfældige politikere seeded (med portrætter, partiId og DateOfBirth).");
            }
            else
            {
                // RETTET: Brug det korrekte DbSet navn 'FakePolitikere'
                Console.WriteLine("INFO: Politikere findes allerede i databasen.");
            }
        }
    }
}