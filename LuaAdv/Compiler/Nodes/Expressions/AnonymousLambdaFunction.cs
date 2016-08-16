using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class AnonymousLambdaFunction : Expression
    {
        public Token funcToken;
        public Token nameToken;
        public string name;
        public List<Tuple<Token, string, Expression>> parameterList;
        public Expression expression;

        public AnonymousLambdaFunction(Token funcToken, List<Tuple<Token, string, Expression>> parameterList, Expression expression)
        {
            this.funcToken = funcToken;
            this.parameterList = parameterList;
            this.expression = expression;
        }

        public override Token Token => funcToken;

        public override Node[] Children => new Node[] { expression };

        public override string ReturnType => "?";
    }
}
