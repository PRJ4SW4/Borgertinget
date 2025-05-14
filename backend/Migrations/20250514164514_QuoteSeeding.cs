using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class QuoteSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PoliticianQuotes",
                columns: new[] { "QuoteId", "AktorId", "QuoteText" },
                values: new object[,]
                {
                    { 1, 12, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 2, 12, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 3, 18, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 4, 18, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 5, 34, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 6, 34, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 7, 38, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 8, 38, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 9, 43, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 10, 43, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 11, 48, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 12, 48, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 13, 49, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 14, 49, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 15, 57, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 16, 57, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 17, 67, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 18, 67, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 19, 74, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 20, 74, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 21, 80, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 22, 80, "En effektiv offentlig sektor er en service for borgerne." },
                    { 23, 82, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 24, 82, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 25, 93, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 26, 93, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 27, 97, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 28, 97, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 29, 99, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 30, 99, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 31, 100, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 32, 100, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 33, 102, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 34, 102, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 35, 109, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 36, 109, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 37, 110, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 38, 110, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 39, 112, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 40, 112, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 41, 113, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 42, 113, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 43, 118, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 44, 118, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 45, 119, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 46, 119, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 47, 120, "En effektiv offentlig sektor er en service for borgerne." },
                    { 48, 120, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 49, 121, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 50, 121, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 51, 125, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 52, 125, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 53, 127, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 54, 127, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 55, 130, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 56, 130, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 57, 134, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 58, 134, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 59, 138, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 60, 138, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 61, 139, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 62, 139, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 63, 141, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 64, 141, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 65, 145, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 66, 145, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 67, 152, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 68, 152, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 69, 154, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 70, 154, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 71, 162, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 72, 162, "En effektiv offentlig sektor er en service for borgerne." },
                    { 73, 164, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 74, 164, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 75, 168, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 76, 168, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 77, 172, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 78, 172, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 79, 173, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 80, 173, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 81, 176, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 82, 176, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 83, 178, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 84, 178, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 85, 180, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 86, 180, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 87, 182, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 88, 182, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 89, 189, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 90, 189, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 91, 191, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 92, 191, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 93, 197, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 94, 197, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 95, 199, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 96, 199, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 97, 201, "En effektiv offentlig sektor er en service for borgerne." },
                    { 98, 201, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 99, 206, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 100, 206, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 101, 207, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 102, 207, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 103, 208, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 104, 208, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 105, 213, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 106, 213, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 107, 214, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 108, 214, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 109, 217, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 110, 217, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 111, 219, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 112, 219, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 113, 220, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 114, 220, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 115, 224, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 116, 224, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 117, 238, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 118, 238, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 119, 244, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 120, 244, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 121, 252, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 122, 252, "En effektiv offentlig sektor er en service for borgerne." },
                    { 123, 257, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 124, 257, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 125, 260, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 126, 260, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 127, 262, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 128, 262, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 129, 266, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 130, 266, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 131, 273, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 132, 273, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 133, 278, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 134, 278, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 135, 286, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 136, 286, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 137, 351, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 138, 351, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 139, 667, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 140, 667, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 141, 909, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 142, 909, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 143, 1146, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 144, 1146, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 145, 1257, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 146, 1257, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 147, 1305, "En effektiv offentlig sektor er en service for borgerne." },
                    { 148, 1305, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 149, 1417, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 150, 1417, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 151, 1454, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 152, 1454, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 153, 1475, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 154, 1475, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 155, 1613, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 156, 1613, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 157, 1615, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 158, 1615, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 159, 1845, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 160, 1845, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 161, 3993, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 162, 3993, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 163, 3997, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 164, 3997, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 165, 4434, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 166, 4434, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 167, 6380, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 168, 6380, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 169, 9796, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 170, 9796, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 171, 9816, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 172, 9816, "En effektiv offentlig sektor er en service for borgerne." },
                    { 173, 9830, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 174, 9830, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 175, 9939, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 176, 9939, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 177, 9952, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 178, 9952, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 179, 9963, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 180, 9963, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 181, 9964, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 182, 9964, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 183, 9976, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 184, 9976, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 185, 10051, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 186, 10051, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 187, 11702, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 188, 11702, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 189, 14241, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 190, 14241, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 191, 14252, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 192, 14252, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 193, 14282, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 194, 14282, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 195, 14283, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 196, 14283, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 197, 14355, "En effektiv offentlig sektor er en service for borgerne." },
                    { 198, 14355, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 199, 14461, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 200, 14461, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 201, 14510, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 202, 14510, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 203, 14740, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 204, 14740, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 205, 15178, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 206, 15178, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 207, 15734, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 208, 15734, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 209, 15757, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 210, 15757, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 211, 15760, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 212, 15760, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 213, 15762, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 214, 15762, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 215, 15763, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 216, 15763, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 217, 15770, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 218, 15770, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 219, 15773, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 220, 15773, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 221, 15774, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 222, 15774, "En effektiv offentlig sektor er en service for borgerne." },
                    { 223, 15775, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 224, 15775, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 225, 15776, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 226, 15776, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 227, 15777, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 228, 15777, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 229, 15779, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 230, 15779, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 231, 15787, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 232, 15787, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 233, 15793, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 234, 15793, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 235, 15800, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 236, 15800, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 237, 16073, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 238, 16073, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 239, 16180, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 240, 16180, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 241, 16351, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 242, 16351, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 243, 16503, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 244, 16503, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 245, 16582, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 246, 16582, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 247, 16728, "En effektiv offentlig sektor er en service for borgerne." },
                    { 248, 16728, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 249, 17141, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 250, 17141, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 251, 17360, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 252, 17360, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 253, 17628, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 254, 17628, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 255, 17629, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 256, 17629, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 257, 18688, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 258, 18688, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 259, 18693, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 260, 18693, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 261, 18694, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 262, 18694, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 263, 18695, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 264, 18695, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 265, 18696, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 266, 18696, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 267, 18699, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 268, 18699, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 269, 18700, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 270, 18700, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 271, 18701, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 272, 18701, "En effektiv offentlig sektor er en service for borgerne." },
                    { 273, 18703, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 274, 18703, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 275, 18706, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 276, 18706, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 277, 18707, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 278, 18707, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 279, 18708, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 280, 18708, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 281, 18709, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 282, 18709, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 283, 18712, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 284, 18712, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 285, 18713, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 286, 18713, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 287, 18715, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 288, 18715, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 289, 18716, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 290, 18716, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 291, 18717, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 292, 18717, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 293, 18718, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 294, 18718, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 295, 18719, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 296, 18719, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 297, 18720, "En effektiv offentlig sektor er en service for borgerne." },
                    { 298, 18720, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 299, 18721, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 300, 18721, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 301, 18722, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 302, 18722, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 303, 18723, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 304, 18723, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 305, 18724, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 306, 18724, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 307, 18725, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 308, 18725, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 309, 18726, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 310, 18726, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 311, 18729, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 312, 18729, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 313, 18882, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 314, 18882, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 315, 19000, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 316, 19000, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 317, 19920, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 318, 19920, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 319, 19928, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 320, 19928, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 321, 20159, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 322, 20159, "En effektiv offentlig sektor er en service for borgerne." },
                    { 323, 20349, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 324, 20349, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 325, 20350, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 326, 20350, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 327, 20351, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 328, 20351, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 329, 20352, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 330, 20352, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 331, 20353, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 332, 20353, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 333, 20354, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 334, 20354, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 335, 20355, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 336, 20355, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 337, 20356, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 338, 20356, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 339, 20357, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 340, 20357, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 341, 20358, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 342, 20358, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 343, 20359, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 344, 20359, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 345, 20360, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 346, 20360, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 347, 20361, "En effektiv offentlig sektor er en service for borgerne." },
                    { 348, 20361, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 349, 20362, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 350, 20362, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 351, 20363, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 352, 20363, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 353, 20364, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 354, 20364, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 355, 20365, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 356, 20365, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 357, 20366, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 358, 20366, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 359, 20367, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 360, 20367, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 361, 20368, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 362, 20368, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 363, 20369, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 364, 20369, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 365, 20370, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 366, 20370, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 367, 20371, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 368, 20371, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 369, 20372, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 370, 20372, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 371, 20373, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 372, 20373, "En effektiv offentlig sektor er en service for borgerne." },
                    { 373, 20374, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 374, 20374, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 375, 20375, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 376, 20375, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 377, 20376, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 378, 20376, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 379, 20377, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 380, 20377, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 381, 20378, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 382, 20378, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 383, 20379, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 384, 20379, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 385, 20380, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 386, 20380, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 387, 20381, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 388, 20381, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 389, 20382, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 390, 20382, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 391, 20383, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 392, 20383, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 393, 20384, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 394, 20384, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 395, 20385, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 396, 20385, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 397, 20386, "En effektiv offentlig sektor er en service for borgerne." },
                    { 398, 20386, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 399, 20388, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 400, 20388, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 401, 20389, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 402, 20389, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 403, 20390, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 404, 20390, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 405, 20391, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 406, 20391, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 407, 20392, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 408, 20392, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 409, 20393, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 410, 20393, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 411, 20394, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 412, 20394, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 413, 20395, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 414, 20395, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 415, 20396, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 416, 20396, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 417, 20397, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 418, 20397, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 419, 20398, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 420, 20398, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 421, 20399, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 422, 20399, "En effektiv offentlig sektor er en service for borgerne." },
                    { 423, 20400, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 424, 20400, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 425, 20411, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 426, 20411, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 427, 20415, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 428, 20415, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 429, 20423, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 430, 20423, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 431, 20496, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 432, 20496, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 433, 20546, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 434, 20546, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 435, 20559, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 436, 20559, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 437, 20580, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 438, 20580, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 439, 20781, "En robust økonomi giver os råderum til at investere i velfærd." },
                    { 440, 20781, "Børns trivsel og udvikling skal altid have førsteprioritet." },
                    { 441, 20798, "Vi må sikre en værdig ældrepleje for dem, der har bygget vores samfund." },
                    { 442, 20798, "Digitalisering byder på store muligheder, men kræver også omtanke." },
                    { 443, 20820, "Et stærkt civilsamfund bidrager til et rigere og mere mangfoldigt Danmark." },
                    { 444, 20820, "Retssikkerhed og lighed for loven er grundpiller i vores demokrati." },
                    { 445, 20925, "Vi skal lytte til borgerne og inddrage dem mere i de politiske processer." },
                    { 446, 20925, "Innovation og iværksætteri er nøglen til fremtidig vækst." },
                    { 447, 20962, "En effektiv offentlig sektor er en service for borgerne." },
                    { 448, 20962, "Kunst og kultur beriger vores liv og styrker vores fællesskabsfølelse." },
                    { 449, 20966, "Vi har et ansvar for at efterlade en bedre verden til de næste generationer." },
                    { 450, 20966, "Forebyggelse er ofte bedre og billigere end reparation." },
                    { 451, 20976, "Fremtiden kræver modige beslutninger og fælles ansvar." },
                    { 452, 20976, "Vi skal sikre et Danmark i balance, både socialt og økonomisk." },
                    { 453, 21044, "En stærk velfærdsstat er fundamentet for tryghed og lige muligheder." },
                    { 454, 21044, "Investering i uddannelse og forskning er investering i vores fremtid." },
                    { 455, 21076, "Den grønne omstilling er en nødvendighed, vi må gribe som en mulighed." },
                    { 456, 21076, "Dialog og samarbejde på tværs af partiskel er vejen frem." },
                    { 457, 21083, "Det lokale engagement er drivkraften i et levende demokrati." },
                    { 458, 21083, "Vi skal turde tænke nyt for at løse fremtidens udfordringer." },
                    { 459, 21143, "Et konkurrencedygtigt erhvervsliv skaber arbejdspladser og velstand." },
                    { 460, 21143, "Alle borgere fortjener respekt og en fair behandling af systemet." },
                    { 461, 21159, "Transparens og åbenhed er afgørende for tilliden til det politiske system." },
                    { 462, 21159, "Vi skal værne om de danske værdier og vores kulturelle arv." },
                    { 463, 21161, "Internationalt samarbejde er essentielt i en globaliseret verden." },
                    { 464, 21161, "En robust økonomi giver os råderum til at investere i velfærd." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 88);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 89);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 90);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 91);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 92);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 93);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 94);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 95);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 96);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 97);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 98);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 99);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 110);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 113);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 114);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 115);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 116);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 117);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 118);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 119);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 120);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 122);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 123);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 124);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 125);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 126);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 127);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 128);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 129);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 130);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 131);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 132);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 133);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 134);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 135);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 136);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 137);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 138);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 139);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 140);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 141);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 142);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 143);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 144);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 145);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 146);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 147);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 148);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 149);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 150);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 151);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 152);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 153);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 154);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 155);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 156);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 157);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 158);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 159);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 160);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 161);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 162);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 163);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 164);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 165);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 166);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 167);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 168);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 169);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 170);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 171);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 172);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 173);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 174);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 175);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 176);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 177);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 178);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 179);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 180);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 181);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 182);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 183);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 184);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 185);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 186);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 187);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 188);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 189);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 190);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 191);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 192);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 193);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 194);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 195);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 196);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 197);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 198);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 199);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 200);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 201);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 202);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 203);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 204);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 205);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 206);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 207);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 208);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 209);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 210);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 211);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 212);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 213);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 214);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 215);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 216);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 217);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 218);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 219);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 220);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 221);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 222);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 223);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 224);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 225);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 226);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 227);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 228);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 229);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 230);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 231);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 232);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 233);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 234);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 235);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 236);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 237);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 238);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 239);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 240);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 241);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 242);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 243);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 244);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 245);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 246);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 247);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 248);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 249);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 250);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 251);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 252);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 253);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 254);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 255);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 256);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 257);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 258);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 259);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 260);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 261);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 262);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 263);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 264);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 265);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 266);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 267);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 268);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 269);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 270);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 271);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 272);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 273);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 274);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 275);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 276);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 277);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 278);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 279);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 280);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 281);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 282);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 283);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 284);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 285);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 286);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 287);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 288);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 289);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 290);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 291);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 292);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 293);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 294);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 295);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 296);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 297);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 298);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 299);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 300);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 301);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 302);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 303);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 304);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 305);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 306);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 307);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 308);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 309);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 310);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 311);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 312);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 313);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 314);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 315);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 316);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 317);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 318);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 319);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 320);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 321);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 322);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 323);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 324);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 325);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 326);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 327);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 328);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 329);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 330);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 331);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 332);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 333);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 334);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 335);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 336);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 337);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 338);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 339);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 340);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 341);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 342);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 343);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 344);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 345);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 346);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 347);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 348);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 349);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 350);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 351);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 352);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 353);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 354);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 355);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 356);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 357);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 358);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 359);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 360);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 361);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 362);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 363);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 364);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 365);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 366);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 367);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 368);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 369);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 370);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 371);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 372);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 373);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 374);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 375);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 376);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 377);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 378);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 379);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 380);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 381);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 382);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 383);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 384);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 385);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 386);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 387);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 388);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 389);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 390);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 391);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 392);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 393);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 394);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 395);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 396);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 397);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 398);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 399);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 400);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 401);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 402);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 403);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 404);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 405);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 406);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 407);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 408);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 409);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 410);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 411);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 412);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 413);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 414);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 415);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 416);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 417);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 418);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 419);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 420);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 421);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 422);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 423);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 424);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 425);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 426);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 427);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 428);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 429);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 430);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 431);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 432);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 433);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 434);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 435);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 436);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 437);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 438);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 439);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 440);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 441);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 442);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 443);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 444);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 445);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 446);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 447);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 448);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 449);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 450);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 451);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 452);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 453);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 454);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 455);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 456);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 457);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 458);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 459);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 460);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 461);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 462);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 463);

            migrationBuilder.DeleteData(
                table: "PoliticianQuotes",
                keyColumn: "QuoteId",
                keyValue: 464);
        }
    }
}
