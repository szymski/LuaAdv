using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.Nodes
{
    public class Break : Statement
    {
        public Break(Token token)
        {
            this.Token = token;
        }

        public override Token Token { get; }

        public override Node[] Children => new Node[] { };
    }
}
