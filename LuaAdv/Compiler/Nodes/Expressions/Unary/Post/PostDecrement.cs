using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions.Unary.Post
{
    public class PostDecrement : Unary, IStatementable
    {
        public PostDecrement(Token token, Expression expression) : base(token, expression)
        {
        }
    }
}
