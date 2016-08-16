using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;

namespace LuaAdv.Compiler.Nodes.Math
{
    public class Multiply : TwoSideOperator
    {
        public Multiply(Expression left, Token token, Expression right) : base(left, token, right)
        {
        }

        public override string ReturnType => "?";
    }
}
