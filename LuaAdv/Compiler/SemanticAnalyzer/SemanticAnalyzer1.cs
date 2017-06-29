using System;
using System.Collections.Generic;
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
        public SemanticAnalyzer1(Node mainNode) : base(mainNode)
        {
            var scopeNode = new ScopeNode(mainNode, new Scope());
            MainNode = scopeNode;
            MainScope = scopeNode.scope;
            CurrentScope = MainScope;
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
                node.nodes[i] = node.nodes[i].Accept(this);

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
            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            var information = new FunctionInformation()
            {
                Line = node.funcToken.Line,
                Character = node.funcToken.Character,
                Name = node.name.Token.Value,
                ParameterList = node.parameterList.Select(p => new Tuple<string, string>(p.Item2, p.Item3?.Token.Value ?? "")).ToList(),
                ReturnType = "" // TODO: Return type analysis
            };

            CurrentScope.Functions.Add(information);

            node.sequence = PushScope(node.sequence).Accept(this);

            return node;
        }

        public override Node Visit(StatementLambdaFunctionDeclaration node)
        {
            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            node.expression = PushScope(node.expression).Accept(this);

            return node;
        }

        public override Node Visit(StatementLambdaMethodDeclaration node)
        {
            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            node.expression = PushScope(node.expression).Accept(this);

            return node;
        }

        public override Node Visit(StatementMethodDeclaration node)
        {
            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            node.sequence = PushScope(node.sequence).Accept(this);

            return node;
        }

        public override Node Visit(GlobalVariablesDeclaration node)
        {
            for (int i = 0; i < node.values.Length; i++)
                node.values[i] = (Expression)node.values[i].Accept(this);

            return node;
        }

        public override Node Visit(LocalVariablesDeclaration node)
        {
            for (var i = 0; i < node.values.Length; i++)
                node.values[i] = (Expression)node.values[i].Accept(this);

            return node;
        }

        public override Node Visit(AnonymousFunction node)
        {
            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            node.sequence = (Expression)node.sequence.Accept(this);

            return node;
        }

        public override Node Visit(AnonymousLambdaFunction node)
        {
            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            node.expression = (Expression)node.expression.Accept(this);

            return node;
        }
    }
}
