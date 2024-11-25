using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2XML.Strategies
{
    public interface IXmlParsingStrategy
    {
        void ParseXml(string xmlContent, Action<List<string>, List<string>, List<string>, List<string>> updatePickers);
    }
}

