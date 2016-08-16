using System;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class This : NamedVariable
    {
        public This(Token token)
        {
            this.Token = token;
        }

        public override Token Token { get; }
         
        public override string ReturnType => "?";
    }
}
