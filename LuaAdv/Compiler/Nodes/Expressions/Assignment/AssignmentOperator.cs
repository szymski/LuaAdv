using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions.Assignment
{
    public abstract class AssignmentOperator : TwoSideOperator, IStatementable
    {
        public AssignmentOperator(Expression left, Token token, Expression right) : base(left, token, right)
        {
            this.left = left;
            this.Token = token;
            this.right = right;
        }

        public override Token Token { get; }
    }
}
