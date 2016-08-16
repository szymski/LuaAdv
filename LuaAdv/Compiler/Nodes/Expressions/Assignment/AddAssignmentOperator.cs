using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions.Assignment
{
    public class AddAssignmentOperator : AssignmentOperator
    {
        public AddAssignmentOperator(Expression left, Token token, Expression right) : base(left, token, right)
        {
        }
    }
}
