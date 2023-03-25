using System.Linq;

namespace LuaAdv.Compiler.Nodes.Expressions;

public class InterpolatedString : Expression {
    public Expression[] values;

    public InterpolatedString(Token token, Expression[] valueExpressions)
    {
        this.Token = token;
        this.values = valueExpressions;
    }

    public override Token Token { get; }

    public override Node[] Children => values.Cast<Node>().ToArray();

    public override string ReturnType => "string";
}