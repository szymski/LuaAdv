using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Statements
{
    public class SingleEnum : Statement
    {
        public override Token Token { get; }
        public string name;
        public Node value;

        public SingleEnum(Token token, string name, Node value)
        {
            Token = token;
            this.name = name;
            this.value = value;
        }
    }
}
