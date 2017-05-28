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

namespace LuaAdv.Compiler.CodeGenerators.Lua
{
    public class LuaCodeGenerator : IAstVisitor
    {
        public Node MainNode { get; }

        private SourceCodeBuilder builder = new SourceCodeBuilder();
        private Queue<string> beforeQueue = new Queue<string>();
        private Queue<string> afterQueue = new Queue<string>();

        public string Output => builder.Output;

        public LuaCodeGenerator(Node mainNode)
        {
            MainNode = mainNode;
            mainNode.Accept(this);
        }

        void PushTab() => builder.Tabs++;
        void PopTab() => builder.Tabs--;

        void PushBefore()
        {
            builder.PushStringBuilder();
        }

        void PopBefore()
        {
            beforeQueue.Enqueue(builder.Output);
            builder.PopStringBuilder();
        }

        void PushAfter()
        {
            builder.PushStringBuilder();
        }

        void PopAfter()
        {
            afterQueue.Enqueue(builder.Output);
            builder.PopStringBuilder();
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
            node.init.Accept(this);
            node.condition.Accept(this);
            node.after.Accept(this);
            node.sequence.Accept(this);

            return node;
        }

        public Node Visit(Foreach node)
        {
            node.table.Accept(this);
            node.sequence.Accept(this);

            return node;
        }

        public Node Visit(While node)
        {
            node.condition.Accept(this);
            node.sequence.Accept(this);

            return node;
        }

        public Node Visit(Break node)
        {
            builder.Append("break");

            return node;
        }

        public Node Visit(Continue node)
        {
            builder.Append("continue");

            return node;
        }

        public Node Visit(Sequence node)
        {
            for (int i = 0; i < node.nodes.Length; i++)
            {
                builder.PushStringBuilder();
                node.nodes[i].Accept(this);
                string sequence = builder.Output;
                builder.PopStringBuilder();

                while (beforeQueue.Count > 0)
                    builder.AppendLine(beforeQueue.Dequeue());

                builder.Append(sequence);

                while (afterQueue.Count > 0)
                {
                    builder.AppendLine();
                    builder.Append(afterQueue.Dequeue());
                }

                if (i != node.nodes.Length - 1)
                    builder.AppendLine();
            }

            return node;
        }

        public Node Visit(If node)
        {
            for(int i = 0; i < node.ifs.Count; i++)
            {
                var ifChild = node.ifs[i];

                if (i == 0)
                {
                    builder.Append("if ");
                    ifChild.Item2.Accept(this);
                    builder.AppendLine(" then");
                    PushTab();
                    ifChild.Item3.Accept(this);
                    PopTab();
                    builder.AppendLine();
                }
                else if (ifChild.Item2 != null)
                {
                    builder.Append("elseif ");
                    ifChild.Item2.Accept(this);
                    builder.AppendLine(" then");
                    PushTab();
                    ifChild.Item3.Accept(this);
                    PopTab();
                    builder.AppendLine();
                }
                else
                {
                    builder.AppendLine("else ");
                    PushTab();
                    ifChild.Item3.Accept(this);
                    PopTab();
                    builder.AppendLine();
                }
            }

            builder.AppendLine("end");

            return node;
        }

        public Node Visit(Return node)
        {
            builder.Append("return ");

            for (int i = 0; i < node.values.Length; i++)
            {
                node.values[i].Accept(this);
                if (i != node.values.Length - 1)
                    builder.Append(", ");
            }

            return node;
        }

        public Node Visit(StatementFunctionDeclaration node)
        {
            if(node.local)
                builder.Append("local ");
            builder.Append("function ");
            node.name.Accept(this);
            builder.Append("(");

            for (int i = 0; i < node.parameterList.Count; i++)
            {
                builder.Append(node.parameterList[i].Item2);
                if(i != node.parameterList.Count - 1)
                    builder.Append(", ");
            }

            builder.AppendLine(")");
            PushTab();

            for (int i = 0; i < node.parameterList.Count; i++)
            {
                if(node.parameterList[i].Item3 == null)
                    continue;

                builder.Append("if !{0} then {0} = ", node.parameterList[i].Item2);
                node.parameterList[i].Item3.Accept(this);
                builder.AppendLine(" end");
            }

            node.sequence.Accept(this);

            PopTab();
            builder.AppendLine();
            builder.AppendLine("end");

            return node;
        }

