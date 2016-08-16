using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public abstract class TwoSideOperator : Expression
    {
        public Expression left;
        public Expression right;

        public TwoSideOperator(Expression left, Token token, Expression right)
        {
            this.left = left;
            this.Token = token;
            this.right = right;
        }

        public override Token Token { get; }

        public override Node[] Children => new Node[] { left, right };

        public override string ReturnType => "?";
    }
}
