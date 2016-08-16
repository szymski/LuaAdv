using System;

namespace LuaAdv.Compiler.Nodes.Expressions.Comparison
{
    public class NotEquals : TwoSideOperator
    {
        public NotEquals(Expression left, Token token, Expression right) : base(left, token, right)
        {
        }

        public override string ReturnType => "bool";
    }
}
