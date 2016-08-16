using System;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class Vararg : NamedVariable
    {
        public Vararg(Token token)
        {
            this.Token = token;
        }

        public override Token Token { get; }
         
        public override string ReturnType => "...";
    }
}
