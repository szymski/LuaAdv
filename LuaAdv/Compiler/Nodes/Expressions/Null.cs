using System;
using LuaAdv.Compiler.Nodes.Expressions.BasicTypes;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class Null : BasicType
    {
        public Null(Token token)
        {
            this.Token = token;
        }

        public override Token Token { get; }
         
        public override string ReturnType => "null";
    }
}