        public Node Visit(StatementLambdaFunctionDeclaration node)
        {
            foreach (var defExp in node.parameterList.Select(d => d.Item3).Where(d => d != null))
                defExp.Accept(this);

            node.expression.Accept(this);

            return node;
        }

        public Node Visit(StatementLambdaMethodDeclaration node)
        {
            foreach (var defExp in node.parameterList.Select(d => d.Item3).Where(d => d != null))
                defExp.Accept(this);

            node.expression.Accept(this);

            return node;
        }

        public Node Visit(StatementMethodDeclaration node)
        {
            foreach (var defExp in node.parameterList.Select(d => d.Item3).Where(d => d != null))
                defExp.Accept(this);

            node.sequence.Accept(this);

            return node;
        }

        public Node Visit(GlobalVariablesDeclaration node)
        {
            for (int i = 0; i < node.variables.Length; i++)
            {
                node.variables[i].Accept(this);
                if(i != node.variables.Length - 1)
                    builder.Append(", ");
            }

            if (node.values.Length > 0)
            {
                builder.Append(" = ");

                for (int i = 0; i < node.values.Length; i++)
                {
                    node.values[i].Accept(this);
                    if (i != node.values.Length - 1)
                        builder.Append(", ");
                }
            }

            return node;
        }

        public Node Visit(LocalVariablesDeclaration node)
        {
            builder.Append("local ");

            for (int i = 0; i < node.variables.Length; i++)
            {
                builder.Append(node.variables[i].Item2);
                if (i != node.variables.Length - 1)
                    builder.Append(", ");
            }

            if (node.values.Length > 0)
            {
                builder.Append(" = ");

                for (int i = 0; i < node.values.Length; i++)
                {
                    node.values[i].Accept(this);
                    if (i != node.values.Length - 1)
                        builder.Append(", ");
                }
            }

            return node;
        }

        public Node Visit(StatementExpression node)
        {
            bool pushEntire = node.expression is FunctionCall || node.expression is MethodCall;

            if(!pushEntire)
                builder.PushStringBuilder();

            node.expression.Accept(this);

            if (!pushEntire)
                builder.PopStringBuilder();

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
            PushBefore();
            node.left.Accept(this);
            builder.Append(" = ");
            node.right.Accept(this);
            PopBefore();

            node.left.Accept(this);

            return node;
        }

        public Node Visit(AddAssignmentOperator node)
        {
            PushBefore();
            node.left.Accept(this);
            builder.Append(" = ");
            node.left.Accept(this);
            builder.Append(" + ");
            node.right.Accept(this);
            PopBefore();

            node.left.Accept(this);

            return node;
        }

        public Node Visit(SubtractAssignmentOperator node)
        {
            PushBefore();
            node.left.Accept(this);
            builder.Append(" = ");
            node.left.Accept(this);
            builder.Append(" - ");
            node.right.Accept(this);
            PopBefore();

            node.left.Accept(this);

            return node;
        }

        public Node Visit(MultiplyAssignmentOperator node)
        {
            PushBefore();
            node.left.Accept(this);
            builder.Append(" = ");
            node.left.Accept(this);
            builder.Append(" * ");
            node.right.Accept(this);
            PopBefore();

            node.left.Accept(this);

            return node;
        }

        public Node Visit(DivideAssignmentOperator node)
        {
            PushBefore();
            node.left.Accept(this);
            builder.Append(" = ");
            node.left.Accept(this);
            builder.Append(" / ");
            node.right.Accept(this);
            PopBefore();

            node.left.Accept(this);

            return node;
        }

        public Node Visit(ModuloAssignmentOperator node)
        {
            PushBefore();
            node.left.Accept(this);
            builder.Append(" = ");
            node.left.Accept(this);
            builder.Append(" % ");
            node.right.Accept(this);
            PopBefore();

            node.left.Accept(this);

            return node;
        }

        public Node Visit(ConcatAssignmentOperator node)
        {
            PushBefore();
            node.left.Accept(this);
            builder.Append(" = ");
            node.left.Accept(this);
            builder.Append(" .. ");
            node.right.Accept(this);
            PopBefore();

            node.left.Accept(this);

            return node;
        }

