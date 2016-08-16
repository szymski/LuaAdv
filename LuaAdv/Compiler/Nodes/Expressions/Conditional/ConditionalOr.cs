using System;

namespace LuaAdv.Compiler.Nodes.Expressions.Conditional
{
    public class ConditionalOr : TwoSideOperator
    {
        public ConditionalOr(Expression left, Token token, Expression right) : base(left, token, right)
        {
        }

        public override string ReturnType => "bool";
    }
}
