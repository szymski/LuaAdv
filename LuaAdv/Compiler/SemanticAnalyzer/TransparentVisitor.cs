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
    public class TransparentVisitor : IAstVisitor
    {
        public virtual Node MainNode { get; protected set; }
        public Scope MainScope { get; protected set; }
        public Scope CurrentScope { get; protected set; }

        public TransparentVisitor(Node mainNode)
        {
            MainNode = mainNode;
        }

        public virtual Node Visit(Node node)
        {
            throw new NotImplementedException(node.GetType().Name + " analysis not implemented!");
        }

        public virtual Node Visit(Statement node)
        {
            throw new NotImplementedException(node.GetType().Name + " analysis not implemented!");
        }

        #region Scopes

        public virtual Node Visit(ScopeNode node)
        {
            var oldScope = CurrentScope;
            CurrentScope = node.scope;
            node.node.Accept(this);
            CurrentScope = oldScope;

            return node;
        }

        public ScopeNode PushScope(Node innerNode)
        {
            return new ScopeNode(innerNode, new Scope(CurrentScope));
        }

        #endregion

        public virtual Node Visit(For node)
        {
            node.init = (Statement)node.init.Accept(this);
            node.condition = (Expression)node.condition.Accept(this);
            node.after = (Statement)node.after.Accept(this);
            node.sequence = (Sequence)node.sequence.Accept(this);

            return node;
        }

        public virtual Node Visit(Foreach node)
        {
            node.table = (Expression)node.table.Accept(this);
            node.sequence = (Sequence)node.sequence.Accept(this);

            return node;
        }

        public virtual Node Visit(While node)
        {
            node.condition = (Expression)node.condition.Accept(this);
            node.sequence = (Sequence)node.sequence.Accept(this);

            return node;
        }

        public virtual Node Visit(Break node)
        {
            return node;
        }

        public virtual Node Visit(Continue node)
        {
            return node;
        }

        public virtual Node Visit(Sequence node)
        {
            for (int i = 0; i < node.nodes.Length; i++)
                node.nodes[i] = node.nodes[i].Accept(this);

            return node;
        }

        public virtual Node Visit(If node)
        {
            for (int i = 0; i < node.ifs.Count; i++)
            {
                var ifChild = node.ifs[i];

                node.ifs[i] = new Tuple<Token, Expression, Sequence>(ifChild.Item1, ifChild.Item2?.Accept(this) as Expression, (Sequence)ifChild.Item3.Accept(this));
            }

            return node;
        }

        public virtual Node Visit(Return node)
        {
            for (var index = 0; index < node.values.Length; index++)
                node.values[index] = (Expression)node.values[index].Accept(this);

            return node;
        }

        public virtual Node Visit(StatementFunctionDeclaration node)
        {
            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            node.sequence = node.sequence.Accept(this);

            return node;
        }

        public virtual Node Visit(StatementLambdaFunctionDeclaration node)
        {
            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            node.expression = node.expression.Accept(this);

            // TODO: Do lowering to function declaration

            return node;
        }

        public virtual Node Visit(StatementLambdaMethodDeclaration node)
        {
            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            node.expression = node.expression.Accept(this);

            // TODO: Do lowering to method declaration

            return node;
        }

        public virtual Node Visit(StatementMethodDeclaration node)
        {
            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            node.sequence = node.sequence.Accept(this);

            return node;
        }

        public virtual Node Visit(GlobalVariablesDeclaration node)
        {
            for (int i = 0; i < node.values.Length; i++)
                node.values[i] = (Expression)node.values[i].Accept(this);

            return node;
        }

        public virtual Node Visit(LocalVariablesDeclaration node)
        {
            for (var i = 0; i < node.values.Length; i++)
                node.values[i] = (Expression)node.values[i].Accept(this);

            return node;
        }

        public virtual Node Visit(StatementExpression node)
        {
            node.expression = (Expression)node.expression.Accept(this);

            return node;
        }

        public virtual Node Visit(NullStatement node)
        {
            return node;
        }

        public virtual Node Visit(Expression node)
        {
            return node;
        }

        public virtual Node Visit(ValueAssignmentOperator node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(AddAssignmentOperator node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(SubtractAssignmentOperator node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(MultiplyAssignmentOperator node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(DivideAssignmentOperator node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(ModuloAssignmentOperator node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(ConcatAssignmentOperator node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(Bool node)
        {
            return node;
        }

        public virtual Node Visit(Number node)
        {
            return node;
        }

        public virtual Node Visit(StringType node)
        {
            return node;
        }

        public virtual Node Visit(Equals node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(NotEquals node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(Greater node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(Less node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(GreaterOrEqual node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(LessOrEqual node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(Is node)
        {
            node.expression = (Expression)node.expression.Accept(this);

            return node;
        }

        public virtual Node Visit(ConditionalAnd node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(ConditionalOr node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(LogicalAnd node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(LogicalOr node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(LogicalXor node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(Add node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(Subtract node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(Multiply node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(Divide node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(Modulo node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(Power node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(Concat node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(PostDecrement node)
        {
            node.expression = (Expression)node.expression.Accept(this);

            return node;
        }

        public virtual Node Visit(PostIncrement node)
        {
            node.expression = (Expression)node.expression.Accept(this);

            return node;
        }

        public virtual Node Visit(PreDecrement node)
        {
            node.expression = (Expression)node.expression.Accept(this);

            return node;
        }

        public virtual Node Visit(PreIncrement node)
        {
            node.expression = (Expression)node.expression.Accept(this);

            return node;
        }

        public virtual Node Visit(Negation node)
        {
            node.expression = (Expression)node.expression.Accept(this);

            return node;
        }

        public virtual Node Visit(Negative node)
        {
            node.expression = (Expression)node.expression.Accept(this);

            return node;
        }

        public virtual Node Visit(Not node)
        {
            node.expression = (Expression)node.expression.Accept(this);

            return node;
        }

        public virtual Node Visit(AnonymousFunction node)
        {
            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            node.sequence = node.sequence.Accept(this);

            return node;
        }

        public virtual Node Visit(AnonymousLambdaFunction node)
        {
            for (int i = 0; i < node.parameterList.Count; i++)
                if (node.parameterList[i].Item3 != null)
                    node.parameterList[i] = new Tuple<Token, string, Expression>(node.parameterList[i].Item1, node.parameterList[i].Item2, (Expression)node.parameterList[i].Item3.Accept(this));

            node.expression = (Expression)node.expression.Accept(this);

            return node;
        }

        public virtual Node Visit(FunctionCall node)
        {
            node.function = (Expression)node.function.Accept(this);

            for (var i = 0; i < node.parameters.Length; i++)
                node.parameters[i] = (Expression)node.parameters[i].Accept(this);

            return node;
        }

        public virtual Node Visit(GroupedEquation node)
        {
            node.expression = (Expression)node.expression.Accept(this);

            return node;
        }

        public virtual Node Visit(MethodCall node)
        {
            node.methodTable = (Expression)node.methodTable.Accept(this);

            for (var i = 0; i < node.parameters.Length; i++)
                node.parameters[i] = (Expression)node.parameters[i].Accept(this);

            return node;
        }

        public virtual Node Visit(SuperCall node)
        {
            for (var i = 0; i < node.parameters.Length; i++)
                node.parameters[i] = (Expression)node.parameters[i].Accept(this);

            return node;
        }

        public virtual Node Visit(NullPropagation node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(ShiftLeft node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(ShiftRight node)
        {
            node.left = (Expression)node.left.Accept(this);
            node.right = (Expression)node.right.Accept(this);

            return node;
        }

        public virtual Node Visit(Table node)
        {
            for (int index = 0; index < node.values.Length; index++)
            {
                var key = node.values[index];
                node.values[index] = new Tuple<Expression, Expression>((Expression)key.Item1.Accept(this), key.Item2?.Accept(this) as Expression);
            }

            return node;
        }

        public virtual Node Visit(TableDotIndex node)
        {
            node.table = (Expression)node.table.Accept(this);

            return node;
        }

        public virtual Node Visit(TableIndex node)
        {
            node.table = (Expression)node.table.Accept(this);
            node.key = (Expression)node.key.Accept(this);

            return node;
        }

        public virtual Node Visit(Ternary node)
        {
            node.conditionExpression = (Expression)node.conditionExpression.Accept(this);
            node.expression1 = (Expression)node.expression1.Accept(this);
            node.expression2 = (Expression)node.expression2.Accept(this);

            return node;
        }

        public virtual Node Visit(Variable node)
        {
            return node;
        }

        public virtual Node Visit(This node)
        {
            return node;
        }

        public virtual Node Visit(Vararg node)
        {
            return node;
        }

        public virtual Node Visit(Null node)
        {
            return node;
        }

        public virtual Node Visit(CommentNode node)
        {
            return node;
        }

        public virtual Node Visit(DocumentationCommentNode node)
        {
            return node;
        }

        public virtual Node Visit(TableLength node)
        {
            node.table.Accept(this);
            return node;
        }

        public virtual Node Visit(Class node)
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

            return node;
        }

        public virtual Node Visit(ClassMethod node)
        {
            node.method = (StatementMethodDeclaration)node.method.Accept(this); // TODO: Possibility to replace method may be neccesary.

            return node;
        }
    }
}
