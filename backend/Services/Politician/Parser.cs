using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace backend.Services.Politician
{
    public static class BioParser
    {
        /// <summary>
        /// Extracts publication titles enclosed in »...« from a text block.
        /// </summary>
        private static List<string> ExtractPublicationTitles(string? publicationsText)
        {
            var titles = new List<string>();
            if (string.IsNullOrWhiteSpace(publicationsText))
            {
                return titles;
            }
            var regex = new Regex(@"»(.*?)«");
            MatchCollection matches = regex.Matches(publicationsText);
            foreach (Match match in matches.Where(m => m.Success && m.Groups.Count > 1))
            {
                string title = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(title))
                {
                    titles.Add(title);
                }
            }
            return titles;
        }

        /// <summary>
        /// Parses an XML string representing member biography data.
        /// </summary>
        public static Dictionary<string, object> ParseBiografiXml(string? biografiXml)
        {
            var extractedData = new Dictionary<string, object>();
            if (string.IsNullOrWhiteSpace(biografiXml))
            {
                return extractedData;
            }

            try
            {
                XDocument doc = XDocument.Parse(biografiXml);
                XElement? memberElement = doc.Root;

                if (memberElement == null || memberElement.Name != "member")
                {
                     // Optionally log or handle cases where the root is not <member>
                     Console.WriteLine("Warning: Biography XML root element is not <member> or is null.");
                     return extractedData; // Return empty if structure is unexpected
                }

                // --- Direct Elements ---
                extractedData["Status"] = memberElement.Element("status")?.Value ?? "";
                extractedData["Sex"] = memberElement.Element("sex")?.Value ?? "";
                extractedData["EducationStatistic"] = memberElement.Element("educationStatistic")?.Value ?? "";
                extractedData["Party"] = memberElement.Element("party")?.Value ?? "";
                extractedData["PartyShortname"] = memberElement.Element("partyShortname")?.Value ?? "";
                extractedData["Born"] = memberElement.Element("born")?.Value ?? "";
                extractedData["PictureMiRes"] = memberElement.Element("pictureMiRes")?.Value ?? "";
                extractedData["firstname_from_bio"] = memberElement.Element("firstname")?.Value ?? "";
                extractedData["lastname_from_bio"] = memberElement.Element("lastname")?.Value ?? "";

                // --- Nested Elements ---

                // Email (Single)
                extractedData["Email"] = memberElement.Element("emails")?.Element("email")?.Value ?? "";

                // Personal Information -> Function
                XElement? function = memberElement.Element("personalInformation")?.Element("function");
                extractedData["FunctionFormattedTitle"] = function?.Element("formattedTitle")?.Value ?? "";
                extractedData["FunctionStartDate"] = function?.Element("functionStartDate")?.Value ?? "";

                // Career Elements
                XElement? career = memberElement.Element("career");

                // Constituencies (List)
                extractedData["Constituencies"] = career?.Element("constituencies")
                                                   ?.Elements("constituency")
                                                   .Select(el => el.Value.Trim())
                                                   .Where(s => !string.IsNullOrEmpty(s))
                                                   .ToList() ?? new List<string>();

                // Nominations (List)
                extractedData["Nominations"] = career?.Element("nominations")
                                                 ?.Elements("nomination")
                                                 .Select(el => el.Value.Trim())
                                                 .Where(s => !string.IsNullOrEmpty(s))
                                                 .ToList() ?? new List<string>();

                // Parliamentary Positions Of Trust (List) - Fixed Parsing
                extractedData["ParliamentaryPositionsOfTrust"] = career?.Element("parliamentaryPositionsOfTrust")
                                                                   ?.Elements("parliamentaryPositionOfTrust")
                                                                   .Select(el => Regex.Replace(el.Value, "<.*?>", string.Empty).Trim())
                                                                   .Where(s => !string.IsNullOrEmpty(s))
                                                                   .ToList() ?? new List<string>();
                // Add Ministers if needed
                extractedData["Ministers"] = career?.Element("ministers")?.Elements("minister").Select(el => el.Value.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>();


                // Educations (List)
                extractedData["Educations"] = memberElement.Element("educations")
                                                ?.Elements("education")
                                                .Select(el => el.Value.Trim())
                                                .Where(s => !string.IsNullOrEmpty(s))
                                                .ToList() ?? new List<string>();

                // Occupations (List)
                extractedData["Occupations"] = memberElement.Element("occupations")
                                               ?.Elements("occupation")
                                               .Select(el => el.Value.Trim())
                                               .Where(s => !string.IsNullOrEmpty(s))
                                               .ToList() ?? new List<string>();

                // Positions Of Trust (Single Text Field - might contain HTML)
                extractedData["PositionsOfTrust"] = memberElement.Element("positionsOfTrust")?.Element("editorFormattedText")?.Value ??
                                                    memberElement.Element("positionsOfTrust")?.Element("positionOfTrust")?.Value ?? "";

                // Publications
                string? publicationsText = memberElement.Element("publications")?.Element("editorFormattedText")?.Value;
                extractedData["PublicationTitles"] = ExtractPublicationTitles(publicationsText);

                 // Add Spokesmen if needed
                extractedData["Spokesmen"] = memberElement.Element("spokesmen")?.Elements("spokesman").Select(el => el.Value.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>();

            }
            catch (System.Xml.XmlException ex)
            {
                Console.WriteLine($"Error parsing biography XML: {ex.Message}");
                // Consider logging the error more formally
            }
            catch (Exception ex) // Catch other potential errors during parsing
            {
                 Console.WriteLine($"An unexpected error occurred during biography parsing: {ex.Message}");
            }

            return extractedData;
        }
    }
}
