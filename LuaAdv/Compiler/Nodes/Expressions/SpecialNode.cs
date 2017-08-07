using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class SpecialNode : Expression
    {
        public override Token Token { get; }
        public string value;

        public SpecialNode(Token token, string value)
        {
            Token = token;
            this.value = value;
        }

        public override string ReturnType => "?";
    }
}
