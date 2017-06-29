using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.Nodes
{
    public class CommentNode : Statement
    {
        public override Token Token { get; }

        public CommentNode(Token token)
        {
            Token = token;
        }
    }
}
