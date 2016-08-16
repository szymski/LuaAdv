using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.Nodes
{
    public class StatementLambdaMethodDeclaration : Statement
    {
        public Token funcToken;
        public NamedVariable tableName;
        public string name;
        public List<Tuple<Token, string, Expression>> parameterList;
        public Node expression;

        public StatementLambdaMethodDeclaration(Token funcToken, NamedVariable tableName, string name,
            List<Tuple<Token, string, Expression>> parameterList, Expression expression)
        {
            this.funcToken = funcToken;
            this.tableName = tableName;
            this.name = name;
            this.parameterList = parameterList;
            this.expression = expression;
        }

        public override Token Token => funcToken;

        public override Node[] Children => new Node[] { expression };
    }
}
