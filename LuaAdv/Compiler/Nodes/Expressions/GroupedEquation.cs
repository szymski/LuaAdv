using System;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class GroupedEquation : Expression
    {
        public Node expression;

        public GroupedEquation(Token startToken, Node expression)
        {
            this.Token = startToken;
            this.expression = expression;
        }

        public override Token Token { get; }

        public override string ReturnType => "?";

        public override Node[] Children => new Node[] { expression };
    }
}
