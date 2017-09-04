namespace Portalworkers.DocxTemplating
{
    public class FieldContent
    {
        public FieldContent()
        {
        }

        public FieldContent(string name, string label, string value)
        {
            Name = name;
            Label = label;
            Value = value;
        }

        public string Name { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
    }
}
