using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions.Unary.Pre
{
    public class PreDecrement : Unary, IStatementable
    {
        public PreDecrement(Token token, Expression expression) : base(token, expression)
        {
        }
    }
}
