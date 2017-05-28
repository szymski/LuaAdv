using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.SemanticAnalyzer1
{
    public class FunctionInformation
    {
        public int Line { get; set; }
        public int Character { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Item 1 - parameter name
        /// Item 2 - default value
        /// </summary>
        public List<Tuple<string, string>> ParameterList { get; set; } = new List<Tuple<string, string>>();
        public string ReturnType { get; set; } = "";
    }
}
