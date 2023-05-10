using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    /// <summary>
    /// Lowered to AnonymousFunction in SemanticAnalyzer2.
    /// </summary>
    public class AnonymousLambdaFunction : Expression, ILowered
    {
        public Token funcToken;
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
