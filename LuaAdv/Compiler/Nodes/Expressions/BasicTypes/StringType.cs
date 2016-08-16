using System;

namespace LuaAdv.Compiler.Nodes.Expressions.BasicTypes
{
    public class StringType : BasicType
    {
        public string value;       

        public StringType(Token token, string value)
        {
            this.Token = token;
            this.value = value;
        }

        public override Token Token { get; }
         
        public override string ReturnType => "string";
    }
}
