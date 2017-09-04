namespace Portalworkers.DocxTemplating.Grammar
{
    public class ConditionExpression : Expression
    {
        public AbstractCondition Condition { get; set; }

        public override SimplePlaceholderExpression EffectivePlaceholder
        {
            get
            {
                return null;
            }
        }
    }
}