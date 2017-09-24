using System;
using System.IO;

namespace Portalworkers.DocxTemplating.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var content = new Content(
                new FieldContent("name", "Full name", "John Doe")
            );

            content.Tables.Add(new TableContent("positions",
                new TableRowContent(
                    new FieldContent("qty", "Quantity", "1"),
                    new FieldContent("item", "Item", "Paper clips"),
                    new FieldContent("subtotal", "Line amount", "2")
                ),
                new TableRowContent(
                    new FieldContent("qty", "Quantity", "8"),
                    new FieldContent("item", "Item", "Envelopes"),
                    new FieldContent("subtotal", "Line amount", "8")
                )
            ));

            using (var fs = File.OpenRead("Template.docx"))
            using (var ms = new MemoryStream())
            {
                fs.CopyTo(ms);
                var templateProcessor = new TemplateProcessor(ms);
                templateProcessor.FillContent(content);

                File.WriteAllBytes("Document.docx", ms.ToArray());
            }
        }
    }
}
