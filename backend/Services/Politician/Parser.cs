using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace backend.Services.Politicians
{
    public static class BioParser
    {
        //Helper til at udtrække bog titler politikere har udgivet. skal udviddes for fuld coverage af alle partiers politikere,
        //da struktur er forskellige, men nogle bruger '>> titel <<>> titel2 <<', virker i dette tilfælde
        private static List<string> ExtractPublicationTitles(string? publicationsText)
        {
            var titles = new List<string>();
            if (string.IsNullOrWhiteSpace(publicationsText))
            {
                return titles;
            }
            var regex = new Regex(@"»(.*?)«"); //bedste forsøg
            MatchCollection matches = regex.Matches(publicationsText); // match regex på streng
            foreach (Match match in matches.Where(m => m.Success && m.Groups.Count > 1))
            {
                string title = match.Groups[1].Value.Trim(); //trim trailing white space
                if (!string.IsNullOrEmpty(title))
                {
                    titles.Add(title); //tilføj titler
                }
            }
            return titles;
        }

        //Biografi parser, opretter en dictionarary af streng(datapunkt) og værdi (base object)
        public static Dictionary<string, object> ParseBiografiXml(string? biografiXml)
        {
            var extractedData = new Dictionary<string, object>();
            if (string.IsNullOrWhiteSpace(biografiXml))
            {
                return extractedData;
            }

            try
            { //anvend system xml linq
                XDocument doc = XDocument.Parse(biografiXml);
                XElement? memberElement = doc.Root;

                if (memberElement == null || memberElement.Name != "member")
                {
                    return extractedData;
                }

                // --- Direkte Elementer ---
                extractedData["Status"] = memberElement.Element("status")?.Value ?? "";
                extractedData["Sex"] = memberElement.Element("sex")?.Value ?? "";
                extractedData["EducationStatistic"] =
                    memberElement.Element("educationStatistic")?.Value ?? "";
                extractedData["Party"] = memberElement.Element("party")?.Value ?? "";
                extractedData["PartyShortname"] =
                    memberElement.Element("partyShortname")?.Value ?? "";
                extractedData["Born"] = memberElement.Element("born")?.Value ?? "";
                extractedData["PictureMiRes"] = memberElement.Element("pictureMiRes")?.Value ?? "";
                extractedData["firstname_from_bio"] =
                    memberElement.Element("firstname")?.Value ?? "";
                extractedData["lastname_from_bio"] = memberElement.Element("lastname")?.Value ?? "";

                // --- Nested Elementer ---

                // Email (Single)
                extractedData["Email"] =
                    memberElement.Element("emails")?.Element("email")?.Value ?? "";

                // Personal Information -> Function
                XElement? function = memberElement
                    .Element("personalInformation")
                    ?.Element("function");
                extractedData["FunctionFormattedTitle"] =
                    function?.Element("formattedTitle")?.Value ?? "";
                extractedData["FunctionStartDate"] =
                    function?.Element("functionStartDate")?.Value ?? "";

                // Career Elements
                XElement? career = memberElement.Element("career");

                // Constituencies (List)
                extractedData["Constituencies"] =
                    career
                        ?.Element("constituencies")
                        ?.Elements("constituency")
                        .Select(el => el.Value.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList() ?? new List<string>();

                // Nominations (List)
                extractedData["Nominations"] =
                    career
                        ?.Element("nominations")
                        ?.Elements("nomination")
                        .Select(el => el.Value.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList() ?? new List<string>();

                // Parliamentary Positions Of Trust
                extractedData["ParliamentaryPositionsOfTrust"] =
                    career
                        ?.Element("parliamentaryPositionsOfTrust")
                        ?.Elements("parliamentaryPositionOfTrust")
                        .Select(el => Regex.Replace(el.Value, "<.*?>", string.Empty).Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList() ?? new List<string>();
                // Add Ministers if needed
                extractedData["Ministers"] =
                    career
                        ?.Element("ministers")
                        ?.Elements("minister")
                        .Select(el => el.Value.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList() ?? new List<string>();

                // Educations (List)
                extractedData["Educations"] =
                    memberElement
                        .Element("educations")
                        ?.Elements("education")
                        .Select(el => el.Value.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList() ?? new List<string>();

                // Occupations (List)
                extractedData["Occupations"] =
                    memberElement
                        .Element("occupations")
                        ?.Elements("occupation")
                        .Select(el => el.Value.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList() ?? new List<string>();

                // Positions Of Trust
                extractedData["PositionsOfTrust"] =
                    memberElement.Element("positionsOfTrust")?.Element("editorFormattedText")?.Value
                    ?? memberElement.Element("positionsOfTrust")?.Element("positionOfTrust")?.Value
                    ?? "";

                // Publications (Det er denne der skal udviddes)
                string? publicationsText = memberElement
                    .Element("publications")
                    ?.Element("editorFormattedText")
                    ?.Value;
                extractedData["PublicationTitles"] = ExtractPublicationTitles(publicationsText);

                // Add Spokesmen
                extractedData["Spokesmen"] =
                    memberElement
                        .Element("spokesmen")
                        ?.Elements("spokesman")
                        .Select(el => el.Value.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList() ?? new List<string>();
            }
            catch (System.Xml.XmlException ex)
            {
                Console.WriteLine($"Error parsing biography XML: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"An unexpected error occurred during biography parsing: {ex.Message}"
                );
            }

            return extractedData;
        }
    }
}
