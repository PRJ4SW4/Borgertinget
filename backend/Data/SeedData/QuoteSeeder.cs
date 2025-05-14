// Fil: Data/SeedData/QuoteSeeder.cs
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.Data.SeedData
{
    public static class QuoteSeeder
    {
        private static int _nextQuoteId = 1; // Start ID for citaterne

        private static readonly List<string> GenericQuotes = new List<string>
        {
            "Fremtiden kræver modige beslutninger og fælles ansvar.",
            "Vi skal sikre et Danmark i balance, både socialt og økonomisk.",
            "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder.",
            "Investering i uddannelse og forskning er investering i vores fremtid.",
            "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed.",
            "Dialog og samarbejde på tværs af partiskel er vejen frem.",
            "Det lokale engagement er drivkraften i et levende demokrati.",
            "Vi skal turde tænke nyt for at løse fremtidens udfordringer.",
            "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand.",
            "Alle borgere fortjener respekt og en fair behandling af systemet.",
            "Transparens og åbenhed er afgørende for tilliden til det politiske system.",
            "Vi skal værne om de danske værdier og vores kulturelle arv.",
            "Internationalt samarbejde er essentielt i en globaliseret verden.",
            "En robust økonomi giver os råderum til at investere i velfærd.",
            "Børns trivsel og udvikling skal altid have førsteprioritet.",
            "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund.",
            "Digitalisering byder på store muligheder, men kræver også omtanke.",
            "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark.",
            "Retssikkerhed og lighed for loven er grundpiller i vores demokrati.",
            "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer.",
            "Innovation og iværksætteri er nøglen til fremtidig vækst.",
            "En effektiv offentlig sektor er en service for borgerne.",
            "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse.",
            "Vi har et ansvar for at efterlade en bedre verden til de næste generationer.",
            "Forebyggelse er ofte bedre og billigere end reparation."
        };

        private static PoliticianQuote CreateQuote(int aktorId, string text)
        {
            return new PoliticianQuote
            {
                QuoteId = _nextQuoteId++, // Vi tildeler ID manuelt
                AktorId = aktorId,
                QuoteText = text
            };
        }

        public static void SeedQuotes(ModelBuilder modelBuilder)
        {
            _nextQuoteId = 1; // Nulstil for hver kørsel
            var quotes = new List<PoliticianQuote>();

            List<int> aktorIdsToSeed = new List<int>
            {
                12,
                18,
                34,
                38,
                43,
                48,
                49,
                57,
                67,
                74,
                80,
                82,
                93,
                97,
                99,
                100,
                102,
                109,
                110,
                112,
                113,
                118,
                119,
                120,
                121,
                125,
                127,
                130,
                134,
                138,
                139,
                141,
                145,
                152,
                154,
                162,
                164,
                168,
                172,
                173,
                176,
                178,
                180,
                182,
                189,
                191,
                197,
                199,
                201,
                206,
                207,
                208,
                213,
                214,
                217,
                219,
                220,
                224,
                238,
                244,
                252,
                257,
                260,
                262,
                266,
                273,
                278,
                286,
                351,
                667,
                909,
                1146,
                1257,
                1305,
                1417,
                1454,
                1475,
                1613,
                1615,
                1845,
                3993,
                3997,
                4434,
                6380,
                9796,
                9816,
                9830,
                9939,
                9952,
                9963,
                9964,
                9976,
                10051,
                11702,
                14241,
                14252,
                14282,
                14283,
                14355,
                14461,
                14510,
                14740,
                15178,
                15734,
                15757,
                15760,
                15762,
                15763,
                15770,
                15773,
                15774,
                15775,
                15776,
                15777,
                15779,
                15787,
                15793,
                15800,
                16073,
                16180,
                16351,
                16503,
                16582,
                16728,
                17141,
                17360,
                17628,
                17629,
                18688,
                18693,
                18694,
                18695,
                18696,
                18699,
                18700,
                18701,
                18703,
                18706,
                18707,
                18708,
                18709,
                18712,
                18713,
                18715,
                18716,
                18717,
                18718,
                18719,
                18720,
                18721,
                18722,
                18723,
                18724,
                18725,
                18726,
                18729,
                18882,
                19000,
                19920,
                19928,
                20159,
                20349,
                20350,
                20351,
                20352,
                20353,
                20354,
                20355,
                20356,
                20357,
                20358,
                20359,
                20360,
                20361,
                20362,
                20363,
                20364,
                20365,
                20366,
                20367,
                20368,
                20369,
                20370,
                20371,
                20372,
                20373,
                20374,
                20375,
                20376,
                20377,
                20378,
                20379,
                20380,
                20381,
                20382,
                20383,
                20384,
                20385,
                20386,
                20388,
                20389,
                20390,
                20391,
                20392,
                20393,
                20394,
                20395,
                20396,
                20397,
                20398,
                20399,
                20400,
                20411,
                20415,
                20423,
                20496,
                20546,
                20559,
                20580,
                20781,
                20798,
                20820,
                20925,
                20962,
                20966,
                20976,
                21044,
                21076,
                21083,
                21143,
                21159,
                21161
            };

            if (!aktorIdsToSeed.Any())
            {
                Console.WriteLine("QuoteSeeder: Ingen Aktor ID'er specificeret i 'aktorIdsToSeed'. Skipper citat-seeding.");
                // Hvis filen er tom, og der ikke er quotes, så kald HasData med en tom liste for at undgå fejl,
                // eller bare return. For at undgå fejl ved tomme 'quotes' senere:
                if (!quotes.Any() && aktorIdsToSeed.Any()) { /* Dette sker kun hvis GenericQuotes er tom */ }
                else if (!quotes.Any())
                {
                    modelBuilder.Entity<PoliticianQuote>().HasData(new List<PoliticianQuote>()); // Undgå fejl med tom HasData
                    return;
                }
            }
            if (!GenericQuotes.Any())
            {
                 Console.WriteLine("QuoteSeeder: Ingen generiske citater defineret. Skipper citat-seeding.");
                 modelBuilder.Entity<PoliticianQuote>().HasData(new List<PoliticianQuote>()); // Undgå fejl med tom HasData
                 return;
            }

            int genericQuoteIndex = 0;
            foreach (var aktorId in aktorIdsToSeed)
            {
                // Sikrer at vi ikke går out of bounds på GenericQuotes, hvis der er færre citater end aktorId'er * 2
                if (GenericQuotes.Count == 0) break; // Stop hvis der ingen generiske citater er

                quotes.Add(CreateQuote(aktorId, GenericQuotes[genericQuoteIndex % GenericQuotes.Count]));
                genericQuoteIndex++;
                quotes.Add(CreateQuote(aktorId, GenericQuotes[genericQuoteIndex % GenericQuotes.Count])); // <<< RETTET HER
                genericQuoteIndex++;
            }

            if (quotes.Any())
            {
                modelBuilder.Entity<PoliticianQuote>().HasData(quotes);
                Console.WriteLine($"QuoteSeeder: Successfully prepared {quotes.Count} quotes for {aktorIdsToSeed.Count} Aktors for seeding.");
            }
            else
            {
                Console.WriteLine("QuoteSeeder: No quotes were prepared for seeding (possibly due to empty Aktor ID list or empty generic quotes list).");
                // Kald HasData med en tom liste for at EF Core er tilfreds, hvis det er nødvendigt
                // afhængigt af hvordan din OnModelCreating ellers er bygget op.
                // Typisk er det ikke nødvendigt hvis ingen data skal seedes for en tabel.
                // modelBuilder.Entity<PoliticianQuote>().HasData(new List<PoliticianQuote>());
            }
        }
    }
}
