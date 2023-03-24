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
    /// <summary>
    /// Does lowering (replaces complex nodes with simpler ones) and validates correctness.
    /// </summary>
    public class SemanticAnalyzer2 : TransparentVisitor
    {
        public string FileName { get; set; } = "";

        public SemanticAnalyzer2(Node mainNode) : base(mainNode)
        {
        }

        public override Node Visit(StatementLambdaFunctionDeclaration node)
        {
            Node newNode = new StatementFunctionDeclaration(node.local, node.funcToken, node.name, node.parameterList, new Sequence(node.expression.Token, new Node[] { new Return(node.expression.Token, new[] { (Expression)node.expression }) }));
            newNode = newNode.Accept(this);
            return newNode;
        }

        public override Node Visit(StatementLambdaMethodDeclaration node)
        {
            Node newNode = new StatementMethodDeclaration(node.funcToken, node.tableName, node.name, node.parameterList, new Sequence(node.expression.Token, new Node[] { new Return(node.expression.Token, new[] { (Expression)node.expression }) }));
            newNode = newNode.Accept(this);
            return newNode;
        }

        public override Node Visit(AnonymousLambdaFunction node)
        {
            Node newNode = new AnonymousFunction(node.funcToken, node.parameterList, new Sequence(null, new Node[] { new Return(null, new[] { node.expression }) }));
            newNode = newNode.Accept(this);
            return newNode;
        }

        public override Node Visit(Class node)
        {
            var newFields = new List<Tuple<string, Expression, TokenDocumentationComment>>();
            foreach (var field in node.fields)
                newFields.Add(new Tuple<string, Expression, TokenDocumentationComment>(field.Item1, (Expression)field.Item2?.Accept(this), field.Item3));

            node.fields = newFields.ToArray();

            var newMethods = new List<Tuple<string, Tuple<Token, string, Expression>[], Node, TokenDocumentationComment>>();
            foreach (var method in node.methods)
            {
                var newParams = new List<Tuple<Token, string, Expression>>();
                foreach (var param in method.Item2)
                    newParams.Add(new Tuple<Token, string, Expression>(param.Item1, param.Item2, param.Item3 != null ? (Expression)param.Item3.Accept(this) : null));

                var prevScope = CurrentScope;
                var scope = new Scope(CurrentScope);
                CurrentScope = scope;
                CurrentScope.RawFunctionName = method.Item1;
                CurrentScope.FunctionName = $"{node.name}:{method.Item1}";

                var newMethod = method.Item3.Accept(this);

                CurrentScope = prevScope;

                newMethods.Add(new Tuple<string, Tuple<Token, string, Expression>[], Node, TokenDocumentationComment>(method.Item1, newParams.ToArray(), new ScopeNode(newMethod, scope), method.Item4));

                // TODO: Scopes for class methods should created be here, before visiting nodes.
            }

            var sequenceNodes = new List<Node>();
            sequenceNodes.Add(node);

            Tuple<string, Tuple<Token, string, Expression>[], Node, TokenDocumentationComment> constructorMethod = null;

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

                sequenceNodes.Add(methodNode);
            }

            // Construct a sequence of initializing variables

            List<Node> initializerNodes = new List<Node>();

            foreach (var field in node.fields.Where(f => f.Item2 != null))
            {
                var initializerNode = new StatementExpression(new ValueAssignmentOperator(new TableDotIndex(new This(null), null, field.Item1), null, field.Item2)); // TODO: Null tokens could be replaced with a special class.
                initializerNodes.Add(initializerNode);
            }

            if (constructorMethod == null)
            {
                if (node.baseClass != null)
                    initializerNodes.Insert(0, new StatementExpression(new SuperCall(null, new Expression[0])));
                constructorMethod = new Tuple<string, Tuple<Token, string, Expression>[], Node, TokenDocumentationComment>("__this", new Tuple<Token, string, Expression>[0], new Sequence(null, initializerNodes.ToArray()), null); // TODO: Create scope for implicit constructor
            }
            else
            {
                var sequence = initializerNodes.Concat(((constructorMethod.Item3 as ScopeNode).node as Sequence).nodes);
                constructorMethod = new Tuple<string, Tuple<Token, string, Expression>[], Node, TokenDocumentationComment>("__this", constructorMethod.Item2, new Sequence(null, sequence.ToArray()), constructorMethod.Item4);
            }

            var constructorDeclaration =
                new StatementMethodDeclaration(node.Token /* TODO: This should be a function token */,
                    new Variable(node.Token, $"C{node.name}"), constructorMethod.Item1, constructorMethod.Item2.ToList(), constructorMethod.Item3);
            var constructorNode = new ClassMethod(node, constructorDeclaration);

            var constructorScope = new Scope(CurrentScope);
            constructorScope.RawFunctionName = constructorMethod.Item1;
            constructorScope.FunctionName = constructorMethod.Item1;

            sequenceNodes.Add(new ScopeNode(constructorNode, constructorScope));

            return new Sequence(null, sequenceNodes.ToArray());
        }

        public override Node Visit(SpecialNode node)
        {
            switch (node.value)
            {
                case "__LINE__":
                    return new Number(node.Token, node.Token.Line + 1);
                case "__FUNCTION__":
                    return new StringType(node.Token, CurrentScope.FunctionName ?? "");
                case "__TIME__":
                    return new StringType(node.Token, DateTime.Now.ToString("T"));
                case "__DATE__":
                    return new StringType(node.Token, DateTime.Now.ToString("d"));
                case "__LONGDATE__":
                    return new StringType(node.Token, DateTime.Now.ToString("D"));
                case "__DATETIME__":
                    return new StringType(node.Token, DateTime.Now.ToString("G"));
                case "__LONGDATETIME__":
                    return new StringType(node.Token, DateTime.Now.ToString("R"));
                case "__FILE__":
                    return new StringType(node.Token, this.FileName);
                default:
                    throw new Exception($"'{node.value}' special token analysis not implemented.");
            }
        }

        public override Node Visit(Variable node)
        {
            var enumNode = CurrentScope.LookupSingleEnum(node.name);
            if (enumNode != null)
                return enumNode.value.Accept(this);

            return node;
        }

        public override Node Visit(TableDotIndex node)
        {
            node.table = (Expression)node.table.Accept(this);

            if (node.table is Variable variable)
            {
                var name = variable.name;

                var enumNode = CurrentScope.LookupMultiEnum(name);
                if (enumNode != null)
                {
                    var key = node.index;
                    var value = enumNode.values.FirstOrDefault(v => v.Item1 == key);
                    if (value == null)
                        throw new CompilerException($"There is no such key '{key}' in the enum '{name}'.", node.Token);

                    return value.Item2;
                }
            }

            return node;
        }

        public override Node Visit(SingleEnum node)
        {
            return new NullStatement(node.Token);
        }

        public override Node Visit(MultiEnum node)
        {
            return new NullStatement(node.Token);
        }

        public override Node Visit(StaticIf node)
        {
            for (int i = 0; i < node.ifs.Count; i++)
            {
                var ifChild = node.ifs[i];

                // Else
                if (i == node.ifs.Count - 1 && ifChild.Item2 == null)
                    return ifChild.Item3.Accept(this);

                var condition = ifChild.Item2.Accept(this) as Expression;

                bool isTrue = false;

                if (condition is Bool)
                    isTrue = ((Bool)condition).value;
                else if (condition is Number)
                    isTrue = ((Number)condition).value > 0;
                else if (condition is Variable == false)
                    throw new CompilerException("Only basic types are supported in static if statements.", ifChild.Item2.Token);

                if (isTrue)
                    return ifChild.Item3.Accept(this);
            }

            return new NullStatement(null);
        }

        public override Node Visit(Add node)
        {
            node.left = node.left.Accept(this);
            node.right = node.right.Accept(this);

            if (node.left is Number left && node.right is Number right)
                return new Number(node.Token, left.value + right.value);

            return node;
        }

        public override Node Visit(Subtract node)
        {
            node.left = node.left.Accept(this);
            node.right = node.right.Accept(this);

            if (node.left is Number left && node.right is Number right)
                return new Number(node.Token, left.value - right.value);

            return node;
        }

        public override Node Visit(Multiply node)
        {
            node.left = node.left.Accept(this);
            node.right = node.right.Accept(this);

            if (node.left is Number left && node.right is Number right)
                return new Number(node.Token, left.value * right.value);

            return node;
        }

        public override Node Visit(Divide node)
        {
            node.left = node.left.Accept(this);
            node.right = node.right.Accept(this);

            if (node.left is Number left && node.right is Number right)
                return new Number(node.Token, left.value / right.value);

            return node;
        }

        public override Node Visit(GroupedEquation node)
        {
            node.expression = node.expression.Accept(this);

            if (node.expression is Number n)
                return new Number(node.Token, n.value);

            return node;
        }

        public override Node Visit(Decorator node)
        {
            node.function = node.function.Accept(this);

            for (int i = 0; i < node.parameters.Length; i++)
                node.parameters[i] = node.parameters[i].Accept(this);

            if (node.decoratedNode is Class cl)
            {
                var seq = (Sequence)node.decoratedNode.Accept(this);

                var classVar = new Variable(node.token, $"C{cl.name}");

                var decoratorConstructorCall = new FunctionCall((Expression)node.function, node.parameters);
                var decoratorCall = new FunctionCall(decoratorConstructorCall, new Node[] { classVar });
                var newNode = new StatementExpression(new ValueAssignmentOperator(classVar, node.token, decoratorCall));

                return new DecoratedClass(cl, seq.nodes, new Node[] { newNode });
            }

            node.decoratedNode = node.decoratedNode.Accept(this);

            if (node.decoratedNode is DecoratedClass decoratedClass)
            {
                var classVar = new Variable(node.token, $"C{decoratedClass.@class.name}");

                var decoratorConstructorCall = new FunctionCall((Expression)node.function, node.parameters);
                var decoratorCall = new FunctionCall(decoratorConstructorCall, new Node[] { classVar });
                var newNode = new StatementExpression(new ValueAssignmentOperator(classVar, node.token, decoratorCall));

                return new DecoratedClass(decoratedClass.@class, decoratedClass.classSequence, new Node[] { newNode }.Concat(decoratedClass.decoratorSequence).ToArray());
            }

            Node innerNode = node.decoratedNode;
            if (innerNode is ScopeNode scope)
                innerNode = scope.node;

            if (innerNode is StatementFunctionDeclaration func)
            {
                var decoratorConstructorCall = new FunctionCall((Expression)node.function, node.parameters);
                var anonymousFunc = new AnonymousFunction(func.Token, func.parameterList, func.sequence);
                var decoratorCall = new FunctionCall(decoratorConstructorCall, new Node[] { anonymousFunc });
                Node newNode;
                if (func.local)
                    newNode = new LocalVariablesDeclaration(new [] {new Tuple<Token, string>(func.name.Token, ((Variable)func.name).name) }, new []{ decoratorCall });
                else
                    newNode = new StatementExpression(new ValueAssignmentOperator(func.name, node.token, decoratorCall));

                return newNode;
            }
            else if (innerNode is StatementExpression stmtExpr && stmtExpr.expression is ValueAssignmentOperator assignOp)
            {
                var decoratorConstructorCall = new FunctionCall((Expression)node.function, node.parameters);
                var decoratorCall = new FunctionCall(decoratorConstructorCall, new Node[] { assignOp.right });
                var newNode = new StatementExpression(new ValueAssignmentOperator((Expression)assignOp.left, assignOp.Token, decoratorCall));

                return newNode;
            }

            else if (innerNode is StatementMethodDeclaration method)
            {
                var decoratorConstructorCall = new FunctionCall((Expression)node.function, node.parameters);
                var anonymousFunc = new AnonymousFunction(method.Token, (new[] { new Tuple<Token, string, Expression>(null, "self", null) }).Concat(method.parameterList).ToList(), method.sequence);
                var decoratorCall = new FunctionCall(decoratorConstructorCall, new Node[] { anonymousFunc });
                var newNode = new StatementExpression(new ValueAssignmentOperator(new TableDotIndex(method.tableName, null, method.name), node.token, decoratorCall));

                return newNode;
            }

            return node.decoratedNode;
        }

        public override Node Visit(DecoratedClass node)
        {
            return new Sequence(null, node.classSequence);
        }
    }
}
