using System;
using System.Linq;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class FunctionCall : Expression, IStatementable
    {
        public Expression function;
        public Expression[] parameters;

        public FunctionCall(Expression function, Expression[] parameters)
        {
            this.function = function;
            this.parameters = parameters;
        }

        public override Token Token => function.Token;

        public override Node[] Children => (new Node[] { function }).Concat(parameters).ToArray();

        public override string ReturnType => "?";
    }
}
