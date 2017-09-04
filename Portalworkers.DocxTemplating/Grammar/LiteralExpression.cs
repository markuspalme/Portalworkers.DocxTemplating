namespace Portalworkers.DocxTemplating.Grammar
{
    public class LiteralExpression
    {
        public LiteralExpression(string value)
        {
            this.Value = value;
        }

        public string Value { get; set; }
    }
}