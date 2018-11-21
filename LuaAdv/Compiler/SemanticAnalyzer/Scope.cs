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

        private string _rawFunctionName = null;

        /// <summary>
        /// Just the function/method name without table/metatable.
        /// </summary>
        public string RawFunctionName
        {
            get { return _rawFunctionName ?? Parent?.RawFunctionName; }
            set { _rawFunctionName = value; }
        }

        private Dictionary<string, SingleEnum> _singleEnums = new Dictionary<string, SingleEnum>();
        private Dictionary<string, MultiEnum> _multiEnums = new Dictionary<string, MultiEnum>();

        public Scope(Scope parent = null)
        {
            Parent = parent;
        }

        public void AddSingleEnum(SingleEnum node)
        {
            // TODO: Allow enum overwriting?
            if (_singleEnums.ContainsKey(node.name))
            {
                _singleEnums[node.name] = node;
                return;
            }

            _singleEnums.Add(node.name, node);
        }

        public void AddMultiEnum(MultiEnum node)
        {
            // TODO: Allow enum overwriting?
            if (_multiEnums.ContainsKey(node.name))
            {
                _multiEnums[node.name] = node;
                return;
            }

            _multiEnums.Add(node.name, node);
        }

        public SingleEnum LookupSingleEnum(string name)
        {
            if (_singleEnums.ContainsKey(name))
                return _singleEnums[name];
            else
                return Parent?.LookupSingleEnum(name);
        }

        public MultiEnum LookupMultiEnum(string name)
        {
            if (_multiEnums.ContainsKey(name))
                return _multiEnums[name];
            else
                return Parent?.LookupMultiEnum(name);
        }

        public void JoinScope(Scope scope)
        {
            foreach (var pair in scope._singleEnums)
                _singleEnums[pair.Key] = pair.Value;
        }
    }
}
