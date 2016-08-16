using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions.Unary
{
    public class Negative : Unary
    {
        public Negative(Token token, Expression expression) : base(token, expression)
        {
        }
    }
}
