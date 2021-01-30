using System;
using System.Collections.Generic;
using System.Text;

namespace Generators.Sql
{
    public interface ISqlComponent
    {
    }

    public class SqlWhere : ISqlComponent
    {
        public SqlWhere(string field, WhereOp op, object value)
        {
            Field = field;
            Op = op;
            Value = value;
        }

        public string Field { get; }

        public WhereOp Op { get; }

        public object Value { get; }
    }

    public enum WhereOp
    {
        Equal,
        NotEqual
    }

    public static class SqlHelper
    {
        public static string ToSymbol(this WhereOp op)
        {
            return op switch
            {
                WhereOp.Equal => "=",
                WhereOp.NotEqual => "<>",
                _ => throw new NotImplementedException()
            };
        }
    }
}
