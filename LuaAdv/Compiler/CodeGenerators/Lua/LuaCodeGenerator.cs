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
using LuaAdv.Compiler.SemanticAnalyzer1;

namespace LuaAdv.Compiler.CodeGenerators.Lua
{
    public class LuaCodeGenerator : IAstVisitor
    {
        public Node MainNode { get; }

        private SourceCodeBuilder builder = new SourceCodeBuilder();
        private Stack<Queue<string>> beforeQueue = new Stack<Queue<string>>();
        private Stack<Queue<string>> afterQueue = new Stack<Queue<string>>();

        public Scope MainScope { get; protected set; }
        public Scope CurrentScope { get; protected set; }

        public string Output => builder.Output;

        public LuaCodeGenerator(Node mainNode)
        {
            beforeQueue.Push(new Queue<string>());
            afterQueue.Push(new Queue<string>());

            MainNode = mainNode;
            mainNode.Accept(this);

            MainScope = (mainNode as ScopeNode).scope;
        }

        void PushTab() => builder.Tabs++;
        void PopTab() => builder.Tabs--;

        void Push()
        {
            builder.PushStringBuilder();
            beforeQueue.Push(new Queue<string>());
            afterQueue.Push(new Queue<string>());
        }

        void Pop()
        {
            builder.PopStringBuilder();
            beforeQueue.Pop();
            afterQueue.Pop();
        }

        void PushBefore()
        {
            Push();
        }

        void PopBefore()
        {
            var data = builder.Output;
            Pop();
            beforeQueue.Peek().Enqueue(data);
        }

        void PushAfter()
        {
            Push();
        }

        void PopAfter()
        {
            var data = builder.Output;
            Pop();
            afterQueue.Peek().Enqueue(data);
        }

        public Node Visit(Node node)
        {
            throw new NotImplementedException(node.GetType().Name + " analysis not implemented!");
        }

        public Node Visit(Statement node)
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

        public Node Visit(ScopeExpression node)
        {
            var oldScope = CurrentScope;
            CurrentScope = node.scope;
            node.expression.Accept(this);
            CurrentScope = oldScope;

            return node;
        }

        public ScopeNode PushScope(Node innerNode)
        {
            return new ScopeNode(innerNode, new Scope(CurrentScope));
        }

        #endregion

        public Node Visit(For node)
        {
            // TODO: Rewrite to use Lua for loop

            //Node init = expression.init;

            //if (init is LocalVariablesDeclaration)
            //{
            //    var localVarDecl = (LocalVariablesDeclaration) init;
            //    init = new GlobalVariablesDeclaration();
            //}

            PushBefore();
            node.init.Accept(this);
            PopBefore();

            builder.Append("while ");
            node.condition.Accept(this);
            builder.AppendLine(" do");

            PushTab();
            node.sequence.Accept(this);
            node.after.Accept(this);
            PopTab();

            builder.AppendLine();
            builder.AppendLine("end");

            return node;
        }

        public Node Visit(Foreach node)
        {
            // TODO: Come up with syntax for allowing iterators other than "pairs"
            
            builder.Append("for {0}, {1} in pairs(", node.keyName ?? "_", node.varName);
            node.table.Accept(this);
            builder.AppendLine(") do");
            PushTab();
            node.sequence.Accept(this);
            PopTab();
            builder.AppendLine();
            builder.AppendLine("end");

            return node;
        }

        public Node Visit(While node)
        {
            // TODO: Fix after and before queue

            builder.Append("while ");
            Push();
            node.condition.Accept(this);
            string sequence = builder.Output;
            var queueBefore = beforeQueue.Peek();
            var queueAfter = afterQueue.Peek();
            Pop();
            builder.Append(sequence);
            builder.AppendLine(" do");

            PushTab();

            while (queueBefore.Count > 0)
                builder.AppendLine(queueBefore.Dequeue());

            while (queueAfter.Count > 0)
                builder.AppendLine(queueAfter.Dequeue());

            node.sequence.Accept(this);

            PopTab();

            builder.AppendLine();
            builder.AppendLine("end");

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
                Push();
                node.nodes[i].Accept(this);
                string sequence = builder.Output;
                var queueBefore = beforeQueue.Peek();
                var queueAfter = afterQueue.Peek();
                Pop();

                while (queueBefore.Count > 0)
                    builder.AppendLine(queueBefore.Dequeue());

                builder.Append(sequence);

                while (queueAfter.Count > 0)
                {
                    builder.AppendLine();
                    builder.Append(queueAfter.Dequeue());
                }

                if (i != node.nodes.Length - 1)
                    builder.AppendLine();
            }

            return node;
        }