        public Node Visit(Bool node)
        {
            builder.Append(node.value ? "true" : "false");

            return node;
        }

        public Node Visit(Number node)
        {
            builder.Append(node.value.ToString());

            return node;
        }

        public Node Visit(StringType node)
        {
            builder.Append("\"{0}\"", node.value);

            return node;
        }

        public Node Visit(Equals node)
        {
            node.left.Accept(this);
            builder.Append(" == ");
            node.right.Accept(this);

            return node;
        }

        public Node Visit(NotEquals node)
        {
            node.left.Accept(this);
            builder.Append(" ~= ");
            node.right.Accept(this);

            return node;
        }

        public Node Visit(Greater node)
        {
            node.left.Accept(this);
            builder.Append(" > ");
            node.right.Accept(this);

            return node;
        }

        public Node Visit(Less node)
        {
            node.left.Accept(this);
            builder.Append(" < ");
            node.right.Accept(this);

            return node;
        }

        public Node Visit(GreaterOrEqual node)
        {
            node.left.Accept(this);
            builder.Append(" >= ");
            node.right.Accept(this);

            return node;
        }

        public Node Visit(LessOrEqual node)
        {
            node.left.Accept(this);
            builder.Append(" <= ");
            node.right.Accept(this);

            return node;
        }

        public Node Visit(Is node)
        {
            node.expression.Accept(this);
            // TODO: Is

            return node;
        }

        public Node Visit(ConditionalAnd node)
        {
            node.left.Accept(this);
            builder.Append(" and ");
            node.right.Accept(this);

            return node;
        }

        public Node Visit(ConditionalOr node)
        {
            node.left.Accept(this);
            builder.Append(" or ");
            node.right.Accept(this);

            return node;
        }

        public Node Visit(LogicalAnd node)
        {
            builder.Append("bit.band(");
            node.left.Accept(this);
            builder.Append(", ");
            node.right.Accept(this);
            builder.Append(")");

            return node;
        }

        public Node Visit(LogicalOr node)
        {
            builder.Append("bit.bor(");
            node.left.Accept(this);
            builder.Append(", ");
            node.right.Accept(this);
            builder.Append(")");

            return node;
        }

        public Node Visit(LogicalXor node)
        {
            builder.Append("bit.bxor(");
            node.left.Accept(this);
            builder.Append(", ");
            node.right.Accept(this);
            builder.Append(")");

            return node;
        }

        public Node Visit(Add node)
        {
            node.left.Accept(this);
            builder.Append(" + ");
            node.right.Accept(this);

            return node;
        }

        public Node Visit(Subtract node)
        {
            node.left.Accept(this);
            builder.Append(" - ");
            node.right.Accept(this);

            return node;
        }

        public Node Visit(Multiply node)
        {
            node.left.Accept(this);
            builder.Append(" * ");
            node.right.Accept(this);

            return node;
        }

        public Node Visit(Divide node)
        {
            node.left.Accept(this);
            builder.Append(" / ");
            node.right.Accept(this);

            return node;
        }

        public Node Visit(Modulo node)
        {
            node.left.Accept(this);
            builder.Append(" % ");
            node.right.Accept(this);

            return node;
        }

        public Node Visit(Power node)
        {
            node.left.Accept(this);
            builder.Append(" ^ ");
            node.right.Accept(this);

            return node;
        }

        public Node Visit(Concat node)
        {
            node.left.Accept(this);
            builder.Append(" .. ");
            node.right.Accept(this);

            return node;
        }

        public Node Visit(PostDecrement node)
        {
            node.expression.Accept(this);

            PushAfter();
            node.expression.Accept(this);
            builder.Append(" = ");
            node.expression.Accept(this);
            builder.Append(" - 1");
            PopAfter();

            return node;
        }

        public Node Visit(PostIncrement node)
        {
            node.expression.Accept(this);

            PushAfter();
            node.expression.Accept(this);
            builder.Append(" = ");
            node.expression.Accept(this);
            builder.Append(" + 1");
            PopAfter();

            return node;
        }

        public Node Visit(PreDecrement node)
        {
            PushBefore();
            node.expression.Accept(this);
            builder.Append(" = ");
            node.expression.Accept(this);
            builder.Append(" - 1");
            PopBefore();

            node.expression.Accept(this);

            return node;
        }

