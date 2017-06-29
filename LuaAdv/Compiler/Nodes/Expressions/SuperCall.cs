using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class SuperCall : Expression, IStatementable
    {
        public Expression[] parameters;

        public SuperCall(Token token, Expression[] parameters)
        {
            Token = token;
            this.parameters = parameters;
        }

        public override Token Token { get; }

        public override Node[] Children => parameters;

        public override string ReturnType => "?";
    }
}
