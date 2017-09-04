using System.Xml.Linq;

namespace Portalworkers.DocxTemplating
{
    public class ParsedContentPlaceholder
    {
        public XElement ContentControl { get; set; }

        public Grammar.Expression Expression { get; set; }
    }
}
