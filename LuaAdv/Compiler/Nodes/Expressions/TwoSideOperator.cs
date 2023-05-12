using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public abstract class TwoSideOperator : Expression
    {
        public Node left;
        public Node right;

        public TwoSideOperator(Node left, Token token, Node right)
        {
            this.left = left;
            this.Token = token;
            this.right = right;
        }

        public override Token Token { get; }

        public override Node[] Children => new Node[] { left, right };

        public override string ReturnType => "?";

        public void Deconstruct(out Node left, out Node right)
        {
            left = this.left;
            right = this.right;
        }
    }
}
