namespace Portalworkers.DocxTemplating.Grammar
{
    public class SimplePlaceholderExpression : Expression
    {
        public SimplePlaceholderExpression(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }

        public override SimplePlaceholderExpression EffectivePlaceholder
        {
            get
            {
                return this;
            }
        }
    }
}
