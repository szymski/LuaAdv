using System;

namespace LuaAdv.Compiler.Nodes.Expressions.BasicTypes
{
    public class Number : BasicType
    {
        public double value;       

        public Number(Token token, double value)
        {
            this.Token = token;
            this.value = value;
        }

        public override Token Token { get; }
         
        public override string ReturnType => "number";
    }
}
