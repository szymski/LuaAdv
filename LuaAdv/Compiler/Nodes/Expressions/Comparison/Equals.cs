using System;

namespace LuaAdv.Compiler.Nodes.Expressions.Comparison
{
    public class Equals : TwoSideOperator
    {
        public Equals(Expression left, Token token, Expression right) : base(left, token, right)
        {
        }

        public override string ReturnType => "bool";
    }
}
