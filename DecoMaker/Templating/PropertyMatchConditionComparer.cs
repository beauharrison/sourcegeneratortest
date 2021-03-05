using System.Collections.Generic;

namespace DecoMaker.Templating
{
    internal class PropertyMatchConditionComparer : IComparer<PropertyTemplateMatchCondition>
    {
        public int Compare(PropertyTemplateMatchCondition x, PropertyTemplateMatchCondition y)
        {
            if (x.PropertyTypeRule != PropertyTypeRule.Any && y.PropertyTypeRule == PropertyTypeRule.Any)
                return 1;

            if (x.PropertyTypeRule == PropertyTypeRule.Any && y.PropertyTypeRule != PropertyTypeRule.Any)
                return -1;

            return 0;
        }
    }
}
