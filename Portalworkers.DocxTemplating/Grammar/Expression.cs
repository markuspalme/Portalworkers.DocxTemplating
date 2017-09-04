namespace Portalworkers.DocxTemplating.Grammar
{
    public abstract class Expression
    {
        public abstract SimplePlaceholderExpression EffectivePlaceholder { get; }
    }
}