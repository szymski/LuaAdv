using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.Nodes
{
    public class While : Statement
    {
        public Token whileToken;
        public Expression condition;
        public Sequence sequence;

        public While(Token whileToken, Expression condition, Sequence sequence)
        {
            this.whileToken = whileToken;
            this.condition = condition;
            this.sequence = sequence;
        }

        public override Token Token => whileToken;

        public override Node[] Children => new Node[] {condition, sequence};
    }
}
