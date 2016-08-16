using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.Nodes
{
    public class Foreach : Statement
    {
        public Token foreachToken;
        public string keyName;
        public string varName;
        public Expression table;
        public Sequence sequence;

        public Foreach(Token foreachToken, string keyName, string varName, Expression table, Sequence sequence)
        {
            this.foreachToken = foreachToken;
            this.keyName = keyName;
            this.varName = varName;
            this.table = table;
            this.sequence = sequence;
        }

        public override Token Token => foreachToken;

        public override Node[] Children => new Node[] {table, sequence};
    }
}
