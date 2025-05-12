using backend.Data; // Namespace for DataContext
using backend.Models; // Namespace for FakePolitiker, FakeParti etc.
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization; // Nødvendig for CultureInfo
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Data
{
    public class PolidleSeed
    {
        private static readonly string DefaultPortrait = "default_avatar.png"; // Default billede hvis specifik fil mangler

        // Funktion til at hente et specifikt portræt baseret på filnavn
        private static async Task<byte[]> GetSpecificPortraitBytesAsync(string portraitFileName, string imageBasePath)
        {
            string selectedFileName = string.IsNullOrWhiteSpace(portraitFileName) ? DefaultPortrait : portraitFileName;
            string fullImagePath = Path.Combine(imageBasePath, selectedFileName);

            if (!File.Exists(fullImagePath) && selectedFileName != DefaultPortrait)
            {
                Console.WriteLine($"ADVARSEL: Portrætfil ikke fundet: {fullImagePath}. Forsøger default.");
                fullImagePath = Path.Combine(imageBasePath, DefaultPortrait);
            }

            if (File.Exists(fullImagePath))
            {
                try
                {
                    return await File.ReadAllBytesAsync(fullImagePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FEJL: Kunne ikke læse billedfil {fullImagePath}: {ex.Message}");
                    return Array.Empty<byte>();
                }
            }
            else
            {
                Console.WriteLine($"ADVARSEL: Hverken specificeret ('{selectedFileName}') eller default portrætfil blev fundet. Sti forsøgt for default: {fullImagePath}");
                return Array.Empty<byte>();
            }
        }

        public static async Task SeedDataAsync(DataContext context)
        {
            string baseDirectory = AppContext.BaseDirectory;
            // Juster stien hvis nødvendigt, f.eks. gå et niveau op hvis SeedData er ved siden af bin
            string projectRoot = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", ".."));
            string imageBasePath = Path.Combine(projectRoot, "SeedData", "Images");
            // Hvis ovenstående ikke virker, prøv den simple:
            // string imageBasePath = Path.Combine(baseDirectory, "SeedData", "Images");
            Console.WriteLine($"INFO: Leder efter seed-billeder i: {imageBasePath}");
             if (!Directory.Exists(imageBasePath))
            {
                 Console.WriteLine($"ADVARSEL: Mappen for seed-billeder blev ikke fundet: {imageBasePath}");
            }


            // --- Seed Partier ---
            bool partiesWereSeeded = false;
            if (!await context.FakePartier.AnyAsync())
            {
                Console.WriteLine("INFO: Ingen partier fundet. Seeder partier...");
                var partier = new List<FakeParti>
                {
                    // Sørg for at PartiNavn matcher præcist med det brugt i PoliticianDefinition nedenfor
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
                    new FakeParti { PartiNavn = "Nye Borgerlige" } // Selvom de er ude, kan de være relevante for historik
                };
                await context.FakePartier.AddRangeAsync(partier);
                await context.SaveChangesAsync();
                partiesWereSeeded = true;
                Console.WriteLine("INFO: Danske partier seeded.");
            }
            else
            {
                Console.WriteLine("INFO: Partier findes allerede i databasen.");
            }

            // --- Seed Politikere (Manuelt Defineret) ---
            bool politiciansWereSeeded = false;
            if (!await context.FakePolitikere.AnyAsync())
            {
                Console.WriteLine("INFO: Ingen politikere fundet. Seeder manuelt definerede politikere...");

                var seededPartier = await context.FakePartier.ToListAsync();
                var partiNameToIdMap = seededPartier.ToDictionary(p => p.PartiNavn, p => p.PartiId, StringComparer.OrdinalIgnoreCase);

                if (!partiNameToIdMap.Any())
                {
                    Console.WriteLine("FEJL: Kunne ikke finde nogen partier i DB. Politker-seeding afbrydes.");
                    return;
                }

                // *** TOP 20 DANSKE POLITIKERE - FOLKETINGET***
                var manualPoliticiansData = new List<PoliticianDefinition>
                {
                    // Partiledere
                    new PoliticianDefinition { PolitikerNavn = "Mette Frederiksen", DateOfBirthString = "1977-11-19", Køn = "Kvinde", Uddannelse = "Master i Afrikastudier", Region = "Nordjyllands Storkreds", PartiNavn = "Socialdemokratiet", PortraitFileName = "mette_frederiksen.png", Quotes = new List<string> { "Vi skal passe på Danmark.", "Sammenhold er vores styrke." }},
                    new PoliticianDefinition { PolitikerNavn = "Troels Lund Poulsen", DateOfBirthString = "1976-03-30", Køn = "Mand", Uddannelse = "Student)", Region = "Østjyllands Storkreds", PartiNavn = "Venstre", PortraitFileName = "troels_lund_poulsen.png", Quotes = new List<string> { "Danmark skal være et land, hvor det betaler sig at arbejde.", "Vi tror på frihed under ansvar." }},
                    new PoliticianDefinition { PolitikerNavn = "Lars Løkke Rasmussen", DateOfBirthString = "1964-05-15", Køn = "Mand", Uddannelse = "Cand.jur.", Region = "Sjællands Storkreds", PartiNavn = "Moderaterne", PortraitFileName = "lars_lokke_rasmussen.png", Quotes = new List<string> { "Vi skal bygge bro over midten.", "Der er brug for forandring." }},
                    new PoliticianDefinition { PolitikerNavn = "Pia Olsen Dyhr", DateOfBirthString = "1971-11-30", Køn = "Kvinde", Uddannelse = "Cand.scient.pol.", Region = "Københavns Storkreds", PartiNavn = "SF - Socialistisk Folkeparti", PortraitFileName = "pia_olsen_dyhr.png", Quotes = new List<string> { "Klima og fællesskab hænger sammen.", "Vi kæmper for en grønnere og mere retfærdig fremtid." }},
                    new PoliticianDefinition { PolitikerNavn = "Inger Støjberg", DateOfBirthString = "1973-03-16", Køn = "Kvinde", Uddannelse = "InformationsAkademiet", Region = "Nordjyllands Storkreds", PartiNavn = "Danmarksdemokraterne", PortraitFileName = "inger_stojberg.png", Quotes = new List<string> { "Der skal stilles krav.", "Vi skal værne om danske værdier." }},
                    new PoliticianDefinition { PolitikerNavn = "Alex Vanopslagh", DateOfBirthString = "1991-10-17", Køn = "Mand", Uddannelse = "Cand.scient.pol.", Region = "Østjyllands Storkreds", PartiNavn = "Liberal Alliance", PortraitFileName = "alex_vanopslagh.png", Quotes = new List<string> { "Mindre stat, mere frihed.", "Det skal bedre kunne betale sig at arbejde." }},
                    new PoliticianDefinition { PolitikerNavn = "Mona Juul", DateOfBirthString = "1967-03-20", Køn = "Kvinde", Uddannelse = "Cand.merc.", Region = "Østjyllands Storkreds", PartiNavn = "Det Konservative Folkeparti", PortraitFileName = "mona_juul.png", Quotes = new List<string> { "Vi skal passe på dansk erhvervsliv.", "Generationernes kontrakt skal holdes." }},
                    new PoliticianDefinition { PolitikerNavn = "Pelle Dragsted", DateOfBirthString = "1975-04-13", Køn = "Mand", Uddannelse = "BA i historie", Region = "Københavns Storkreds", PartiNavn = "Enhedslisten", PortraitFileName = "pelle_dragsted.png", Quotes = new List<string> { "Omfordeling og klimahandling nu.", "Fællesskabet skal styrkes." }},
                    new PoliticianDefinition { PolitikerNavn = "Martin Lidegaard", DateOfBirthString = "1966-12-12", Køn = "Mand", Uddannelse = "Cand.comm.", Region = "Nordsjællands Storkreds", PartiNavn = "Radikale Venstre", PortraitFileName = "martin_lidegaard.png", Quotes = new List<string> { "Vi har brug for europæisk samarbejde.", "Uddannelse og grøn omstilling er vejen frem." }},
                    new PoliticianDefinition { PolitikerNavn = "Morten Messerschmidt", DateOfBirthString = "1980-11-13", Køn = "Mand", Uddannelse = "Cand.jur.", Region = "Sjællands Storkreds", PartiNavn = "Dansk Folkeparti", PortraitFileName = "morten_messerschmidt.png", Quotes = new List<string> { "Danmark skal være dansk.", "Mere Danmark, mindre EU." }},
                    new PoliticianDefinition { PolitikerNavn = "Franciska Rosenkilde", DateOfBirthString = "1976-03-19", Køn = "Kvinde", Uddannelse = "Cand.mag. i geografi & sundhedsfremme", Region = "Københavns Storkreds", PartiNavn = "Alternativet", PortraitFileName = "franciska_rosenkilde.png", Quotes = new List<string> { "En ny politisk kultur er nødvendig.", "Mod og grønt håb." }},

                    // Nøgleministre og andre profiler
                    new PoliticianDefinition { PolitikerNavn = "Nicolai Wammen", DateOfBirthString = "1971-02-07", Køn = "Mand", Uddannelse = "Cand.scient.pol.", Region = "Østjyllands Storkreds", PartiNavn = "Socialdemokratiet", PortraitFileName = "nicolai_wammen.png", Quotes = new List<string> { "Vi skal have styr på økonomien.", "Ansvarlighed og tryghed." }},
                    new PoliticianDefinition { PolitikerNavn = "Stephanie Lose", DateOfBirthString = "1982-12-15", Køn = "Kvinde", Uddannelse = "Cand.oecon.", Region = "Syddanmarks Storkreds", PartiNavn = "Venstre", PortraitFileName = "stephanie_lose.png", Quotes = new List<string> { "Der skal være balance mellem land og by.", "Sundhedsvæsenet skal styrkes." }},
                    new PoliticianDefinition { PolitikerNavn = "Magnus Heunicke", DateOfBirthString = "1975-01-28", Køn = "Mand", Uddannelse = "Journalist", Region = "Sjællands Storkreds", PartiNavn = "Socialdemokratiet", PortraitFileName = "magnus_heunicke.png", Quotes = new List<string> { "Vores fælles velfærd er vigtig.", "Vi skal passe på miljøet." }},
                    new PoliticianDefinition { PolitikerNavn = "Kaare Dybvad Bek", DateOfBirthString = "1984-08-05", Køn = "Mand", Uddannelse = "Cand.scient. i geografi & geoinformatik", Region = "Sjællands Storkreds", PartiNavn = "Socialdemokratiet", PortraitFileName = "kaare_dybvad_bek.png", Quotes = new List<string> { "Danmark skal ikke knække over.", "Integration kræver en indsats." }},
                    new PoliticianDefinition { PolitikerNavn = "Morten Dahlin", DateOfBirthString = "1989-05-01", Køn = "Mand", Uddannelse = "Cand.merc.jur.", Region = "Sjællands Storkreds", PartiNavn = "Venstre", PortraitFileName = "morten_dahlin.png", Quotes = new List<string> { "Bedre vilkår for iværksættere.", "Færre regler, mere sund fornuft." }},
                    new PoliticianDefinition { PolitikerNavn = "Peter Skaarup", DateOfBirthString = "1964-05-01", Køn = "Mand", Uddannelse = "Teknisk Assistent", Region = "Københavns Storkreds", PartiNavn = "Danmarksdemokraterne", PortraitFileName = "peter_skaarup.png", Quotes = new List<string> { "Retspolitikken skal strammes.", "Grænsekontrol er nødvendigt." }}, // Bemærk parti skift fra DF
                    new PoliticianDefinition { PolitikerNavn = "Ole Birk Olesen", DateOfBirthString = "1972-12-21", Køn = "Mand", Uddannelse = "Journalist", Region = "Østjyllands Storkreds", PartiNavn = "Liberal Alliance", PortraitFileName = "ole_birk_olesen.png", Quotes = new List<string> { "Skatterne skal ned.", "Staten skal fylde mindre." }},
                    new PoliticianDefinition { PolitikerNavn = "Mai Mercado", DateOfBirthString = "1980-08-20", Køn = "Kvinde", Uddannelse = "Cand.scient.pol.", Region = "Fyns Storkreds", PartiNavn = "Det Konservative Folkeparti", PortraitFileName = "mai_mercado.png", Quotes = new List<string> { "Familierne skal have bedre vilkår.", "Børn og unge skal prioriteres." }},
                    new PoliticianDefinition { PolitikerNavn = "Jeppe Bruus", DateOfBirthString = "1978-04-20", Køn = "Mand", Uddannelse = "Cand.scient.pol.", Region = "Københavns Omegns Storkreds", PartiNavn = "Socialdemokratiet", PortraitFileName = "jeppe_bruus.png", Quotes = new List<string> { "Skattesystemet skal være retfærdigt.", "Vi skal bekæmpe skatteunddragelse." }}
                };
                // ***************************************************************************************

                var politikereToAdd = new List<FakePolitiker>();

                foreach (var definition in manualPoliticiansData)
                {
                    if (!partiNameToIdMap.TryGetValue(definition.PartiNavn, out int partiId))
                    {
                        Console.WriteLine($"ADVARSEL: Parti '{definition.PartiNavn}' ikke fundet for politiker '{definition.PolitikerNavn}'. Springes over.");
                        continue;
                    }

                    byte[] portrætBytes = await GetSpecificPortraitBytesAsync(definition.PortraitFileName, imageBasePath);

                    // Brug invariant culture for at sikre korrekt parsing af dato uanset systemets lokale indstillinger
                    if (!DateOnly.TryParseExact(definition.DateOfBirthString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly birthDate))
                    {
                        Console.WriteLine($"ADVARSEL: Kunne ikke parse fødselsdato '{definition.DateOfBirthString}' for politiker '{definition.PolitikerNavn}'. Springes over.");
                        continue;
                    }
                    DateTime birthDateTimeUtc = birthDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

                    var politiker = new FakePolitiker
                    {
                        PolitikerNavn = definition.PolitikerNavn,
                        DateOfBirth = birthDateTimeUtc, // Gemmer som DateTime UTC
                        Køn = definition.Køn,
                        Uddannelse = definition.Uddannelse,
                        Region = definition.Region,
                        Portræt = portrætBytes,
                        PartiId = partiId // Sætter Foreign Key
                        // FakeParti navigation property sættes IKKE her, EF Core klarer det via PartiId
                    };

                    // Tilføj citater (relationen håndteres af EF Core når der gemmes)
                    foreach (var quoteText in definition.Quotes)
                    {
                        politiker.Quotes.Add(new PoliticianQuote { QuoteText = quoteText });
                    }

                    politikereToAdd.Add(politiker);
                }

                if (politikereToAdd.Any())
                {
                    await context.FakePolitikere.AddRangeAsync(politikereToAdd);
                    await context.SaveChangesAsync(); // Gemmer politikere OG deres relaterede citater
                    politiciansWereSeeded = true;
                    Console.WriteLine($"INFO: {politikereToAdd.Count} manuelt definerede politikere seeded.");
                }
                else
                {
                    Console.WriteLine("INFO: Ingen gyldige manuelt definerede politikere blev fundet eller oprettet.");
                }
            }
            else
            {
                Console.WriteLine("INFO: Politikere findes allerede i databasen.");
            }

            Console.WriteLine($"INFO: Seeding status - Parties seeded: {partiesWereSeeded}, Politicians seeded: {politiciansWereSeeded}");
        }

        // Intern hjælpeklasse til at holde data for manuelle politikere
        private class PoliticianDefinition
        {
            public string PolitikerNavn { get; set; } = string.Empty;
            public string DateOfBirthString { get; set; } = string.Empty; // Format: yyyy-MM-dd
            public string Køn { get; set; } = string.Empty;
            public string Uddannelse { get; set; } = string.Empty;
            public string Region { get; set; } = string.Empty;
            public string PartiNavn { get; set; } = string.Empty;
            public string PortraitFileName { get; set; } = string.Empty;
            public List<string> Quotes { get; set; } = new List<string>();
        }
    }
}