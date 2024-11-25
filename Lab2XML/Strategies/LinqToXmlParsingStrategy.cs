using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Lab2XML.Strategies
{
    public class LinqToXmlParsingStrategy : IXmlParsingStrategy
    {
        public void ParseXml(string xmlContent, Action<List<string>, List<string>, List<string>, List<string>> updatePickers)
        {
            var document = XDocument.Parse(xmlContent);

            var faculties = document.Descendants("Faculty")
                .Select(f => f.Attribute("name")?.Value)
                .Where(f => !string.IsNullOrEmpty(f))
                .Distinct()
                .ToList();

            var departments = document.Descendants("Department")
                .Select(d => d.Attribute("name")?.Value)
                .Where(d => !string.IsNullOrEmpty(d))
                .Distinct()
                .ToList();

            var disciplines = document.Descendants("Discipline")
                .Select(d => d.Attribute("name")?.Value)
                .Where(d => !string.IsNullOrEmpty(d))
                .Distinct()
                .ToList();

            var names = document.Descendants("Student")
                .Select(s => s.Attribute("name")?.Value)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToList();

            updatePickers(faculties, departments, disciplines, names);
        }
    }
}
