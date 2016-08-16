using System;
using System.Collections.Generic;
using LuaAdv.Compiler.Nodes.Expressions;

namespace LuaAdv.Compiler.Nodes.Statements
{
    public class StatementLambdaFunctionDeclaration : Statement
    {
        public bool local;
        public Token funcToken;
        public Token nameToken;
        public NamedVariable name;
        public List<Tuple<Token, string, Expression>> parameterList;
        public Expression expression;

        public StatementLambdaFunctionDeclaration(bool local, Token funcToken, NamedVariable name,
            List<Tuple<Token, string, Expression>> parameterList, Expression expression)
        {
            this.local = local;
            this.funcToken = funcToken;
            this.name = name;
            this.parameterList = parameterList;
            this.expression = expression;
        }

        public override Token Token => funcToken;

        public override Node[] Children => new Node[] { expression };
    }
}
