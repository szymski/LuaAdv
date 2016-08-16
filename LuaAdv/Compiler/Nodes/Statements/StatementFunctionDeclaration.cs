using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.Nodes
{
    public class StatementFunctionDeclaration : Statement
    {
        public bool local;
        public Token funcToken;
        public NamedVariable name;
        public List<Tuple<Token, string, Expression>> parameterList;
        public Node sequence;

        public StatementFunctionDeclaration(bool local, Token funcToken, NamedVariable name,
            List<Tuple<Token, string, Expression>> parameterList, Node sequence)
        {
            this.local = local;
            this.funcToken = funcToken;
            this.name = name;
            this.parameterList = parameterList;
            this.sequence = sequence;
        }

        public override Token Token => funcToken;

        public override Node[] Children => new Node[] { sequence };
    }
}
