using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.SemanticAnalyzer1;

namespace LuaAdv.Compiler.Nodes
{
    public class ScopeNode : Node
    {
        public Scope scope;
        public Node node;

        public ScopeNode(Node node, Scope scope)
        {
            this.node = node;
            this.scope = scope;
        }

        public override Token Token { get; }
    }
}
