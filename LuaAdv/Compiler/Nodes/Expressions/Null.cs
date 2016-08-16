using System;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class Null : Expression
    {
        public Null(Token token)
        {
            this.Token = token;
        }

        public override Token Token { get; }
         
        public override string ReturnType => "null";
    }
}
