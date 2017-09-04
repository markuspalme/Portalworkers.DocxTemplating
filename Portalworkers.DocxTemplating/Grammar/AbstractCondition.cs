using System;

namespace Portalworkers.DocxTemplating.Grammar
{
    public abstract class AbstractCondition
    {
        public abstract bool References(string field);

        public abstract bool Evaluate(Func<string, FieldContent> getFieldValue);
    }
}