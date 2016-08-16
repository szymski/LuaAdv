using System;

namespace LuaAdv.Compiler.Nodes.Expressions.Logical
{
    public class LogicalAnd : TwoSideOperator
    {
        public LogicalAnd(Expression left, Token token, Expression right) : base(left, token, right)
        {
        }

        public override string ReturnType => "number";
    }
}
