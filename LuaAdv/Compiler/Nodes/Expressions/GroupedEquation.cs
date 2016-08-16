using System;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class GroupedEquation : Expression
    {
        public Expression expression;

        public GroupedEquation(Token startToken, Expression expression)
        {
            this.Token = startToken;
            this.expression = expression;
        }

        public override Token Token { get; }

        public override string ReturnType => expression.ReturnType;

        public override Node[] Children => new Node[] { expression };
    }
}
