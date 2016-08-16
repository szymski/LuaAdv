using System;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public class Variable : NamedVariable
    {
        public string name;       

        public Variable(Token token, string name)
        {
            this.Token = token;
            this.name = name;
        }

        public override Token Token { get; }
         
        public override string ReturnType => "?";
    }
}
