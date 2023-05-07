using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.SemanticAnalyzer1;

namespace LuaAdv.Compiler.Nodes {
    public class ScopeExpression : Expression, IScoped {
        public Scope scope { get; set; }
        public Expression expression;

        public ScopeExpression(Expression expression, Scope scope)
        {
            this.expression = expression;
            this.scope = scope;
        }

        public override Token Token => expression.Token;

        public override Node[] Children => new[] { expression };
        public override string ReturnType => expression.ReturnType;
    }
}