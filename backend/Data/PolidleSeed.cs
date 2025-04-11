using backend.Data;
using backend.Models;
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

        // Portræt-filnavne (uændret)
        private static readonly List<string> MalePortraits = new List<string> { "mand1.png", "mand2.png", "mand3.png", "mand4.png" };
        private static readonly List<string> FemalePortraits = new List<string> { "kvinde1.png", "kvinde2.png", "kvinde3.png" };
        private static readonly string DefaultPortrait = "default_avatar.png";

        private static readonly Random random = new Random();

        // GetRandomElement (uændret)
        private static T GetRandomElement<T>(List<T> list)
        {
             if (list == null || list.Count == 0) { return default(T); }
            return list[random.Next(list.Count)];
        }

        // GenerateRandomName (uændret)
        private static string GenerateRandomName()
        {
            return $"{GetRandomElement(FirstNames)} {GetRandomElement(LastNames)}";
        }

        // GetPortraitBytesAsync (uændret)
        private static async Task<byte[]> GetPortraitBytesAsync(string gender, string imageBasePath)
        {
            string selectedFileName = DefaultPortrait;
            if (gender == "Mand" && MalePortraits.Any()) { selectedFileName = GetRandomElement(MalePortraits); }
            else if (gender == "Kvinde" && FemalePortraits.Any()) { selectedFileName = GetRandomElement(FemalePortraits); }
            string fullImagePath = Path.Combine(imageBasePath, selectedFileName);
            if (!File.Exists(fullImagePath) && selectedFileName != DefaultPortrait) {
                 Console.WriteLine($"WARNING: Portrait file not found: {fullImagePath}. Trying default.");
                 fullImagePath = Path.Combine(imageBasePath, DefaultPortrait);
            }
            if (File.Exists(fullImagePath)) {
                try { return await File.ReadAllBytesAsync(fullImagePath); }
                catch (Exception ex) { Console.WriteLine($"ERROR: Could not read image file {fullImagePath}: {ex.Message}"); return Array.Empty<byte>(); }
            } else {
                 if(selectedFileName == DefaultPortrait) Console.WriteLine($"WARNING: Default portrait file not found: {fullImagePath}");
                return Array.Empty<byte>();
            }
        }

        // --- SeedDataAsync - Opdateret Parti Seeding ---
        public static async Task SeedDataAsync(DataContext context, int numberOfPoliticians = 50)
        {
            string baseDirectory = AppContext.BaseDirectory;
            string imageBasePath = Path.Combine(baseDirectory, "SeedData", "Images");
            Console.WriteLine($"INFO: Looking for seed images in: {imageBasePath}");

            // --- Seed Partier ---
            if (!await context.FakePartier.AnyAsync())
            {
                // *** HER DEFINERES PARTIERNE ***
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
                    // Tilføj evt. flere eller juster navne
                };
                // *******************************

                await context.FakePartier.AddRangeAsync(partier);
                await context.SaveChangesAsync(); // Gem partier FØR politikere!
                Console.WriteLine("INFO: Danske partier seeded.");
            }
            else
            {
                Console.WriteLine("INFO: Partier findes allerede i databasen.");
            }

            // --- Seed Politikere ---
            if (!await context.FakePolitikere.AnyAsync())
            {
                // Hent partierne fra DB (nu med IDs)
                var seededPartier = await context.FakePartier.ToListAsync();
                if (!seededPartier.Any())
                {
                    Console.WriteLine("ERROR: Ingen partier fundet i DB at tilknytte politikere til. Seeding af politikere afbrydes.");
                    return; // Stop hvis ingen partier kunne hentes/findes
                }

                var politikere = new List<FakePolitiker>();
                for (int i = 0; i < numberOfPoliticians; i++)
                {
                    var randomParti = GetRandomElement(seededPartier); // Vælg et tilfældigt parti objekt
                    if (randomParti == null) { // Sikkerhedstjek hvis GetRandomElement returnerer null
                        Console.WriteLine($"WARNING: Kunne ikke vælge et tilfældigt parti for politiker {i}. Skipper.");
                        continue;
                    }

                    var køn = random.Next(0, 2) == 0 ? "Mand" : "Kvinde";
                    byte[] portrætBytes = await GetPortraitBytesAsync(køn, imageBasePath);

                    politikere.Add(new FakePolitiker
                    {
                        PolitikerNavn = GenerateRandomName(),
                        Alder = random.Next(25, 75),
                        Køn = køn,
                        Uddannelse = GetRandomElement(Educations),
                        Region = GetRandomElement(Regions),
                        Portræt = portrætBytes,
                        // *** VIGTIGT: Brug .Id fra det valgte FakeParti objekt ***
                        PartiId = randomParti.PartiId
                        // ********************************************************
                    });
                }

                await context.FakePolitikere.AddRangeAsync(politikere);
                await context.SaveChangesAsync(); // Gem politikerne
                Console.WriteLine($"INFO: {numberOfPoliticians} tilfældige politikere seeded (med portrætter og partiId).");
            }
            else
            {
                Console.WriteLine("INFO: Politikere findes allerede i databasen.");
            }
        }
    }
}