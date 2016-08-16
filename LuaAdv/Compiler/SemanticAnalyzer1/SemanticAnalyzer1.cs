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
    public class SemanticAnalyzer1 : IAstVisitor
    {
        public Node MainNode { get; }

        public List<FunctionInformation> Functions { get; } = new List<FunctionInformation>();

        public SemanticAnalyzer1(Node mainNode)
        {
            MainNode = mainNode;
            mainNode.Accept(this);
        }

        public Node Visit(Node node)
        {
            throw new NotImplementedException(node.GetType().Name + " analysis not implemented!");
        }

        public Node Visit(Statement node)
        {
            throw new NotImplementedException(node.GetType().Name + " analysis not implemented!");
        }

        public Node Visit(For node)
        {
            node.init = (Statement)node.init.Accept(this);
            node.condition = (Expression)node.condition.Accept(this);
            node.after = (Statement)node.after.Accept(this);
            node.sequence = (Sequence)node.sequence.Accept(this);

            return node;
        }

        public Node Visit(Foreach node)
        {
            node.table = (Expression)node.table.Accept(this);
            node.sequence = (Sequence)node.sequence.Accept(this);

            return node;
        }

        public Node Visit(While node)
        {
            node.condition = (Expression)node.condition.Accept(this);
            node.sequence = (Sequence)node.sequence.Accept(this);

            return node;
        }

        public Node Visit(Break node)
        {
            return node;
        }

        public Node Visit(Continue node)
        {
            return node;
        }

        public Node Visit(Sequence node)
        {
            for (int i = 0; i < node.nodes.Length; i++)
                node.nodes[i] = node.nodes[i].Accept(this);

            return node;
        }

        public Node Visit(If node)
        {
            foreach (var ifChild in node.ifs)
            {
                ifChild.Item2? = ifChild.Item2?.Accept(this);
                ifChild.Item3 = ifChild.Item3.Accept(this);
            }

            return node;
        }

        public Node Visit(Return node)
        {
            foreach (var value in node.values)
                value.Accept(this);

            return node;
        }

        public Node Visit(StatementFunctionDeclaration node)
        {
            foreach (var defExp in node.parameterList.Select(d => d.Item3).Where(d => d != null))
                defExp.Accept(this);

            node.sequence = node.sequence.Accept(this);

            return node;
        }

        public Node Visit(StatementLambdaFunctionDeclaration node)
        {
            foreach (var defExp in node.parameterList.Select(d => d.Item3).Where(d => d != null))
                defExp.Accept(this);

            node.expression = (Expression)node.expression.Accept(this);

            return node;
        }

        public Node Visit(StatementLambdaMethodDeclaration node)
        {
            foreach (var defExp in node.parameterList.Select(d => d.Item3).Where(d => d != null))
                defExp.Accept(this);

            node.expression = node.expression.Accept(this);

            return node;
        }

        public Node Visit(StatementMethodDeclaration node)
        {
            foreach (var defExp in node.parameterList.Select(d => d.Item3).Where(d => d != null))
                defExp.Accept(this);

            node.sequence = node.sequence.Accept(this);

            return node;
        }

        public Node Visit(GlobalVariablesDeclaration node)
        {
            foreach (var value in node.values)
                value.Accept(this);

            return node;
        }

        public Node Visit(LocalVariablesDeclaration node)
        {
            foreach (var value in node.values)
                value.Accept(this);

            return node;
        }

        public Node Visit(StatementExpression node)
        {
            node.expression = (Expression)node.expression.Accept(this);

            return node;
        }

        public Node Visit(NullStatement node)
        {
            return node;
        }

        public Node Visit(Expression node)
        {
            return node;
        }

        public Node Visit(ValueAssignmentOperator node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(AddAssignmentOperator node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(SubtractAssignmentOperator node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(MultiplyAssignmentOperator node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(DivideAssignmentOperator node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(ModuloAssignmentOperator node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(ConcatAssignmentOperator node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(Bool node)
        {
            return node;
        }

        public Node Visit(Number node)
        {
            return node;
        }

        public Node Visit(StringType node)
        {
            return node;
        }

        public Node Visit(Equals node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(NotEquals node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(Greater node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(Less node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(GreaterOrEqual node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(LessOrEqual node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(Is node)
        {
            node.expression= (Expression)node.expression.Accept(this);

            return node;
        }

        public Node Visit(ConditionalAnd node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(ConditionalOr node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(LogicalAnd node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(LogicalOr node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(LogicalXor node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(Add node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(Subtract node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(Multiply node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(Divide node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(Modulo node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(Power node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(Concat node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(PostDecrement node)
        {
            node.expression= (Expression)node.expression.Accept(this);

            return node;
        }

        public Node Visit(PostIncrement node)
        {
            node.expression= (Expression)node.expression.Accept(this);

            return node;
        }

        public Node Visit(PreDecrement node)
        {
            node.expression= (Expression)node.expression.Accept(this);

            return node;
        }

        public Node Visit(PreIncrement node)
        {
            node.expression= (Expression)node.expression.Accept(this);

            return node;
        }

        public Node Visit(Negation node)
        {
            node.expression= (Expression)node.expression.Accept(this);

            return node;
        }

        public Node Visit(Negative node)
        {
            node.expression= (Expression)node.expression.Accept(this);

            return node;
        }

        public Node Visit(Not node)
        {
            node.expression= (Expression)node.expression.Accept(this);

            return node;
        }

        public Node Visit(AnonymousFunction node)
        {
            foreach (var defExp in node.parameterList.Select(d => d.Item3).Where(d => d != null))
                defExp.Accept(this);

            node.sequence= (Expression)node.sequence.Accept(this);

            return node;
        }

        public Node Visit(AnonymousLambdaFunction node)
        {
            foreach (var defExp in node.parameterList.Select(d => d.Item3).Where(d => d != null))
                defExp.Accept(this);

            node.expression= (Expression)node.expression.Accept(this);

            return node;
        }

        public Node Visit(FunctionCall node)
        {
            node.function= (Expression)node.function.Accept(this);

            foreach (var param in node.parameters)
                param.Accept(this);

            return node;
        }

        public Node Visit(GroupedEquation node)
        {
            node.expression= (Expression)node.expression.Accept(this);

            return node;
        }

        public Node Visit(MethodCall node)
        {
            node.methodTable= (Expression)node.methodTable.Accept(this);

            foreach (var param in node.parameters)
                param.Accept(this);

            return node;
        }

        public Node Visit(NullPropagation node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(ShiftLeft node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(ShiftRight node)
        {
            node.left= (Expression)node.left.Accept(this);
            node.right= (Expression)node.right.Accept(this);

            return node;
        }

        public Node Visit(Table node)
        {
            foreach (var key in node.values)
            {
                key.Item1 = key.Item1.Accept(this);
                key.Item2? = key.Item2?.Accept(this);
            }

            return node;
        }

        public Node Visit(TableDotIndex node)
        {
            node.table= (Expression)node.table.Accept(this);

            return node;
        }

        public Node Visit(TableIndex node)
        {
            node.table= (Expression)node.table.Accept(this);
            node.key= (Expression)node.key.Accept(this);

            return node;
        }

        public Node Visit(Ternary node)
        {
            node.conditionExpression= (Expression)node.conditionExpression.Accept(this);
            node.expression1= (Expression)node.expression1.Accept(this);
            node.expression2= (Expression)node.expression2.Accept(this);

            return node;
        }

        public Node Visit(Variable node)
        {
            return node;
        }

        public Node Visit(This node)
        {
            return node;
        }

        public Node Visit(Vararg node)
        {
            return node;
        }

        public Node Visit(Null node)
        {
            return node;
        }
    }
}
