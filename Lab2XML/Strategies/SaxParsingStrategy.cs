using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Lab2XML.Models;

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

        public List<StudentResult> Search(string xmlContent, string faculty, string department, string discipline, string name)
        {
            var results = new List<StudentResult>();

            using (var reader = System.Xml.XmlReader.Create(new StringReader(xmlContent)))
            {
                string currentFaculty = null;
                string currentDepartment = null;
                string currentStudentName = null;
                var disciplines = new List<string>();
                var grades = new List<string>();

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Faculty")
                    {
                        currentFaculty = reader.GetAttribute("name");
                    }
                    else if (reader.NodeType == XmlNodeType.Element && reader.Name == "Department")
                    {
                        currentDepartment = reader.GetAttribute("name");
                    }
                    else if (reader.NodeType == XmlNodeType.Element && reader.Name == "Student")
                    {
                        currentStudentName = reader.GetAttribute("name");
                        disciplines.Clear();
                        grades.Clear();
                    }
                    else if (reader.NodeType == XmlNodeType.Element && reader.Name == "Discipline")
                    {
                        disciplines.Add(reader.GetAttribute("name"));
                        grades.Add(reader.GetAttribute("grade"));
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Student")
                    {
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
                                Disciplines = new List<string>(disciplines),
                                Grades = new List<string>(grades)
                            });
                        }
                    }
                }
            }

            return results;
        }
    }

}
