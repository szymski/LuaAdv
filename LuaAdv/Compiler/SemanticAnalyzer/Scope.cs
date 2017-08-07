using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.SemanticAnalyzer1
{
    public class Scope
    {
        public Scope Parent { get; }

        public List<FunctionInformation> Functions = new List<FunctionInformation>();

        private string _functionName = null;
        public string FunctionName
        {
            get { return _functionName ?? Parent?.FunctionName; }
            set { _functionName = value; }
        }

        private Dictionary<string, SingleEnum> _singleEnums = new Dictionary<string, SingleEnum>();

        public Scope(Scope parent = null)
        {
            Parent = parent;
        }

        public void AddEnum(SingleEnum node)
        {
            // TODO: Allow enum overwriting?
            if (_singleEnums.ContainsKey(node.name))
            {
                _singleEnums[node.name] = node;
                return;
            }

            _singleEnums.Add(node.name, node);
        }

        public SingleEnum LookupEnum(string name)
        {
            if (_singleEnums.ContainsKey(name))
                return _singleEnums[name];
            else
                return Parent?.LookupEnum(name);
        }

        public void JoinScope(Scope scope)
        {
            foreach (var pair in scope._singleEnums)
                _singleEnums[pair.Key] = pair.Value;
        }
    }
}
