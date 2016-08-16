using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions.Assignment
{
    public class ModuloAssignmentOperator : AssignmentOperator
    {
        public ModuloAssignmentOperator(Expression left, Token token, Expression right) : base(left, token, right)
        {
        }

    }
}
