using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Expressions.Assignment;
using LuaAdv.Compiler.Nodes.Expressions.BasicTypes;
using LuaAdv.Compiler.Nodes.Math;
using LuaAdv.Compiler.Nodes.Statements;
using LuaAdv.Compiler.SemanticAnalyzer1;

namespace LuaAdv.Compiler.SemanticAnalyzer
{
    public class SemanticAnalyzer2 : TransparentVisitor
    {
        public SemanticAnalyzer2(Node mainNode) : base(mainNode)
        {
        }

        public override Node Visit(StatementLambdaFunctionDeclaration node)
        {
            var newNode = new StatementFunctionDeclaration(node.local, node.funcToken, node.name, node.parameterList, new Sequence(node.expression.Token, new Node[] { new Return(node.expression.Token, new[] { (Expression)node.expression }) }));
            newNode.Accept(this);
            return newNode;
        }

        public override Node Visit(StatementLambdaMethodDeclaration node)
        {
            var newNode = new StatementMethodDeclaration(node.funcToken, node.tableName, node.name, node.parameterList, new Sequence(node.expression.Token, new Node[] { new Return(node.expression.Token, new[] { (Expression)node.expression }) }));
            newNode.Accept(this);
            return newNode;
        }

        public override Node Visit(Class node)
        {
            var newFields = new List<Tuple<string, Expression>>();
            foreach (var field in node.fields)
                newFields.Add(new Tuple<string, Expression>(field.Item1, (Expression)field.Item2.Accept(this)));

            node.fields = newFields.ToArray();

            var newMethods = new List<Tuple<string, Tuple<Token, string, Expression>[], Sequence>>();
            foreach (var method in node.methods)
            {
                var newParams = new List<Tuple<Token, string, Expression>>();
                foreach (var param in method.Item2)
                    newParams.Add(new Tuple<Token, string, Expression>(param.Item1, param.Item2, param.Item3 != null ? (Expression)param.Item3.Accept(this) : null));

                newMethods.Add(new Tuple<string, Tuple<Token, string, Expression>[], Sequence>(method.Item1, newParams.ToArray(), (Sequence)method.Item3.Accept(this)));
            }

            node.methods = newMethods.ToArray();

            // TODO: Lower class methods to method declarations

            var sequenceNodes = new List<Node>();
            sequenceNodes.Add(node);

            Tuple<string, Tuple<Token, string, Expression>[], Sequence> constructorMethod = null;

            foreach (var method in newMethods)
            {
                if (method.Item1 == "this")
                {
                    constructorMethod = method;
                    continue;
                }

                var methodDeclaration =
                    new StatementMethodDeclaration(node.Token /* TODO: This should be a function token */,
                        new Variable(node.Token, $"C{node.name}"), method.Item1, method.Item2.ToList() /* TODO: This should be an array */, method.Item3);
                var methodNode = new ClassMethod(node, methodDeclaration);

                var scope = new Scope(CurrentScope);
                scope.FunctionName = method.Item1;

                sequenceNodes.Add(new ScopeNode(methodNode, scope));
            }

            // Construct a sequence of initializing variables

            List<Node> initializerNodes = new List<Node>();

            foreach (var field in node.fields)
            {
                var initializerNode = new StatementExpression(new ValueAssignmentOperator(new TableDotIndex(new This(null), null, field.Item1), null, field.Item2)); // TODO: Null tokens could be replaced with a special class.
                initializerNodes.Add(initializerNode);
            }

            if (constructorMethod == null)
            {
                initializerNodes.Insert(0, new StatementExpression(new SuperCall(null, new Expression[0])));
                constructorMethod = new Tuple<string, Tuple<Token, string, Expression>[], Sequence>("__this", new Tuple<Token, string, Expression>[0], new Sequence(null, initializerNodes.ToArray()));
            }
            else
            {
                var sequence = initializerNodes.Concat(constructorMethod.Item3.nodes);
                constructorMethod = new Tuple<string, Tuple<Token, string, Expression>[], Sequence>("__this", constructorMethod.Item2, new Sequence(null, sequence.ToArray()));
            } 

            var constructorDeclaration =
                new StatementMethodDeclaration(node.Token /* TODO: This should be a function token */,
                    new Variable(node.Token, $"C{node.name}"), constructorMethod.Item1, constructorMethod.Item2.ToList(), constructorMethod.Item3);
            var constructorNode = new ClassMethod(node, constructorDeclaration);

            var constructorScope = new Scope(CurrentScope);
            constructorScope.FunctionName = constructorMethod.Item1;

            sequenceNodes.Add(new ScopeNode(constructorNode, constructorScope));

            return new Sequence(null, sequenceNodes.ToArray());
        }
    }
}
