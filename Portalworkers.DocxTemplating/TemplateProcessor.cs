using DocumentFormat.OpenXml.Packaging;
using Portalworkers.DocxTemplating.Grammar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Portalworkers.DocxTemplating
{
    public class TemplateProcessor : IDisposable
    {
        #region Private members
        
        private readonly WordprocessingDocument wordDocument;
        private bool removeContentControls = true;

        #endregion

        #region Constructor

        public TemplateProcessor(Stream templateStream)
        {
            wordDocument = WordprocessingDocument.Open(templateStream, true);
        }

        #endregion

        #region Methods

        public void FillContent(Content content)
        {
            var parts = new List<OpenXmlPart>();
            parts.Add(wordDocument.MainDocumentPart);
            parts.AddRange(wordDocument.MainDocumentPart.Parts.Select(p => p.OpenXmlPart));

            foreach (var part in parts)
            {
                XDocument docx = null;

                using (var str = part.GetStream())
                using (var streamReader = new StreamReader(str))
                using (var xr = XmlReader.Create(streamReader))
                {
                    try
                    {
                        docx = XDocument.Load(xr);
                    }
                    catch (XmlException)
                    {
                        // Empty part
                        continue;
                    }

                    fillPartContent(docx.Root, content);
                }

                using (var xw = XmlWriter.Create(part.GetStream(
                    FileMode.Create,
                    FileAccess.Write)))
                {
                    docx.Save(xw);
                }
            }

            wordDocument.Close();
        }

        private Grammar.Expression parseContentTag(string tag)
        {
            using (var ms = new MemoryStream())
            using (var tw = new StreamWriter(ms, Encoding.UTF8))
            {
                tw.Write(tag);
                tw.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                var scanner = new Grammar.Scanner(ms);
                var p = new Grammar.Parser(scanner);

                p.Parse();

                if (p.errors.count > 0)
                {
                    return null;
                }
                else
                {
                    return p.Expression;
                }
            }
        }

        private TemplateProcessor fillPartContent(XElement part, Content content)
        {
            if (content.Fields != null)
            {
                foreach (var field in content.Fields)
                {
                    var fieldsContentPlaceholders = parseContentPlaceholders(part, field.Name);

                    // Set content control value to the new value
                    foreach (var fieldContentPlaceholder in fieldsContentPlaceholders)
                    {
                        var fillContent = evaluateCondition(fieldContentPlaceholder.Expression, content, null);

                        var contentControl = fieldContentPlaceholder.ContentControl
                           .Element(DocxConstants.SdtContent)
                           .Descendants(DocxConstants.T)
                           .FirstOrDefault();

                        if (fieldContentPlaceholder.Expression is ConditionExpression)
                        {
                            if (!fillContent)
                            {
                                fieldContentPlaceholder.ContentControl.Remove();
                            }
                        }
                        else
                        {
                            var newContent = getFieldContent(fillContent ? field.Value : null, contentControl.GetNamespaceOfPrefix("w"));
                            contentControl.Value = string.Empty;
                            contentControl.Add(newContent);

                            if (removeContentControls)
                            {

                                // Remove the content control for the table and replace it with its contents.
                                var replacementElement = fieldContentPlaceholder.ContentControl.Element(DocxConstants.SdtContent).Elements().Select(x => new XElement(x));
                                fieldContentPlaceholder.ContentControl.ReplaceWith(replacementElement);
                            }
                        }
                    }
                }
            }

            if (content.Tables != null)
            {
                foreach (var table in content.Tables)
                {
                    fillTableContent(part, table, content);
                }
            }

            return this;
        }

        /// <summary>
        /// Gets the placeholders in the given XML element. If the second argument is supplied,
        /// only placeholders matching the field names are returned.
        /// </summary>
        /// <param name="element">
        /// XML element in which to look for placeholders.
        /// </param>
        /// <param name="fieldName">
        /// Optional filter based on the field name.
        /// </param>
        /// <returns>
        /// A list of the found placeholders (includes the XML element for the placeholder 
        /// and the AST of the placeholder's tag).
        /// </returns>
        private List<ParsedContentPlaceholder> parseContentPlaceholders(XElement element, string fieldName)
        {
            return (from std in element.Descendants(DocxConstants.Sdt)
                    where std.Element(DocxConstants.SdtPr) != null && std.Element(DocxConstants.SdtPr).Element(DocxConstants.Tag) != null
                    let tag = std.Element(DocxConstants.SdtPr).Element(DocxConstants.Tag).Attribute(DocxConstants.Val).Value
                    let expression = parseContentTag(tag)
                    where expression != null && ((expression is ConditionExpression && ((ConditionExpression)expression).Condition.References(fieldName)) || (fieldName == null || expression.EffectivePlaceholder?.Name == fieldName))
                    select new ParsedContentPlaceholder()
                    {
                        ContentControl = std,
                        Expression = expression
                    }).ToList();
        }

        /// <summary>
        /// Evaluates whether a placeholder should be filled or not based on the 
        /// (optional) condition in the placeholder's tag.
        /// </summary>
        /// <param name="expression">
        /// The AST of the expression of the placeholder's tag.
        /// </param>
        /// <param name="content">
        /// The content of the doucment.
        /// </param>
        /// <param name="tableContext">
        /// The optional context of the current table. If supplied, the values will be resolved 
        /// from the table context first.
        /// </param>
        /// <returns>
        /// Whether to fill the placeholder.
        /// </returns>
        private bool evaluateCondition(Expression expression, Content content, TableRowContent tableContext)
        {
            var conditionalPlaceholder = expression as ConditionalPlaceholderExpression;

            AbstractCondition condition = null;

            // Not a conditional placeholder
            if (conditionalPlaceholder != null)
            {
                condition = conditionalPlaceholder.Condition;
            }
            else
            {
                var conditionExpressionPlaceholder = expression as ConditionExpression;

                if (conditionExpressionPlaceholder != null)
                {
                    condition = conditionExpressionPlaceholder.Condition;
                }
            }

            if (condition != null)
            {
                return condition.Evaluate(fieldName => getField(fieldName, content, tableContext));
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region Private helpers

        private void fillTableContent(XElement root, TableContent table, Content content)
        {
            var tableContentPlaceholders = parseContentPlaceholders(root, table.Name);

            if (!tableContentPlaceholders.Any())
            {
                return;
            }

            foreach (var tableContentControl in tableContentPlaceholders)
            {
                if(tableContentControl.Expression is ConditionExpression)
                {
                    continue;
                }

                var includeTable = evaluateCondition(tableContentControl.Expression, content, null);
                
                if(!includeTable)
                {
                    tableContentControl.ContentControl.Remove();
                    return;
                }

                var cellContentControl = tableContentControl.ContentControl
                    .Descendants(DocxConstants.Sdt)
                    .FirstOrDefault();

                if (cellContentControl == null)
                {
                    return;
                }

                // Determine the element for the row that contains the content controls.
                // This is the prototype for the rows that the code will generate from data.
                var prototypeRow = tableContentControl.ContentControl
                    .Descendants(DocxConstants.Tr)
                    .FirstOrDefault(r => r.Descendants(DocxConstants.Sdt).Any());

                var newRows = new List<XElement>();

                if (table.Rows != null)
                {
                    foreach (var tableRowContent in table.Rows)
                    {
                        // Clone the prototypeRow.
                        var newRow = new XElement(prototypeRow);

                        var rowPlaceholders = parseContentPlaceholders(newRow, null);

                        foreach (var placeholder in rowPlaceholders)
                        {
                            var fillContent = evaluateCondition(placeholder.Expression, content, tableRowContent);

                            if (placeholder.Expression is ConditionExpression)
                            {
                                if (!fillContent)
                                {
                                    placeholder.ContentControl.Remove();
                                }
                            }
                            else
                            {
                                string fieldName = placeholder.Expression.EffectivePlaceholder.Name;

                                // Get the new value out of contentControlValues.
                                var newValueElement = getField(fieldName, content, tableRowContent);

                                if (newValueElement != null)
                                {
                                    var newContent = getFieldContent(fillContent ? newValueElement.Value : null, prototypeRow.Name.Namespace);

                                    var contentControl = placeholder.ContentControl
                                        .Element(DocxConstants.SdtContent)
                                        .Descendants(DocxConstants.T)
                                        .FirstOrDefault();

                                    contentControl.Value = string.Empty;
                                    contentControl.Add(newContent);
                                }

                                // Fill tables recursively (allowing nested tables).
                                if (tableRowContent.Tables != null)
                                {
                                    var newTableElement = tableRowContent
                                        .Tables
                                        .Where(t => t.Name == fieldName)
                                        .FirstOrDefault();

                                    if (newTableElement != null)
                                    {
                                        fillTableContent(newRow, newTableElement, content);
                                        continue;
                                    }
                                }

                                if (newValueElement == null)
                                {
                                    continue;
                                }

                                if (removeContentControls)
                                {
                                    var replacementElement = placeholder.ContentControl.Element(DocxConstants.SdtContent).Elements().Select(x => new XElement(x));
                                    placeholder.ContentControl.ReplaceWith(replacementElement);
                                }
                            }
                        }

                        newRows.Add(newRow);
                    }
                }

                prototypeRow.AddAfterSelf(newRows);

                var tableElement = prototypeRow.Ancestors(DocxConstants.Tbl).First();
                prototypeRow.Remove();

                if (removeContentControls == true)
                {
                    var tableClone = new XElement(tableElement);
                    tableContentControl.ContentControl.ReplaceWith(tableClone);
                }
            }
        }

        private FieldContent getField(string fieldName, Content content, TableRowContent tableContext)
        {
            FieldContent field = null;

            if(tableContext != null)
            {
                field = tableContext.Fields.FirstOrDefault(f => f.Name == fieldName);
            }

            if(field == null)
            {
                field = content.Fields.FirstOrDefault(f => f.Name == fieldName);
            }

            return field;
        }

        private object getFieldContent(string value, XNamespace wNs)
        {
            object nc = value ?? string.Empty;

            if (value != null && value.Contains("\n"))
            {
                var run = new XElement(wNs + "r");
                var lines = value.Split(new[] { "\n" }, StringSplitOptions.None);

                for (var i = 0; i < lines.Length; i++)
                {
                    run.Add(new XElement(wNs + "t", lines[i]));

                    if (i != lines.Length - 1)
                    {
                        run.Add(new XElement(wNs + "br"));
                    }
                }

                nc = run;
            }

            return nc;
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            if (wordDocument == null)
            {
                return;
            }

            wordDocument.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}