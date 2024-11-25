using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Lab2XML.Strategies
{
    public class SaxParsingStrategy : IXmlParsingStrategy
    {
        public void ParseXml(string xmlContent, Action<List<string>, List<string>, List<string>, List<string>> updatePickers)
        {
            var faculties = new List<string>();
            var departments = new List<string>();
            var disciplines = new List<string>();
            var names = new List<string>();

            using (var reader = XmlReader.Create(new StringReader(xmlContent)))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement("Faculty"))
                        faculties.Add(reader.GetAttribute("name"));

                    if (reader.IsStartElement("Department"))
                        departments.Add(reader.GetAttribute("name"));

                    if (reader.IsStartElement("Discipline"))
                        disciplines.Add(reader.GetAttribute("name"));

                    if (reader.IsStartElement("Student"))
                        names.Add(reader.GetAttribute("name"));
                }
            }

            updatePickers(
                faculties.Distinct().ToList(),
                departments.Distinct().ToList(),
                disciplines.Distinct().ToList(),
                names.Distinct().ToList()
            );
        }
    }
}
