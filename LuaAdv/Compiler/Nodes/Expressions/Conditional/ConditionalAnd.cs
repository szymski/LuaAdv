using System;

namespace LuaAdv.Compiler.Nodes.Expressions.Conditional
{
    public class ConditionalAnd : TwoSideOperator
    {
        public ConditionalAnd(Expression left, Token token, Expression right) : base(left, token, right)
        {
        }

        public override string ReturnType => "bool";
    }
}