        public Node Visit(PreIncrement node)
        {
            PushBefore();
            node.expression.Accept(this);
            builder.Append(" = ");
            node.expression.Accept(this);
            builder.Append(" + 1");
            PopBefore();

            node.expression.Accept(this);

            return node;
        }

        public Node Visit(Negation node)
        {
            builder.Append("bit.bnot(");
            node.expression.Accept(this);
            builder.Append(")");

            return node;
        }

        public Node Visit(Negative node)
        {
            builder.Append("-");
            node.expression.Accept(this);

            return node;
        }

        public Node Visit(Not node)
        {
            builder.Append("!");
            node.expression.Accept(this);

            return node;
        }

        public Node Visit(AnonymousFunction node)
        {
            foreach (var defExp in node.parameterList.Select(d => d.Item3).Where(d => d != null))
                defExp.Accept(this);

            node.sequence.Accept(this);

            return node;
        }

        public Node Visit(AnonymousLambdaFunction node)
        {
            foreach (var defExp in node.parameterList.Select(d => d.Item3).Where(d => d != null))
                defExp.Accept(this);

            node.expression.Accept(this);

            return node;
        }

        public Node Visit(FunctionCall node)
        {
            node.function.Accept(this);

            builder.Append("(");

            for (int i = 0; i < node.parameters.Length; i++)
            {
                node.parameters[i].Accept(this);
                if(i != node.parameters.Length - 1)
                    builder.Append(", ");
            }

            builder.Append(")");

            return node;
        }

        public Node Visit(GroupedEquation node)
        {
            builder.Append("(");
            node.expression.Accept(this);
            builder.Append(")");

            return node;
        }

        public Node Visit(MethodCall node)
        {
            node.methodTable.Accept(this);

            builder.Append(":{0}(", node.name);

            for (int i = 0; i < node.parameters.Length; i++)
            {
                node.parameters[i].Accept(this);
                if (i != node.parameters.Length - 1)
                    builder.Append(", ");
            }

            builder.Append(")");

            return node;
        }

        public Node Visit(NullPropagation node)
        {
            node.left.Accept(this);
            builder.Append(" or ");
            node.right.Accept(this);

            return node;
        }

        public Node Visit(ShiftLeft node)
        {
            builder.Append("bit.lshift(");
            node.left.Accept(this);
            builder.Append(", ");
            node.right.Accept(this);
            builder.Append(")");

            return node;
        }

        public Node Visit(ShiftRight node)
        {
            builder.Append("bit.rshift(");
            node.left.Accept(this);
            builder.Append(", ");
            node.right.Accept(this);
            builder.Append(")");

            return node;
        }

        public Node Visit(Table node)
        {
            builder.AppendLine("{");
            foreach (var key in node.values)
            {
                if (key.Item2 != null)
                {
                    builder.Append("[");
                    key.Item1.Accept(this);
                    builder.Append("] = ");
                    key.Item2.Accept(this);
                }
                else
                    key.Item1.Accept(this);

                builder.AppendLine(",");
            }
            builder.Append("}");

            return node;
        }

        public Node Visit(TableDotIndex node)
        {
            node.table.Accept(this);
            builder.Append(".{0}", node.index);

            return node;
        }

        public Node Visit(TableIndex node)
        {
            node.table.Accept(this);
            builder.Append("[");
            node.key.Accept(this);
            builder.Append("]");

            return node;
        }

        public Node Visit(Ternary node)
        {
            builder.Append("(");
            node.conditionExpression.Accept(this);
            builder.Append(") and (");
            node.expression1.Accept(this);
            builder.Append(") or (");
            node.expression2.Accept(this);
            builder.Append(")");

            return node;
        }

        public Node Visit(Variable node)
        {
            builder.Append(node.name);

            return node;
        }

        public Node Visit(This node)
        {
            builder.Append("self");

            return node;
        }

        public Node Visit(Vararg node)
        {
            builder.Append("...");

            return node;
        }

        public Node Visit(Null node)
        {
            builder.Append("nil");

            return node;
        }

        public Node Visit(ScopeNode node)
        {
            throw new NotImplementedException();
        }
    }
}
