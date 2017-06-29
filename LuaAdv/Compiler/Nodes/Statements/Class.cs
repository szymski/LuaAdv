using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;

namespace LuaAdv.Compiler.Nodes.Statements
{
    public class Class : Statement
    {
        public bool local;
        public string name;
        public string baseClass;
        public Tuple<string, Tuple<Token, string, Expression>[], Sequence>[] methods;
        public Tuple<string, Expression>[] fields;

        public override Token Token { get; }

        public Class(Token classToken, bool local, string name, string baseClass, Tuple<string, Tuple<Token, string, Expression>[], Sequence>[] methods, Tuple<string, Expression>[] fields)
        {
            Token = classToken;
            this.local = local;
            this.name = name;
            this.baseClass = baseClass;
            this.methods = methods;
            this.fields = fields;
        }
    }
}
