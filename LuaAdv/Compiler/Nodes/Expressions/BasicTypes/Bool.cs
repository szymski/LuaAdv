using System;

namespace LuaAdv.Compiler.Nodes.Expressions.BasicTypes
{
    public class Bool : BasicType
    {
        public bool value;       

        public Bool(Token token, bool value)
        {
            this.Token = token;
            this.value = value;
        }

        public override Token Token { get; }
         
        public override string ReturnType => "bool";
    }
}
