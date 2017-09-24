using System;
using System.IO;

namespace Portalworkers.DocxTemplating.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var content = new Content(
                new FieldContent("date", "Invoice date", DateTime.Today.ToString()),
                new FieldContent("invoicenumber", "Invoice no.", "INV-123456"),
                new FieldContent("total", "Total amount", "10")
            );

            content.Tables.Add(
                new TableContent("positions",
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
                )
            );

            using (var ms = new MemoryStream())
            using (var fs = File.OpenRead("InvoiceTemplate.docx"))
            {
                fs.CopyTo(ms);

                var templateProcessor = new TemplateProcessor(ms);
                templateProcessor.FillContent(content);

                File.WriteAllBytes("Invoice.docx", ms.ToArray());
            }
        }
    }
}
