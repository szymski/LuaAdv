using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.SemanticAnalyzer1
{
    public class FunctionInformation
    {
        public int Line { get; }
        public int Character { get; }
        public string Name { get; }
        public List<Tuple<string, string>> ParameterList { get; }
        public string ReturnType { get; }
    }
}
