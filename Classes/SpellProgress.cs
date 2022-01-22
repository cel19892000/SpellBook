using HtmlAgilityPack;
using System;

namespace SpellBook
{
    public class SpellProgress
    {

        public string percentage;
        public string level;

        public SpellProgress() { }

        public SpellProgress(string spellName, HtmlDocument doc)
        {
            percentage = "0";
            level = "0";

            int knownSpellCount = doc.DocumentNode.SelectNodes("//table[@id='spell-data'][1]//tbody//tr").Count;

            for (int i = 1; i <= knownSpellCount; i++)
            {
                string spellID = doc.DocumentNode.SelectSingleNode("//*[@id=\"spell-data\"]/tbody/tr[" + i + "]/th").InnerText;

                if (String.Equals(spellID, spellName, StringComparison.OrdinalIgnoreCase))
                {
                    string location = "//*[@id=\"spell-data\"]/tbody/tr[" + i + "]/td/div";
                    percentage = SearchAttribute(doc, location, "data-percent");
                    level = SearchAttribute(doc, location, "data-content");
                    return;
                }
            }

        }

        public string SearchAttribute(HtmlDocument doc, string location, string attributeType)
        {
            return doc.DocumentNode.SelectSingleNode(location).Attributes[attributeType].Value;
        }

    }
}
