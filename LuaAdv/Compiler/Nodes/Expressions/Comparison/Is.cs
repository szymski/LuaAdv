using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions.Comparison
{
    public class Is : Expression
    {
        public Expression expression;
        public string type;

        public Is(Expression expression, Token token, string type)
        {
            this.expression = expression;
            this.Token = token;
            this.type = type;
        }

        public override Token Token { get; }

        public override Node[] Children => new Node[] { expression };

        public override string ReturnType => "bool";
    }
}
