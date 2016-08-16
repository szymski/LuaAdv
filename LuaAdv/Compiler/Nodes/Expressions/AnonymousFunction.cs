using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class AnonymousFunction : Expression
    {
        public Token funcToken;
        public Token nameToken;
        public string name;
        public List<Tuple<Token, string, Expression>> parameterList;
        public Node sequence;

        public AnonymousFunction(Token funcToken, List<Tuple<Token, string, Expression>> parameterList, Node sequence)
        {
            this.funcToken = funcToken;
            this.parameterList = parameterList;
            this.sequence = sequence;
        }

        public override Token Token => funcToken;

        public override Node[] Children => new Node[] { sequence };

        public override string ReturnType => "?";
    }
}
