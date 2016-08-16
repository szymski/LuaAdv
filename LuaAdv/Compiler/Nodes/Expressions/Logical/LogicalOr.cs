using System;

namespace LuaAdv.Compiler.Nodes.Expressions.Logical
{
    public class LogicalOr : TwoSideOperator
    {
        public LogicalOr(Expression left, Token token, Expression right) : base(left, token, right)
        {
        }

        public override string ReturnType => "number";
    }
}