        public Node Visit(If node)
        {
            for (int i = 0; i < node.ifs.Count; i++)
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
            if (node.local)
                builder.Append("local ");
            builder.Append("function ");
            node.name.Accept(this);
            builder.Append("(");

            for (int i = 0; i < node.parameterList.Count; i++)
            {
                if (node.parameterList[i].Item2 == "this")
                    builder.Append("self");
                else
                    builder.Append(node.parameterList[i].Item2);
                if (i != node.parameterList.Count - 1)
                    builder.Append(", ");
            }

            builder.AppendLine(")");
            PushTab();

            for (int i = 0; i < node.parameterList.Count; i++)
            {
                if (node.parameterList[i].Item3 == null)
                    continue;

                builder.Append("if not {0} then {0} = ", node.parameterList[i].Item2);
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
            // This is supposed to be lowered by the semantic analyser

            return node;
        }

        public Node Visit(StatementLambdaMethodDeclaration node)
        {
            // This is supposed to be lowered by the semantic analyser

            return node;
        }

        public Node Visit(StatementMethodDeclaration node)
        {
            builder.Append("function ");
            node.tableName.Accept(this);
            builder.Append(":");
            builder.Append(node.name);
            builder.Append("(");

            for (int i = 0; i < node.parameterList.Count; i++)
            {
                builder.Append(node.parameterList[i].Item2);
                if (i != node.parameterList.Count - 1)
                    builder.Append(", ");
            }

            builder.AppendLine(")");
            PushTab();

            for (int i = 0; i < node.parameterList.Count; i++)
            {
                if (node.parameterList[i].Item3 == null)
                    continue;

                builder.Append("if not {0} then {0} = ", node.parameterList[i].Item2);
                node.parameterList[i].Item3.Accept(this);
                builder.AppendLine(" end");
            }

            node.sequence.Accept(this);

            PopTab();

            builder.AppendLine();
            builder.AppendLine("end");

            return node;
        }

        public Node Visit(GlobalVariablesDeclaration node)
        {
            for (int i = 0; i < node.variables.Length; i++)
            {
                node.variables[i].Accept(this);
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
            bool pushEntire = node.expression is FunctionCall || node.expression is MethodCall || node.expression is SuperCall;

            if (pushEntire)
                node.expression.Accept(this);
            else
            {
                Push();
                node.expression.Accept(this);
                var queueBefore = beforeQueue.Peek();
                var queueAfter = afterQueue.Peek();
                Pop();

                builder.AppendLine();

                foreach (var value in queueBefore)
                    builder.AppendLine(value);

                foreach (var value in queueAfter)
                    builder.AppendLine(value);
            }

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
            builder.Append(node.value.ToString().Replace(',', '.'));

            return node;
        }

        public Node Visit(StringType node)
        {
            builder.Append("\"{0}\"", node.value);

            return node;
        }

        public Node Visit(InterpolatedString node)
        {
            throw new CompilerException("Interpolated strings are supposed to be lowered in semantic analysis.", node.Token);
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
            switch (node.type)
            {
                case "number":
                    builder.Append("type(");
                    node.expression.Accept(this);
                    builder.Append(") == \"number\"");
                    break;
                case "boolean":
                    builder.Append("type(");
                    node.expression.Accept(this);
                    builder.Append(") == \"boolean\"");
                    break;
                case "string":
                    builder.Append("type(");
                    node.expression.Accept(this);
                    builder.Append(") == \"string\"");
                    break;
                case "function":
                    builder.Append("type(");
                    node.expression.Accept(this);
                    builder.Append(") == \"function\"");
                    break;
                case "table":
                    builder.Append("type(");
                    node.expression.Accept(this);
                    builder.Append(") == \"table\"");
                    break;
                default: // X.__type or type(x)
                    builder.Append("(");
                    node.expression.Accept(this);
                    builder.Append(").isType and (");
                    node.expression.Accept(this);
                    builder.Append("):isType(\"{0}\") or (type(", node.type);
                    node.expression.Accept(this);
                    builder.Append(") == \"{0}\")", node.type);
                    break;
            }

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
            builder.Append("not ");
            node.expression.Accept(this);

            return node;
        }

        public Node Visit(AnonymousFunction node)
        {
            //foreach (var defExp in expression.parameterList.Select(d => d.Item3).Where(d => d != null))
            //    defExp.Accept(this);

            //expression.sequence.Accept(this);

            builder.Append("function(");

            for (int i = 0; i < node.parameterList.Count; i++)
            {
                if (node.parameterList[i].Item2 == "this")
                    builder.Append("self");
                else
                    builder.Append(node.parameterList[i].Item2);
                if (i != node.parameterList.Count - 1)
                    builder.Append(", ");
            }

            builder.AppendLine(")");
            PushTab();

            for (int i = 0; i < node.parameterList.Count; i++)
            {
                if (node.parameterList[i].Item3 == null)
                    continue;

                builder.Append("if not {0} then {0} = ", node.parameterList[i].Item2);
                node.parameterList[i].Item3.Accept(this);
                builder.AppendLine(" end");
            }

            node.sequence.Accept(this);

            PopTab();
            builder.AppendLine();
            builder.Append("end");

            return node;
        }

        public Node Visit(AnonymousLambdaFunction node)
        {
            // This is supposed to be lowered by the semantic analyser

            return node;
        }

        public Node Visit(FunctionCall node)
        {
            node.function.Accept(this);

            builder.Append("(");

            for (int i = 0; i < node.parameters.Length; i++)
            {
                node.parameters[i].Accept(this);
                if (i != node.parameters.Length - 1)
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

        public Node Visit(SuperCall node)
        {
            // TODO: This should be moved to the semantic analyzer.
            if(CurrentScope.RawFunctionName == null)
                throw new CompilerException("Super-method cannot be called in an no-name function.", node.Token.Line, node.Token.Character);

            builder.Append("getmetatable(self).__baseclass.{0}(self", CurrentScope.RawFunctionName);

            if (node.parameters.Length > 0)
                builder.Append(", ");

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
            if (node.values.Length == 0)
            {
                builder.Append("{ }");
                return node;
            }

            builder.AppendLine("{");
            PushTab();
            foreach (var key in node.values)
            {
                if (key.Item1 != null)
                {
                    // TODO: Don't use tbl["key"] but tbl.key when key is a valid identifier string 
                    builder.Append("[");
                    key.Item1.Accept(this);
                    builder.Append("] = ");
                    key.Item2.Accept(this);
                }
                else
                    key.Item2.Accept(this);

                builder.AppendLine(",");
            }
            PopTab();
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

        public Node Visit(TableLength node)
        {
            builder.Append("#");
            node.table.Accept(this);

            return node;
        }

        public Node Visit(CommentNode node)
        {
            return node;
        }

        public Node Visit(DocumentationCommentNode node)
        {
            return node;
        }

        private const string ClassIndexFunctionBody = @"function(tbl, key)
    local meta = getmetatable(tbl)
    
    local field = meta[key]
    if field then
        return field
    end
    
    local base = meta.__baseclass
    if base then
        return base[key]
    end
end";

        private const string ClassIsTypeFunctionBody = @"function(self, type, metatable)
    if metatable then
        if metatable.__type == type then
            return true
        elseif metatable.__baseclass then
            return self:isType(type, metatable.__baseclass)
        else
            return false
        end
    elseif self.__type == type then
	    return true
	elseif getmetatable(self).__baseclass then
		return self:isType(type, getmetatable(self).__baseclass)
	else 
		return false
	end
end";

        public Node Visit(Class node)
        {
            var metatableName = $"C{node.name}";

            // Metatable
            if (node.local)
                builder.Append("local ");
            builder.AppendLine("{0} = {{}}", metatableName);
            builder.AppendLine("{0}.__index = {1}", metatableName, ClassIndexFunctionBody);
            builder.AppendLine("{0}.__type = \"{1}\"", metatableName, node.name);
            builder.AppendLine("{0}.__baseclass = nil", metatableName);
            builder.AppendLine("{0}.isType = {1}", metatableName, ClassIsTypeFunctionBody);
            builder.AppendLine();

            // Inheritance
            if (!string.IsNullOrEmpty(node.baseClass))
            {
                var baseMetatableName = $"C{node.baseClass}";
                builder.AppendLine("if not {0} then error(\"{1}\") end", baseMetatableName,
                    $"Base class '{node.baseClass}' not found. Class '{node.name}' could not be constructed.");

                builder.AppendLine("{0}.__baseclass = {1}", metatableName, baseMetatableName);
            }

            // Constructor
            if(node.local)
                builder.Append("local ");
            builder.AppendLine("function {0}(...)", node.name);
            PushTab();
            builder.AppendLine("local tbl = { }");
            builder.AppendLine("setmetatable(tbl, {0})", metatableName);
            builder.AppendLine("tbl:__this(...)");
            builder.AppendLine("return tbl");
            PopTab();
            builder.AppendLine("end");

            return node;
        }

        public Node Visit(ClassMethod node)
        {
            node.method.Accept(this);

            return node;
        }

        public Node Visit(SpecialNode node)
        {
            throw new Exception("Special nodes should already have been replaced in semantic analyzer.");
        }

        public Node Visit(SingleEnum node)
        {
            // Enums don't exist in final code
            return node;
        }
        public Node Visit(MultiEnum node)
        {
            // Enums don't exist in final code
            return node;
        }

        public Node Visit(Decorator node)
        {
            // Decorators don't exist in final code
            return node;
        }

        public Node Visit(DecoratedClass node)
        {
            return new Sequence(null, node.classSequence.Concat(node.decoratorSequence).ToArray()).Accept(this);
        }

        public Node Visit(StaticIf node)
        {
            throw new Exception("Static if shouldn't exist here.");
        }
    }
}
