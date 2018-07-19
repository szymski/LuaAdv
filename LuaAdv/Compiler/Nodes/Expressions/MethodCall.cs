using System;
using System.Linq;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class MethodCall : NamedVariable, IStatementable // TODO: This isn't a named variable, rewrite.
    {
        public Node methodTable;
        public Token nameToken;
        public string name;
        public Node[] parameters;

        public MethodCall(Node method, Token nameToken, string name, Node[] parameters)
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
