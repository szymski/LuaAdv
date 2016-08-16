using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.Nodes
{
    public class For : Statement
    {
        public Token forToken;
        public Statement init;
        public Expression condition;
        public Statement after;
        public Sequence sequence;

        public For(Token forToken, Statement init, Expression condition, Statement after, Sequence sequence)
        {
            this.forToken = forToken;
            this.init = init;
            this.condition = condition;
            this.after = after;
            this.sequence = sequence;
        }

        public override Token Token => forToken;

        public override Node[] Children => new Node[] {init, condition, after, sequence};
    }
}
