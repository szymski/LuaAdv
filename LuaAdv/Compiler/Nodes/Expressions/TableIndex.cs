using System;

namespace LuaAdv.Compiler.Nodes.Expressions.BasicTypes
{
    public class TableIndex : NamedVariable
    {
        public Expression table;
        public Expression key;

        public TableIndex(Expression table, Expression index)
        {
            this.table = table;
            this.key = index;
        }

        public override Token Token { get; }

        public override Node[] Children => new Node[] { table, key };

        public override string ReturnType => "?";
    }
}
