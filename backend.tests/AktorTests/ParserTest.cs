using backend.Services.Politicians;

namespace backend.Services.Tests
{
    [TestFixture]
    public class ParserServiceTests
    {
        [SetUp]
        public void SetUp() { }

        [Test]
        public void ParseBiografiXml_NullInput_ReturnsEmptyDictionary()
        {
            // Arrange
            string? xmlInput = null;

            // Act
            var result = BioParser.ParseBiografiXml(xmlInput);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ParseBiografiXml_EmptyInput_ReturnsEmptyDictionary()
        {
            // Arrange
            string xmlInput = "";

            // Act
            var result = BioParser.ParseBiografiXml(xmlInput);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ParseBiografiXml_IncorrectXml_ReturnsEmptyDictionary()
        {
            // Arrange
            string xmlInput = "<member><status>Active</status><malformed>";

            // Act
            var result = BioParser.ParseBiografiXml(xmlInput);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ParseBiografiXml_WithProvidedComplexXml_ParsesAllFieldsCorrectly()
        {
            // Arrange
            string biografiXml =
                @"
<member>
    <url/>
    <status>1</status>
    <sex>Kvinde</sex>
    <educationStatistic>LVU</educationStatistic>
    <occupationStatistic>Privat</occupationStatistic>
    <title>Mette Frederiksen (S)</title>
    <firstname>Mette</firstname>
    <lastname>Frederiksen</lastname>
    <profession>Statsminister</profession>
    <party>Socialdemokratiet</party>
    <partyShortname>S</partyShortname>
    <formattedDateLongMonth/>
    <born>19-11-1977</born>
    <died/>
    <pictureMiRes>https://www.ft.dk/-/media/cv/foto/2022/s/mette-frederiksen/mette_frederiksen_500.ashx</pictureMiRes>
    <pictureHiRes>https://www.ft.dk/-/media/cv/foto/2022/s/mette-frederiksen/mette_frederiksen-fotograf_socialdemokratiet.zip</pictureHiRes>
    <addresses><address>Statsministeriet, Christiansborg, Prins Jørgens Gård 11 1218&amp;nbsp;København K</address></addresses>
    <phoneFolketinget/>
    <mobilePhone/>
    <privatePhone/>
    <ministerPhone>+45 3392 3300</ministerPhone>
    <fax/>
    <emails><email>stm@stm.dk</email></emails>
    <personalInformation>
        <memberData>&lt;p&gt;Mette Frederiksen, f&amp;oslash;dt 19. november 1977 i Aalborg, datter af typograf Flemming Frederiksen og p&amp;aelig;dagog Anette Frederiksen.&amp;nbsp;Har datteren Ida Feline og s&amp;oslash;nnen Magne.&lt;/p&gt;</memberData>
        <function>
            <formattedTitle>Medlem af Folketinget;Minister;Tidligere minister</formattedTitle>
            <formattedTitles>Medlem af Folketinget;Minister;Tidligere minister</formattedTitles>
            <functionStartDate>20. november 2001</functionStartDate>
        </function>
    </personalInformation>
    <career>
        <ministers>
            <minister>Justitsminister, 10. oktober 2014 – 28. juni 2015.</minister>
            <minister>Beskæftigelsesminister, 3. oktober 2011 – 10. oktober 2014.</minister>
            <minister>Statsminister fra 27. juni 2019.</minister>
        </ministers>
        <presidents/>
        <presidiums/>
        <constituencies>
            <constituency>Folketingsmedlem for Socialdemokratiet i Nordjyllands Storkreds fra 5. juni 2019.</constituency>
            <constituency>Folketingsmedlem for Socialdemokratiet i Københavns Omegns Storkreds, 13. november 2007 – 5. juni 2019.</constituency>
            <constituency>Folketingsmedlem for Socialdemokratiet i Københavns Amtskreds, 20. november 2001 – 13. november 2007.</constituency>
        </constituencies>
        <substitutes/>
        <nominations>
            <nomination>Kandidat for Socialdemokratiet i Aalborg Østkredsen fra 2018.</nomination>
            <nomination>Kandidat for Socialdemokratiet i Ballerupkredsen, 2000-2018.</nomination>
        </nominations>
        <parliamentaryPositionsOfTrust>
            <parliamentaryPositionOfTrust/>
            <parliamentaryPositionOfTrust>Formand for Socialdemokratiet fra 2015.&lt;br/&gt;</parliamentaryPositionOfTrust>
            <parliamentaryPositionOfTrust>Næstformand for Socialdemokratiets folketingsgruppe, 2005-2011.&lt;br/&gt;</parliamentaryPositionOfTrust>
            <parliamentaryPositionOfTrust>Tidligere socialordfører, kulturordfører og ligestillingsordfører.&lt;br/&gt;</parliamentaryPositionOfTrust>
        </parliamentaryPositionsOfTrust>
        <auditors/>
    </career>
    <educations>
        <education>Masteruddannelse i afrikastudier, Københavns Universitet, 2009.</education>
        <education>Bachelor i administration og samfundsfag, Aalborg Universitet, 2007.</education>
        <education>Student, Aalborghus Gymnasium, 1993-1996.</education>
        <education>Byplanvejens Skole, 1983-1993.</education>
    </educations>
    <occupations>
        <occupation>Ungdomskonsulent i LO, 2000-2001.</occupation>
    </occupations>
    <positionsOfTrust><positionOfTrust/></positionsOfTrust>
    <publications>
        <editorFormattedText>&lt;p&gt;Har bidraget til&amp;nbsp;&amp;raquo;Fra kamp til kultur&amp;nbsp;&amp;ndash;&amp;nbsp;20 smagsdommere skyder med skarpt&amp;laquo;, 2004 og&amp;nbsp;&amp;raquo;Epostler&amp;laquo;, 2003.&amp;nbsp;&lt;/p&gt;</editorFormattedText>
    </publications>
    <honours><editorFormattedText>Modtog Ting-Prisen i 2012 og Nina Bang-prisen i 2002.</editorFormattedText></honours>
    <spokesmen/>
    <Websites><WebsiteUrl><Url>http://www.stm.dk</Url><Desciption>www.stm.dk</Desciption><LinkType>external</LinkType></WebsiteUrl></Websites>
</member>";

            // Act
            var result = BioParser.ParseBiografiXml(biografiXml);

            // Assert
            Assert.That(result, Is.Not.Null);

            // --- Direct Elements ---
            Assert.That(result["Status"], Is.EqualTo("1"));
            Assert.That(result["Sex"], Is.EqualTo("Kvinde"));
            Assert.That(result["EducationStatistic"], Is.EqualTo("LVU"));
            Assert.That(result["Party"], Is.EqualTo("Socialdemokratiet"));
            Assert.That(result["PartyShortname"], Is.EqualTo("S"));
            Assert.That(result["Born"], Is.EqualTo("19-11-1977"));
            Assert.That(
                result["PictureMiRes"],
                Is.EqualTo(
                    "https://www.ft.dk/-/media/cv/foto/2022/s/mette-frederiksen/mette_frederiksen_500.ashx"
                )
            );
            Assert.That(result["firstname_from_bio"], Is.EqualTo("Mette"));
            Assert.That(result["lastname_from_bio"], Is.EqualTo("Frederiksen"));

            // --- Nested Elements ---
            Assert.That(result["Email"], Is.EqualTo("stm@stm.dk"));
            Assert.That(
                result["FunctionFormattedTitle"],
                Is.EqualTo("Medlem af Folketinget;Minister;Tidligere minister")
            );
            Assert.That(result["FunctionStartDate"], Is.EqualTo("20. november 2001"));

            // --- Career Elements (Lists) ---
            var expectedMinisters = new List<string>
            {
                "Justitsminister, 10. oktober 2014 – 28. juni 2015.",
                "Beskæftigelsesminister, 3. oktober 2011 – 10. oktober 2014.",
                "Statsminister fra 27. juni 2019.",
            };
            Assert.That((List<string>)result["Ministers"], Is.EqualTo(expectedMinisters));

            var expectedConstituencies = new List<string>
            {
                "Folketingsmedlem for Socialdemokratiet i Nordjyllands Storkreds fra 5. juni 2019.",
                "Folketingsmedlem for Socialdemokratiet i Københavns Omegns Storkreds, 13. november 2007 – 5. juni 2019.",
                "Folketingsmedlem for Socialdemokratiet i Københavns Amtskreds, 20. november 2001 – 13. november 2007.",
            };
            Assert.That((List<string>)result["Constituencies"], Is.EqualTo(expectedConstituencies));

            var expectedNominations = new List<string>
            {
                "Kandidat for Socialdemokratiet i Aalborg Østkredsen fra 2018.",
                "Kandidat for Socialdemokratiet i Ballerupkredsen, 2000-2018.",
            };
            Assert.That((List<string>)result["Nominations"], Is.EqualTo(expectedNominations));

            // ParliamentaryPositionsOfTrust
            var expectedParliamentaryPositions = new List<string>
            {
                "Formand for Socialdemokratiet fra 2015.",
                "Næstformand for Socialdemokratiets folketingsgruppe, 2005-2011.",
                "Tidligere socialordfører, kulturordfører og ligestillingsordfører.",
            };
            Assert.That(
                (List<string>)result["ParliamentaryPositionsOfTrust"],
                Is.EqualTo(expectedParliamentaryPositions)
            );

            // --- Educations (List) ---
            var expectedEducations = new List<string>
            {
                "Masteruddannelse i afrikastudier, Københavns Universitet, 2009.",
                "Bachelor i administration og samfundsfag, Aalborg Universitet, 2007.",
                "Student, Aalborghus Gymnasium, 1993-1996.",
                "Byplanvejens Skole, 1983-1993.",
            };
            Assert.That((List<string>)result["Educations"], Is.EqualTo(expectedEducations));

            // --- Occupations (List) ---
            var expectedOccupations = new List<string> { "Ungdomskonsulent i LO, 2000-2001." };
            Assert.That((List<string>)result["Occupations"], Is.EqualTo(expectedOccupations));

            // --- Positions Of Trust ---
            Assert.That(result["PositionsOfTrust"], Is.EqualTo(""));

            Assert.That(
                (List<string>)result["PublicationTitles"],
                Is.TypeOf<List<string>>().And.Empty
            );

            // --- Spokesmen ---
            // The XML has <spokesmen/>, so the list should be empty.
            Assert.That(result["Spokesmen"], Is.TypeOf<List<string>>().And.Empty);
        }
    }
}
