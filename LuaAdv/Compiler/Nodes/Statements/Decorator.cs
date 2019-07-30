using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;

namespace LuaAdv.Compiler.Nodes.Statements
{
    public class Decorator : Statement
    {
        public Token token;
        public Node decoratedNode;
        public Node function;
        public Node[] parameters;

        public Decorator(Token token, Node decoratedNode, Expression function, Node[] parameters)
        {
            this.token = token;
            this.decoratedNode = decoratedNode;
            this.function = function;
            this.parameters = parameters;
        }

        public override Token Token => token;

        public override Node[] Children => (new Node[] { function, decoratedNode }).Concat(parameters).ToArray();
    }
}
