using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions.Unary
{
    public class Unary : Expression
    {
        public Expression expression;

        public Unary(Token token, Expression expression)
        {
            this.Token = token;
            this.expression = expression;
        }

        public override Token Token { get; }

        public override Node[] Children => new Node[] { expression };

        public override string ReturnType => "?";
    }
}
