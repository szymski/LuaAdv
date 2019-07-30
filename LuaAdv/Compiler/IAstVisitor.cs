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

namespace LuaAdv.Compiler
{
    public interface IAstVisitor
    {
        Node Visit(Node node);

        #region Statements

        Node Visit(Statement node);

        // Loops
        Node Visit(For node);
        Node Visit(Foreach node);
        Node Visit(While node);
        Node Visit(Break node);
        Node Visit(Continue node);

        Node Visit(Sequence node);
        Node Visit(If node);
        Node Visit(Return node);
        Node Visit(StatementFunctionDeclaration node);
        Node Visit(StatementLambdaFunctionDeclaration node);
        Node Visit(StatementLambdaMethodDeclaration node);
        Node Visit(StatementMethodDeclaration node);
        Node Visit(GlobalVariablesDeclaration node);
        Node Visit(LocalVariablesDeclaration node);
        Node Visit(StatementExpression node);
        Node Visit(NullStatement node);

        #endregion

        #region Expressions

        Node Visit(Expression node);

        // Assignment
        Node Visit(ValueAssignmentOperator node);
        Node Visit(AddAssignmentOperator node);
        Node Visit(SubtractAssignmentOperator node);
        Node Visit(MultiplyAssignmentOperator node);
        Node Visit(DivideAssignmentOperator node);
        Node Visit(ModuloAssignmentOperator node);
        Node Visit(ConcatAssignmentOperator node);

        // Basic types
        Node Visit(Bool node);
        Node Visit(Number node);
        Node Visit(StringType node);

        // Comparison
        Node Visit(Equals node);
        Node Visit(NotEquals node);
        Node Visit(Greater node);
        Node Visit(Less node);
        Node Visit(GreaterOrEqual node);
        Node Visit(LessOrEqual node);
        Node Visit(Is node);

        // Conditional
        Node Visit(ConditionalAnd node);
        Node Visit(ConditionalOr node);

        // Logical
        Node Visit(LogicalAnd node);
        Node Visit(LogicalOr node);
        Node Visit(LogicalXor node);

        // Math
        Node Visit(Add node);
        Node Visit(Subtract node);
        Node Visit(Multiply node);
        Node Visit(Divide node);
        Node Visit(Modulo node);
        Node Visit(Power node);
        Node Visit(Concat node);

        // Unary
        Node Visit(PostDecrement node);
        Node Visit(PostIncrement node);
        Node Visit(PreDecrement node);
        Node Visit(PreIncrement node);
        Node Visit(Negation node);
        Node Visit(Negative node);
        Node Visit(Not node);

        Node Visit(AnonymousFunction node);
        Node Visit(AnonymousLambdaFunction node);
        Node Visit(FunctionCall node);
        Node Visit(GroupedEquation node);
        Node Visit(MethodCall node);
        Node Visit(SuperCall node);
        Node Visit(NullPropagation node);
        Node Visit(ShiftLeft node);
        Node Visit(ShiftRight node);
        Node Visit(Table node);
        Node Visit(TableDotIndex node);
        Node Visit(TableIndex node);
        Node Visit(Ternary node);
        Node Visit(Variable node);
        Node Visit(This node);
        Node Visit(Vararg node);
        Node Visit(Null node);
        Node Visit(TableLength node);

        #endregion

        Node Visit(Class node);
        Node Visit(ClassMethod node);
        Node Visit(ScopeNode node);
        Node Visit(CommentNode node);
        Node Visit(DocumentationCommentNode node);
        Node Visit(SpecialNode node);
        Node Visit(SingleEnum node);
        Node Visit(MultiEnum node);
        Node Visit(Decorator node);

        Node Visit(StaticIf node);
    }
}
