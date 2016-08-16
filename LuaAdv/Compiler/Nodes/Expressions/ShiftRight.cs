using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class ShiftRight : TwoSideOperator
    {
        public ShiftRight(Expression left, Token token, Expression right) : base(left, token, right)
        {
        }

        public override string ReturnType => "number";
    }
}
