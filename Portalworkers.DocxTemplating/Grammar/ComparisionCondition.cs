using System;

namespace Portalworkers.DocxTemplating.Grammar
{
    public class ComparisionCondition : AbstractCondition
    {
        #region Properties

        public SimplePlaceholderExpression Lhs { get; set; }

        public ConditionOperator Operator { get; set; }

        public LiteralExpression Rhs { get; set; }

        #endregion

        public override bool Evaluate(Func<string, FieldContent> getFieldValue)
        {
            var field = getFieldValue(Lhs.Name);

            if (field == null || field.Value == null)
            {
                return false;
            }

            switch (Operator)
            {
                case ConditionOperator.Equals:
                    return field.Value.Equals(Rhs.Value);

                case ConditionOperator.NotEquals:
                    return !field.Value.Equals(Rhs.Value);

                default:
                    throw new Exception("Unexpected operator: " + Operator + ".");
            }
        }

        public override bool References(string field)
        {
            if (field == null)
            {
                return false;
            }

            return Lhs.EffectivePlaceholder.Name.ToLowerInvariant() == field.ToLowerInvariant();
        }
    }
}
