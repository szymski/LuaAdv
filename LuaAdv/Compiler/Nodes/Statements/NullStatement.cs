using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Statements
{
    public class NullStatement : Statement
    {
        public NullStatement(Token token)
        {
            this.Token = token;
        }

        public override Token Token { get; }
    }
}
