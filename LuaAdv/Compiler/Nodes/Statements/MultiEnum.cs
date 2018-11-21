using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Statements
{
    public class MultiEnum : Statement
    {
        public override Token Token { get; }
        public string name;
        public Tuple<string, Node>[] values;

        public MultiEnum(Token token, string name, Tuple<string, Node>[] values)
        {
            Token = token;
            this.name = name;
            this.values = values;
        }
    }
}
