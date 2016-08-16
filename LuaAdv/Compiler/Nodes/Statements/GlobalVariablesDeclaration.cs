using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.Nodes
{
    public class GlobalVariablesDeclaration : Statement
    {
        public NamedVariable[] variables;
        public Expression[] values;

        public GlobalVariablesDeclaration(NamedVariable[] variables, Expression[] values)
        {
            this.variables = variables;
            this.values = values;
        }

        public override Token Token => variables[0].Token;

        public override Node[] Children => values ?? new Node[] { };
    }
}
