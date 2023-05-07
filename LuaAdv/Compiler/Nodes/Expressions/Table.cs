using System;
using System.Linq;

namespace LuaAdv.Compiler.Nodes.Expressions {
    public class Table : Expression {
        public string name;
        public Tuple<Expression, Expression>[] values;

        public Table(Token token, Tuple<Expression, Expression>[] values)
        {
            this.Token = token;
            this.values = values;
        }

        public override Token Token { get; }

        public override Node[] Children => values.SelectMany(x => new Node[] { x.Item1, x.Item2 }).ToArray();

        public override string ReturnType => "table";
    }
}