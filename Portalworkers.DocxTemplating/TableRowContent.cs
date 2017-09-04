using System.Collections.Generic;

namespace Portalworkers.DocxTemplating
{
    public class TableRowContent
    {
        public TableRowContent()
        {
        }

        public TableRowContent(IEnumerable<FieldContent> fields)
        {
            Fields = fields;
        }

        public TableRowContent(params FieldContent[] fields)
        {
            Fields = fields;
        }

        public IEnumerable<FieldContent> Fields { get; set; }

        public IEnumerable<TableContent> Tables { get; set; }
    }
}
