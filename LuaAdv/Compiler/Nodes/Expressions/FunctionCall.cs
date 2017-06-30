using System;
using System.Linq;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class FunctionCall : Expression, IStatementable
    {
        public Expression function;
        public Node[] parameters;

        public FunctionCall(Expression function, Node[] parameters)
        {
            this.function = function;
            this.parameters = parameters;
        }

        public override Token Token => function.Token;

        public override Node[] Children => (new Node[] { function }).Concat(parameters).ToArray();

        public override string ReturnType => "?";
    }
}
