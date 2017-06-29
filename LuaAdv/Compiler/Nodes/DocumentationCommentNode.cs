using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.Nodes
{
    public class DocumentationCommentNode : Statement
    {
        public override Token Token { get; }

        public DocumentationCommentNode(Token token)
        {
            Token = token;
        }
    }
}
