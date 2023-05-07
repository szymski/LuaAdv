using LuaAdv.Compiler.SemanticAnalyzer1;

namespace LuaAdv.Compiler.Nodes
{
    public class ScopeNode : Node, IScoped
    {
        public Scope scope { get; set; }
        public Node node;

        public ScopeNode(Node node, Scope scope)
        {
            this.node = node;
            this.scope = scope;
        }

        public override Token Token => node.Token;

        public override Node[] Children => new[] { node };
    }
}
