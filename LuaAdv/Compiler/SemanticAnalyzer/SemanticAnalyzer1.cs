using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Expressions.Assignment;
using LuaAdv.Compiler.Nodes.Expressions.BasicTypes;
using LuaAdv.Compiler.Nodes.Expressions.Comparison;
using LuaAdv.Compiler.Nodes.Expressions.Conditional;
using LuaAdv.Compiler.Nodes.Expressions.Logical;
using LuaAdv.Compiler.Nodes.Expressions.Unary;
using LuaAdv.Compiler.Nodes.Expressions.Unary.Post;
using LuaAdv.Compiler.Nodes.Expressions.Unary.Pre;
using LuaAdv.Compiler.Nodes.Math;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.SemanticAnalyzer1
{
    /// <summary>
    /// Analyzes basic AST information and creates scopes, like:
    ///     - Function declarations
    ///     - Function documentation
    ///     - TODO enums
    ///     - TODO templates
    /// </summary>
    public class SemanticAnalyzer1 : TransparentVisitor
    {
        public SemanticAnalyzer1(Node mainNode, Scope[] toJoinScopes = null) : base(mainNode)
        {
            var scopeNode = new ScopeNode(mainNode, new Scope());
            MainNode = scopeNode;
            MainScope = scopeNode.scope;
            CurrentScope = MainScope;

            if (toJoinScopes != null)
                foreach (var scope in toJoinScopes)
                    CurrentScope.JoinScope(scope);
        }

        public override Node Visit(For node)
        {
            node.init = (Statement)node.init.Accept(this);
            node.condition = (Expression)node.condition.Accept(this);
            node.after = (Statement)node.after.Accept(this);
            node.sequence = (Sequence)node.sequence.Accept(this);

            return node;
        }

        public override Node Visit(Foreach node)
        {
            node.table = (Expression)node.table.Accept(this);
            node.sequence = (Sequence)node.sequence.Accept(this);

            return node;
        }

        public override Node Visit(While node)
        {
            node.condition = (Expression)node.condition.Accept(this);
            node.sequence = (Sequence)node.sequence.Accept(this);

            return node;
        }

        public override Node Visit(Sequence node)
        {
            for (int i = 0; i < node.nodes.Length; i++)
            {
                var newNode = node.nodes[i].Accept(this); // TODO: Throws ArrayTypeMismatch sometimes. Temporarily fixed by modifying Sequence class.
                node.nodes[i] = newNode;
            }

            return node;
        }

        public override Node Visit(If node)
        {
            for (int i = 0; i < node.ifs.Count; i++)
            {
                var ifChild = node.ifs[i];

                node.ifs[i] = new Tuple<Token, Expression, Sequence>(ifChild.Item1, ifChild.Item2?.Accept(this) as Expression, (Sequence)ifChild.Item3.Accept(this));
            }

            return node;
        }

        public override Node Visit(Return node)
        {
            for (var index = 0; index < node.values.Length; index++)
                node.values[index] = (Expression)node.values[index].Accept(this);

            return node;
        }

        public override Node Visit(StatementFunctionDeclaration node)
        {
            var scope = new Scope(CurrentScope);
            var previousScope = CurrentScope;
            CurrentScope = scope;

            CurrentScope.RawFunctionName = NamedVariablesToRawStringName(node.name);
            CurrentScope.FunctionName = NamedVariablesToStringName(node.name);

            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            var information = new FunctionInformation()
            {
                Line = node.funcToken.Line,
                Character = node.funcToken.Character,
                IsMethod = false,
                Name = CurrentScope.FunctionName,
                ParameterList = node.parameterList.Select(p => new Tuple<string, string>(p.Item2, p.Item3?.Token.Value ?? "")).ToList(),
                ReturnType = "" // TODO: Return type analysis
            };

            previousScope.Functions.Add(information);

            node.sequence = node.sequence.Accept(this);

            CurrentScope = previousScope;

            return new ScopeNode(node, scope);
        }

        // TODO: Move somewhere else.
        private string NamedVariablesToStringName(Expression node)
        {
            if (node is Variable)
                return (node as Variable).name;
            else if (node is TableDotIndex)
                return $"{NamedVariablesToStringName((node as TableDotIndex).table)}.{(node as TableDotIndex).index}";
            else
                return "INVALID";
        }

        // TODO: Move somewhere else.
        private string NamedVariablesToRawStringName(Expression node)
        {
            if (node is Variable)
                return (node as Variable).name;
            else
                return "INVALID";
        }

        public override Node Visit(StatementLambdaFunctionDeclaration node)
        {
            var scope = new Scope(CurrentScope);
            var previousScope = CurrentScope;
            CurrentScope = scope;

            CurrentScope.RawFunctionName = NamedVariablesToRawStringName(node.name);
            CurrentScope.FunctionName = NamedVariablesToStringName(node.name);

            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            var information = new FunctionInformation()
            {
                Line = node.funcToken.Line,
                Character = node.funcToken.Character,
                IsMethod = false,
                Name = CurrentScope.FunctionName,
                ParameterList = node.parameterList.Select(p => new Tuple<string, string>(p.Item2, p.Item3?.Token.Value ?? "")).ToList(),
                ReturnType = "" // TODO: Return type analysis
            };

            previousScope.Functions.Add(information);

            node.expression = node.expression.Accept(this);

            CurrentScope = previousScope;

            return new ScopeNode(node, scope);
        }

        public override Node Visit(StatementLambdaMethodDeclaration node)
        {
            var scope = new Scope(CurrentScope);
            var previousScope = CurrentScope;
            CurrentScope = scope;

            CurrentScope.RawFunctionName = node.name;
            CurrentScope.FunctionName = $"{NamedVariablesToStringName(node.tableName)}:{node.name}";

            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            var information = new FunctionInformation()
            {
                Line = node.funcToken.Line,
                Character = node.funcToken.Character,
                Name = CurrentScope.FunctionName,
                IsMethod = true,
                ParameterList = node.parameterList.Select(p => new Tuple<string, string>(p.Item2, p.Item3?.Token.Value ?? "")).ToList(),
                ReturnType = "" // TODO: Return type analysis
            };

            previousScope.Functions.Add(information);

            node.expression = node.expression.Accept(this);

            CurrentScope = previousScope;

            return new ScopeNode(node, scope);
        }

        public override Node Visit(StatementMethodDeclaration node)
        {
            var scope = new Scope(CurrentScope);
            var previousScope = CurrentScope;
            CurrentScope = scope;

            CurrentScope.RawFunctionName = node.name;
            CurrentScope.FunctionName = $"{NamedVariablesToStringName(node.tableName)}:{node.name}";

            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            var information = new FunctionInformation()
            {
                Line = node.funcToken.Line,
                Character = node.funcToken.Character,
                Name = CurrentScope.FunctionName,
                IsMethod = true,
                ParameterList = node.parameterList.Select(p => new Tuple<string, string>(p.Item2, p.Item3?.Token.Value ?? "")).ToList(),
                ReturnType = "" // TODO: Return type analysis
            };

            previousScope.Functions.Add(information);

            node.sequence = node.sequence.Accept(this);

            CurrentScope = previousScope;

            return new ScopeNode(node, scope);
        }
        public virtual Node Visit(AnonymousFunction node)
        {
            var scope = new Scope(CurrentScope);
            var previousScope = CurrentScope;
            CurrentScope = scope;

            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            node.sequence = node.sequence.Accept(this);

            CurrentScope = previousScope;

            return new ScopeNode(node, scope);
        }

        public override Node Visit(AnonymousLambdaFunction node)
        {
            var scope = new Scope(CurrentScope);
            scope.FunctionName = $"{CurrentScope.FunctionName ?? "global"}_anonymous@{node.Token.Line}";
            scope.RawFunctionName = $"{CurrentScope.RawFunctionName ?? "global"}_anonymous@{node.Token.Line}";
            var previousScope = CurrentScope;
            CurrentScope = scope;

            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            node.expression = (Expression)node.expression.Accept(this);

            CurrentScope = previousScope;

            return new ScopeExpression(node, scope);
        }

        public override Node Visit(SingleEnum node)
        {
            node.value = node.value.Accept(this);
            CurrentScope.AddSingleEnum(node);
            return node;
        }

        public override Node Visit(MultiEnum node)
        {
            for (int i = 0; i < node.values.Length; i++)
                node.values[i] = new Tuple<string, Node>(node.values[i].Item1, node.values[i].Item2.Accept(this));

            CurrentScope.AddMultiEnum(node);

            return node;
        }
    }
}
