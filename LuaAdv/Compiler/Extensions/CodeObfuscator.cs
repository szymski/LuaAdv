using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Expressions.BasicTypes;
using LuaAdv.Compiler.Nodes.Expressions.Comparison;
using LuaAdv.Compiler.Nodes.Expressions.Logical;
using LuaAdv.Compiler.Nodes.Expressions.Unary;
using LuaAdv.Compiler.SemanticAnalyzer1;

namespace LuaAdv.Compiler.Extensions
{
    public class ObfuscatorScope
    {
        private static int _lastVarId = 0;

        public ObfuscatorScope Parent { get; set; }
        public Scope Scope { get; set; }

        private Dictionary<string, string> _nameMap = new Dictionary<string, string>();
        private Dictionary<string, Node> _inlined = new Dictionary<string, Node>();

        public string Rename(string name, bool create = false, int depth = 0)
        {
            if (_nameMap.ContainsKey(name))
                return _nameMap[name];
            else
            {
                var newName = Parent?.Rename(name, false, depth + 1);

                if (newName == null)
                {
                    if (!create && depth > 0)
                        return null;
                    else if (!create && depth == 0)
                        return name;
                    else
                    {
                        newName = $"_var{++_lastVarId}";
                        _nameMap.Add(name, newName);
                        return newName;
                    }
                }
                else
                    return newName;
            }
        }

        public Node GetInlined(string name)
        {
            if (_inlined.ContainsKey(name))
                return _inlined[name];
            else
                return Parent?.GetInlined(name);
        }

        public bool CanBeInlined(Node node)
        {
            if (node == null)
                return false;

            return (node is BasicType ||
                node is TwoSideOperator ||
                node is Not ||
                node is Negative ||
                node is Negation) &&
                (node.Children?.All(CanBeInlined) ?? true);
        }

        public void MakeInlined(string name, Node node)
        {
            if (_inlined.ContainsKey(name))
                _inlined.Remove(name);

            _inlined.Add(name, node);
        }
    }

    public class CodeObfuscator : TransparentVisitor
    {
        public ObfuscatorScope CurrentObfuscatorScope { get; protected set; }

        private Dictionary<string, string> _nameMap = new Dictionary<string, string>();

        private Sequence _firstSequenceNode = null;
        private Table _stringTableNode = null;
        private Dictionary<string, int> _stringMap = new Dictionary<string, int>();
        private int _lastStrId = 0;

        private Random _random = new Random();

        public CodeObfuscator(Node mainNode) : base(mainNode)
        {
        }

        public override Node Visit(Sequence node)
        {
            if (_firstSequenceNode == null)
            {
                _firstSequenceNode = node;

                List<Node> toAdd = new List<Node>();
                var strTable = new Table(new TokenIdentifier(), new Tuple<Expression, Expression>[0]);
                _stringTableNode = strTable;

                var strTableDeclaration = new LocalVariablesDeclaration(new Tuple<Token, string>[]
                {
                    new Tuple<Token, string>(new TokenIdentifier(), "_strTable"),
                }, new Node[]
                {
                    strTable,
                });
                toAdd.Add(strTableDeclaration);

                for (int i = 0; i < node.nodes.Length; i++)
                    node.nodes[i] = node.nodes[i].Accept(this);

                node.nodes = toAdd.Concat(node.nodes).ToArray();
            }
            else
                for (int i = 0; i < node.nodes.Length; i++)
                    node.nodes[i] = node.nodes[i].Accept(this);

            return node;
        }

        public override Node Visit(ScopeNode node)
        {
            var oldScope = CurrentScope;
            var oldObfuscatorScope = CurrentObfuscatorScope;
            CurrentScope = node.scope;
            CurrentObfuscatorScope = new ObfuscatorScope()
            {
                Parent = oldObfuscatorScope,
                Scope = CurrentScope,
            };
            node.node = node.node.Accept(this);
            CurrentScope = oldScope;
            CurrentObfuscatorScope = oldObfuscatorScope;

            return node;
        }

