using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.Nodes
{
    public class LocalVariablesDeclaration : Statement
    {
        public Tuple<Token, string>[] variables;
        public Node[] values;

        public LocalVariablesDeclaration(Tuple<Token, string>[] variables, Node[] values)
        {
            this.variables = variables;
            this.values = values;
        }

        public override Token Token => variables[0].Item1;

        public override Node[] Children => values ?? new Node[] { };
    }
}
