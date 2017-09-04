using System;

namespace Portalworkers.DocxTemplating.Grammar
{
    public class BinaryCondition : AbstractCondition
    {
        public BinaryCondition(AbstractCondition lhs, BinaryOperator op, AbstractCondition rhs)
        {
            this.Lhs = lhs;
            this.Operator = op;
            this.Rhs = rhs;
        }

        #region Properties

        public AbstractCondition Lhs { get; set; }

        public BinaryOperator Operator { get; set; }

        public AbstractCondition Rhs{ get; set; }

        #endregion

        public override bool Evaluate(Func<string, FieldContent> getFieldValue)
        {
            var left = Lhs.Evaluate(getFieldValue);
            var right = Rhs.Evaluate(getFieldValue);

            switch (Operator)
            {
                case BinaryOperator.LogicalAnd:
                    return left && right;

                case BinaryOperator.LogicalOr:
                    return left || right;

                default:
                    throw new Exception("Unexpected operator: " + Operator + ".");
            }
        }

        public override bool References(string field)
        {
            return Lhs.References(field) || Rhs.References(field);
        }
    }
}