        public override Node Visit(LocalVariablesDeclaration node)
        {
            for (int i = 0; i < node.variables.Length; i++)
            {
                if (i < node.values.Length && node.values[i] != null && CurrentObfuscatorScope.CanBeInlined(node.values[i]))
                    CurrentObfuscatorScope.MakeInlined(node.variables[i].Item2, node.values[i]);

                node.variables[i] = new Tuple<Token, string>(node.variables[i].Item1, MapName(node.variables[i].Item2, true));
            }

            for (int i = 0; i < node.values.Length; i++)
                node.values[i] = node.values[i].Accept(this);

            return node;
        }

        public override Node Visit(Variable node)
        {
            //var inlined = CurrentObfuscatorScope.GetInlined(node.name);
            //if (inlined != null)
            //    return inlined;

            node.name = MapName(node.name);

            return node;
        }

        public override Node Visit(StatementFunctionDeclaration node)
        {
            if (node.local && node.name is Variable)
            {
                var variable = (Variable)node.name;
                variable.name = MapName(variable.name, true);
            }

            node.name = (NamedVariable)node.name.Accept(this);

            for (int i = 0; i < node.parameterList.Count; i++)
                node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, MapName(node.parameterList[i].Item2, true), node.parameterList[i].Item3 != null ? (Expression)node.parameterList[i].Item3.Accept(this) : null);

            node.sequence = node.sequence.Accept(this);

            return node;
        }

        public override Node Visit(StatementMethodDeclaration node)
        {
            for (int i = 0; i < node.parameterList.Count; i++)
                node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, MapName(node.parameterList[i].Item2, true), node.parameterList[i].Item3 != null ? (Expression)node.parameterList[i].Item3.Accept(this) : null);

            node.sequence = node.sequence.Accept(this);

            return node;
        }

        public override Node Visit(Foreach node)
        {
            if (node.keyName != null)
                node.keyName = MapName(node.keyName, true);

            node.varName = MapName(node.varName, true);

            node.table = (Expression)node.table.Accept(this);
            node.sequence = (Sequence)node.sequence.Accept(this);

            return node;
        }

        public override Node Visit(TableDotIndex node)
        {
            node.table = (Expression)node.table.Accept(this);

            return node;
        }

        public override Node Visit(GlobalVariablesDeclaration node)
        {
            for (int i = 0; i < node.variables.Length; i++)
            {
                node.variables[i] = (NamedVariable)node.variables[i].Accept(this);
            }

            for (int i = 0; i < node.values.Length; i++)
                node.values[i] = node.values[i].Accept(this);

            return node;
        }

        public override Node Visit(StringType node)
        {
            return MapString(node.value);
        }

        public override Node Visit(Null node)
        {
            return GenerateInvalidVariable();
        }

        public override Node Visit(Bool node)
        {
            if(node.value)
                return new Not(new TokenIdentifier(), GenerateInvalidVariable());
            else
                return new GroupedEquation(new TokenIdentifier(), new Equals(GenerateInvalidVariable(), new TokenIdentifier(), new Bool(new TokenIdentifier(), true)));
        }

        private string MapName(string name, bool create = false)
        {
            return CurrentObfuscatorScope.Rename(name, create);
        }

        private Node MapString(string value)
        {
            if (!_stringMap.ContainsKey(value))
            {
                var id = ++_lastStrId;

                _stringMap.Add(value, id);
                _stringTableNode.values = _stringTableNode.values.Concat(new Tuple<Expression, Expression>[]
                {
                    new Tuple<Expression, Expression>(null, new StringType(new TokenIdentifier(), value)), 
                }).ToArray();
            }

            return new TableIndex(new Variable(new TokenIdentifier(), "_strTable"), new Number(new TokenIdentifier(), _stringMap[value]));
        }

        private Expression GenerateInvalidVariable()
        {
            return new Variable(new TokenIdentifier(), "_var" + _random.Next(10000, 99999));
        }
    }
}
