using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2XML.Models
{
    public class StudentResult
    {
        public string Name { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public List<string> Disciplines { get; set; }
        public List<string> Grades { get; set; }
    }
}
