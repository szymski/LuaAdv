using System;
using System.Linq;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class Ternary : Expression
    {
        public Expression conditionExpression;
        public Expression expression1;
        public Expression expression2;

        public Ternary(Expression conditionExpression, Expression expression1, Expression expression2)
        {
            this.conditionExpression = conditionExpression;
            this.expression1 = expression1;
            this.expression2 = expression2;
        }

        public override Token Token => conditionExpression.Token;

        public override Node[] Children => new Node[] {conditionExpression, expression1, expression2};

        public override string ReturnType => "?";
    }
}
