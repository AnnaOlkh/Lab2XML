using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Lab2XML.Models;

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
        /*public List<StudentResult> Search(string xmlContent, string faculty, string department, string discipline, string name)
        {
            var doc = XDocument.Parse(xmlContent);

            var students = doc.Descendants("Student")
                .Where(s =>
                    (string.IsNullOrEmpty(faculty) || s.Ancestors("Faculty").FirstOrDefault()?.Attribute("name")?.Value == faculty) &&
                    (string.IsNullOrEmpty(department) || s.Ancestors("Department").FirstOrDefault()?.Attribute("name")?.Value == department) &&
                    (string.IsNullOrEmpty(discipline) || s.Elements("Discipline").Any(d => d.Attribute("name")?.Value == discipline)) &&
                    (string.IsNullOrEmpty(name) || s.Attribute("name")?.Value == name))
                .Select(s => new StudentResult
                {
                    Name = s.Attribute("name")?.Value,
                    Faculty = s.Ancestors("Faculty").FirstOrDefault()?.Attribute("name")?.Value,
                    Department = s.Ancestors("Department").FirstOrDefault()?.Attribute("name")?.Value,
                    Disciplines = s.Elements("Discipline").Select(d => d.Attribute("name")?.Value).FirstOrDefault(),
                    Grades = s.Elements("Discipline").Select(d => d.Attribute("grade")?.Value).FirstOrDefault()
                })
                .ToList();

            return students;
        }*/
        public List<StudentResult> Search(string xmlContent, string faculty, string department, string discipline, string name)
        {
            var results = new List<StudentResult>();
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xmlContent);

            foreach (System.Xml.XmlNode studentNode in doc.GetElementsByTagName("Student"))
            {
                var disciplines = new List<string>();
                var grades = new List<string>();

                var currentFaculty = studentNode.ParentNode?.ParentNode?.Attributes["name"]?.Value;
                var currentDepartment = studentNode.ParentNode?.Attributes["name"]?.Value;
                var currentStudentName = studentNode.Attributes["name"]?.Value;

                foreach (System.Xml.XmlNode disciplineNode in studentNode.SelectNodes("Discipline"))
                {
                    disciplines.Add(disciplineNode.Attributes["name"]?.Value);
                    grades.Add(disciplineNode.Attributes["grade"]?.Value);
                }

                // Фільтрація за умовами
                if ((string.IsNullOrEmpty(faculty) || currentFaculty == faculty) &&
                    (string.IsNullOrEmpty(department) || currentDepartment == department) &&
                    (string.IsNullOrEmpty(name) || currentStudentName == name) &&
                    (string.IsNullOrEmpty(discipline) || disciplines.Contains(discipline)))
                {
                    results.Add(new StudentResult
                    {
                        Name = currentStudentName,
                        Faculty = currentFaculty,
                        Department = currentDepartment,
                        Disciplines = disciplines,
                        Grades = grades
                    });
                }
            }

            return results;
        }
    }
}
