using System;

namespace LuaAdv.Compiler.Nodes.Expressions.BasicTypes
{
    public class TableDotIndex : NamedVariable
    {
        public Expression table;
        public string index;

        public TableDotIndex(Expression table, Token indexToken, string index)
        {
            this.table = table;
            this.Token = indexToken;
            this.index = index;
        }

        public override Token Token { get; }

        public override Node[] Children => new Node[] { table };

        public override string ReturnType => "?";
    }
}
