using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class NullPropagation : TwoSideOperator
    {
        public NullPropagation(Expression left, Token token, Expression right) : base(left, token, right)
        {
        }

        public override string ReturnType => "?";
    }
}
