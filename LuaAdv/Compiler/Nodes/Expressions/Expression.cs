using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Expressions
{
    public abstract class Expression : Node
    {
        public abstract string ReturnType { get; }
    }
}
