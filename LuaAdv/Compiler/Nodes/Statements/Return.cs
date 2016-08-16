using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.Nodes
{
    public class Return : Statement
    {
        public Expression[] values;

        public Return(Token token, Expression[] values)
        {
            this.Token = token;
            this.values = values;
        }

        public override Token Token { get; }

        public override Node[] Children => values;
    }
}
