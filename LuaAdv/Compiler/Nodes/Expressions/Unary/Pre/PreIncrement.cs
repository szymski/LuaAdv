using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions.Unary.Pre
{
    public class PreIncrement : Unary, IStatementable
    {
        public PreIncrement(Token token, Expression expression) : base(token, expression)
        {
        }
    }
}
