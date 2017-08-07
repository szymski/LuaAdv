using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.Nodes
{
    public class Sequence : Statement
    {
        public Token startToken;
        public Node[] nodes;

        public Sequence(Token startToken, Node[] nodes)
        {
            this.startToken = startToken;
            this.nodes = nodes.Cast<Node>().ToArray(); // TODO: Temporary fix for ArrayTypeMismatch in SemanticAnalyzer1.Visit(Sequence node)
        }

        public override Token Token => startToken ?? nodes[0].Token;

        public override Node[] Children => nodes;
    }
}
