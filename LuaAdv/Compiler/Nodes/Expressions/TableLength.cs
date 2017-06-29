using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class TableLength : Expression
    {
        public Expression table;

        public override Token Token { get; }
        public override string ReturnType => "number";

        public TableLength(Token token, Expression table)
        {
            Token = token;
            this.table = table;
        }

        public override Node[] Children => new Node[] { table };
    }
}
