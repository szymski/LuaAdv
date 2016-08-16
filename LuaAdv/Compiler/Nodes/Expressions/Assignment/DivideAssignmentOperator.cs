using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions.Assignment
{
    public class DivideAssignmentOperator : AssignmentOperator
    {
        public DivideAssignmentOperator(Expression left, Token token, Expression right) : base(left, token, right)
        {
        }
    }
}
