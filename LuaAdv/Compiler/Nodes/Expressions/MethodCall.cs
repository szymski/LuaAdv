using System;
using System.Linq;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class MethodCall : NamedVariable, IStatementable // TODO: This isn't a named variable, rewrite.
    {
        public Expression methodTable;
        public Token nameToken;
        public string name;
        public Expression[] parameters;

        public MethodCall(Expression method, Token nameToken, string name, Expression[] parameters)
        {
            this.methodTable = method;
            this.nameToken = nameToken;
            this.name = name;
            this.parameters = parameters;
        }

        public override Token Token => nameToken;

        public override Node[] Children => (new Node[] { methodTable }).Concat(parameters).ToArray();

        public override string ReturnType => "?";
    }
}
