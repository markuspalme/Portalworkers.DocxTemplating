using System.Collections.Generic;
using System.Linq;

namespace Portalworkers.DocxTemplating
{
    public class Content
    {
        #region Constructor

        public Content()
        {
            Tables = new List<TableContent>();
            Fields = new List<FieldContent>();
        }

        public Content(params FieldContent[] fields) : this()
        {
            this.Fields.AddRange(fields);
        }

        #endregion

        #region Properties

        public List<TableContent> Tables { get; set; }

        public List<FieldContent> Fields { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Reflects the fields in the content and adds a metadata table which can
        /// be used to generate a documentation of the available fields.
        /// </summary>
        public void AddMetadata()
        {
            var tableMeta = new List<TableContent>();

            // List flat fields
            tableMeta.Add(new TableContent()
            {
                Name = "fields",
                Rows = this.Fields.Select(f => new TableRowContent(
                    new FieldContent("field.name", "", f.Name),
                    new FieldContent("field.label", "", f.Label),
                    new FieldContent("field.example", "", f.Value)))
            });

            var tablesTable = new TableContent()
            {
                Name = "tables"
            };

            var tableRows = new List<TableRowContent>();

            foreach (var t in this.Tables)
            {
                getTableMeta(tableRows, t);
            }

            tableMeta.Add(new TableContent()
            {
                Name = "tables",
                Rows = tableRows
            });

            this.Tables.AddRange(tableMeta);
        }

        #endregion

        #region Private helpers

        private void getTableMeta(List<TableRowContent> tableRows, TableContent t)
        {
            tableRows.Add(new TableRowContent()
            {
                Fields = new[] { new FieldContent("table.name", "Table name", t.Name) },
                Tables = new[] { new TableContent()
                        {
                            Name = "tablefields",
                            Rows = t.Rows.Any() ? t.Rows.First().Fields.Select(tf => new TableRowContent(new[] {
                                new FieldContent("tablefield.name", "Name", tf.Name),
                                new FieldContent("tablefield.label", "Label", tf.Label),
                                new FieldContent("tablefield.example", "Example", tf.Value) })) : null
                        }
                    }
            });

            if (t.Rows.Any() && t.Rows.First().Tables != null)
            {
                getTableMeta(tableRows, t.Rows.First().Tables.First());
            }
        }

        #endregion
    }
}
