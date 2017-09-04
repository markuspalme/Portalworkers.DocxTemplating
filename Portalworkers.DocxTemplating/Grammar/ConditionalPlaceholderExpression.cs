namespace Portalworkers.DocxTemplating.Grammar
{
    public class ConditionalPlaceholderExpression : Expression
    {
        public SimplePlaceholderExpression Placeholder { get; set; }

        public AbstractCondition Condition { get; set; }

        public override SimplePlaceholderExpression EffectivePlaceholder
        {
            get
            {
                return Placeholder;
            }
        }
    }
}