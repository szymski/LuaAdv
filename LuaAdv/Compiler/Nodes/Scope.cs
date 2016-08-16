using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes
{
    class Scope : Node
    {
        public Node node;

        public Scope(Node node)
        {
            this.node = node;
        }

        public override Token Token { get; }
    }
}
