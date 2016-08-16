using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions.Unary
{
    public class Negation : Unary
    {
        public Negation(Token token, Expression expression) : base(token, expression)
        {
        }
    }
}
