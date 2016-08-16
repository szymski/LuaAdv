using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;

namespace LuaAdv.Compiler.Nodes.Statements
{
    public class StatementExpression : Statement
    {
        public Expression expression;

        public StatementExpression(Expression expression)
        {
            this.expression = expression;
        }

        public override Token Token => expression.Token;

        public override Node[] Children => new Node[] { expression };
    }
}
