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

namespace LuaAdv.Compiler.SyntaxAnalyzer
{
    public partial class SyntaxAnalyzer
    {
        private string message = "Expression expected.";
        private bool errorOnNoExpression = false;
        private bool inTernary = false;
        private bool requiresNamedVariable;

        /// <summary>
        /// Parses and returns an expression.
        /// </summary>
        /// <param name="message">Message to show, if there was a problem with parsing expression.</param>
        /// <param name="errorOnNoExpression">Default by true. Throws an exception, if no expression was found.</param>
        /// <returns>Expression node.</returns>
        internal Expression Expression(string message = "Expression expected.", bool errorOnNoExpression = true)
        {
            var prevMsg = message;
            var prevErrorOnNoExpression = this.errorOnNoExpression;
            this.message = message;
            this.errorOnNoExpression = errorOnNoExpression;

            var expression = Expression_Ternary();

            this.message = prevMsg;
            this.errorOnNoExpression = prevErrorOnNoExpression;

            return expression;
        }

        // BUG: Cancel comments while parsing expressions - comment tokens inside expressions break them

        Expression Expression_Ternary()
        {
            var exp = Expression_NullPropagation();

            if (AcceptSymbol("?"))
            {
                bool before = inTernary;
                inTernary = true;

                var exp2 = Expression_NullPropagation();

                inTernary = before;

                RequireSymbol(":", "':' required for ternary operator.");

                var exp3 = Expression_NullPropagation();

                return new Ternary(exp, exp2, exp3);
            }

            return exp;
        }

        Expression Expression_NullPropagation()
        {
            var exp = Expression_ConditionalOr();

            if (AcceptSymbol("??"))
            {
                var symbolToken = token;

                var exp2 = Expression_ConditionalOr();

                return new NullPropagation(exp, symbolToken, exp2);
            }

            return exp;
        }

        Expression Expression_ConditionalOr()
        {
            var exp = Expression_ConditionalAnd();

            if (AcceptSymbol("||"))
                return new ConditionalOr(exp, token, Expression_ConditionalOr());

            return exp;
        }

        Expression Expression_ConditionalAnd()
        {
            var exp = Expression_LogicalOr();

            if (AcceptSymbol("&&"))
                return new ConditionalAnd(exp, token, Expression_ConditionalAnd());

            return exp;
        }

        Expression Expression_LogicalOr()
        {
            var exp = Expression_LogicalXor();

            if (AcceptSymbol("|"))
                return new LogicalOr(exp, token, Expression_ConditionalOr());

            return exp;
        }

        Expression Expression_LogicalXor()
        {
            var exp = Expression_LogicalAnd();

            if (AcceptSymbol("^"))
                return new LogicalXor(exp, token, Expression_LogicalXor());

            return exp;
        }

        Expression Expression_LogicalAnd()
        {
            var exp = Expression_Equality();

            if (AcceptSymbol("&"))
                return new LogicalAnd(exp, token, Expression_LogicalAnd());

            return exp;
        }

        Expression Expression_Equality()
        {
            var exp = Expression_Comparison();

            if (AcceptSymbol("=="))
                return new Equals(exp, token, Expression_Equality());
            else if (AcceptSymbol("!="))
                return new NotEquals(exp, token, Expression_Equality());

            return exp;
        }

        Expression Expression_Comparison()
        {
            var exp = Expression_Shift();

            if (AcceptSymbol("<"))
                return new Less(exp, token, Expression_Comparison());
            else if (AcceptSymbol(">"))
                return new Greater(exp, token, Expression_Comparison());
            else if (AcceptSymbol("<="))
                return new LessOrEqual(exp, token, Expression_Comparison());
            else if (AcceptSymbol(">="))
                return new GreaterOrEqual(exp, token, Expression_Comparison());
            else if (AcceptKeyword("is"))
                return new Is(exp, token, RequireIdentifier("Type expected for 'is' operator.").Value);

            return exp;
        }

        Expression Expression_Shift()
        {
            var exp = Expression_Concat();

            if (AcceptSymbol("<<"))
                return new ShiftLeft(exp, token, Expression_Shift());
            else if (AcceptSymbol(">>"))
                return new ShiftRight(exp, token, Expression_Shift());

            return exp;
        }

        Expression Expression_Concat()
        {
            var exp = Expression_Additive();

            if (AcceptSymbol(".."))
                return new Concat(exp, token, Expression_Concat());

            return exp;
        }

        Expression Expression_Additive()
        {
            var exp = Expression_Multiplicative();

            if (AcceptSymbol("+"))
                return new Add(exp, token, Expression_Additive());
            else if (AcceptSymbol("-"))
                return new Subtract(exp, token, Expression_Additive());

            return exp;
        }

        // TODO: Right to left might be required (no recursive call, but a loop)
        Expression Expression_Multiplicative()
        {
            var exp = Expression_Power();

            if (AcceptSymbol("*"))
                return new Multiply(exp, token, Expression_Multiplicative());
            else if (AcceptSymbol("/"))
                return new Divide(exp, token, Expression_Multiplicative());
            else if (AcceptSymbol("%"))
                return new Modulo(exp, token, Expression_Multiplicative());

            return exp;
        }

        Expression Expression_Power()
        {
            var exp = Expression_Unary();

            if (AcceptSymbol("^^"))
                return new Power(exp, token, Expression_Power());

            return exp;
        }

        Expression Expression_Unary()
        {
            if (AcceptSymbol("-"))
                return new Negative(token, Expression_AfterValueOperations());
            else if (AcceptSymbol("!"))
                return new Not(token, Expression_AfterValueOperations());
            else if (AcceptSymbol("~"))
                return new Negation(token, Expression_AfterValueOperations());

            return Expression_PreOperations();
        }

        Expression Expression_PreOperations()
        {
            if (AcceptSymbol("++"))
            {
                var operatorToken = token;
                var exp = Expression_TableLength();

                EnsureNamed(exp);

                return new PreIncrement(operatorToken, exp);
            }
            else if (AcceptSymbol("--"))
            {
                var operatorToken = token;
                var exp = Expression_TableLength();

                EnsureNamed(exp);

                return new PreDecrement(operatorToken, exp);
            }

            return Expression_PostOperations();
        }

        Expression Expression_PostOperations()
        {
            var exp = Expression_AssignmentOperators();

            if (exp == null)
                return null;

            if (AcceptSymbol("++"))
            {
                var operatorToken = token;

                EnsureNamed(exp);

                return new PostIncrement(operatorToken, exp);
            }
            else if (AcceptSymbol("--"))
            {
                var operatorToken = token;

                EnsureNamed(exp);

                return new PostDecrement(operatorToken, exp);
            }

            return exp;
        }

        Expression Expression_AssignmentOperators()
        {
            var exp = Expression_TableLength();

            if (exp == null)
                return null;

            if (requiresNamedVariable)
                return exp;

            if (AcceptSymbol("="))
            {
                var operatorToken = token;
                EnsureNamed(exp);
                var valueExp = Expression();
                return new ValueAssignmentOperator(exp, operatorToken, valueExp);
            }
            // BUG: All below this line doesn't work in function calls, print(a += 123)
            else if (AcceptSymbol("+="))
            {
                var operatorToken = token;
                EnsureNamed(exp);
                var valueExp = Expression();
                return new AddAssignmentOperator(exp, operatorToken, valueExp);
            }
            else if (AcceptSymbol("-="))
            {
                var operatorToken = token;
                EnsureNamed(exp);
                var valueExp = Expression();
                return new SubtractAssignmentOperator(exp, operatorToken, valueExp);
            }
            else if (AcceptSymbol("*="))
            {
                var operatorToken = token;
                EnsureNamed(exp);
                var valueExp = Expression();
                return new MultiplyAssignmentOperator(exp, operatorToken, valueExp);
            }
            else if (AcceptSymbol("/="))
            {
                var operatorToken = token;
                EnsureNamed(exp);
                var valueExp = Expression();
                return new DivideAssignmentOperator(exp, operatorToken, valueExp);
            }
            else if (AcceptSymbol("%="))
            {
                var operatorToken = token;
                EnsureNamed(exp);
                var valueExp = Expression();
                return new ModuloAssignmentOperator(exp, operatorToken, valueExp);
            }
            else if (AcceptSymbol("..="))
            {
                var operatorToken = token;
                EnsureNamed(exp);
                var valueExp = Expression();
                return new ConcatAssignmentOperator(exp, operatorToken, valueExp);
            }

            return exp;
        }

        Expression Expression_TableLength()
        {
            if (AcceptSymbol("#"))
                return new TableLength(token, Expression_AfterValueOperations());

            return Expression_AfterValueOperations();
        }

        Expression Expression_AfterValueOperations(bool requireNamed = false)
        {
            var exp = Expression_SuperCall();

            if (exp == null)
                return null;

            int beforeTokenIndex = tokenIndex;
            Expression beforeExpression = exp;
            bool endedWithNamed = true;

            int lastOperationIndex = tokenIndex;

            while (true)
            {
                if (AcceptSymbol("."))
                {
                    exp = Expression_TableDotIndex(exp);
                    endedWithNamed = true;
                    beforeTokenIndex = tokenIndex;
                    beforeExpression = exp;
                }
                else if (AcceptSymbol(":"))
                {
                    beforeTokenIndex = tokenIndex - 1;
                    beforeExpression = exp;
                    var tempExp = Expression_SelfFunctionCall(exp);
                    exp = tempExp ?? exp;
                    endedWithNamed = false; // TODO: I don't like this. Method call isn't a named variable. Move this somewhere else.

                    // Ternary support
                    if (tempExp == null)
                    {
                        tokenIndex = lastOperationIndex;
                        return exp;
                    }
                }
                else if (AcceptSymbol("["))
                {
                    exp = Expression_TableIndex(exp);
                    endedWithNamed = true;
                    beforeTokenIndex = tokenIndex;
                    beforeExpression = exp;
                }
                else if (AcceptSymbol("("))
                {
                    exp = Expression_FunctionCall(exp);
                    endedWithNamed = false;

                    if (exp == null && requireNamed)
                    {
                        tokenIndex = beforeTokenIndex;
                        exp = beforeExpression;

                        return exp;
                    }
                }
                else
                    break;

                lastOperationIndex = tokenIndex;
            }

            if (requireNamed && !endedWithNamed)
            {
                tokenIndex = beforeTokenIndex;
                exp = beforeExpression;
            }

            return exp;
        }

        #region For Expression_AfterValueOperations

        /// <summary>
        /// Assumes the period has been already parsed.
        /// </summary>
        Expression Expression_TableDotIndex(Expression exp)
        {
            RequireIdentifier("Table index expected.");

            return new TableDotIndex(exp, token, token.Value);
        }

        /// <summary>
        /// Assumes the colon has been already parsed. Can return null, if parsing ternary operation.
        /// </summary>
        Expression Expression_SelfFunctionCall(Expression exp)
        {
            int startIndex = tokenIndex;

            if (inTernary)
            {
                if (!AcceptIdentifier())
                    return null;

                if (!AcceptSymbol("("))
                    return null;

                tokenIndex = startIndex;
            }

            var nameToken = RequireIdentifier("Method name expected.");

            RequireSymbol("(", "'(' required to call a method.");

            var parameters = ExpressionList();

            RequireSymbol(")", "')' required to close method call.");

            return new MethodCall(exp, nameToken, nameToken.Value, parameters.ToArray());
        }

        /// <summary>
        /// Assumes the bracket has been already parsed.
        /// </summary>
        Expression Expression_TableIndex(Expression exp)
        {
            var index = Expression("Table index value expected.");

            RequireSymbol("]", "']' expected to close table index.");

            return new TableIndex(exp, index);
        }

        /// <summary>
        /// Assumes the parenthesis has been already parsed.
        /// </summary>
        Expression Expression_FunctionCall(Expression exp)
        {
            Node[] parameters = ExpressionList().Cast<Node>().ToArray();

            // Default parameter, we are parsing named variable
            if (AcceptSymbol("="))
                return null;

            RequireSymbol(")", "')' required to close function call.");

            return new FunctionCall(exp, parameters);
        }

        #endregion

        Expression Expression_SuperCall()
        {
            if (AcceptKeyword("super"))
            {
                var token = this.token;

                RequireSymbol("(", "'(' required to call a super-method.");

                var parameters = ExpressionList();

                RequireSymbol(")", "')' required to close super-method call.");

                return new SuperCall(token, parameters.ToArray());
            }

            return Expression_QuickAnonymousFunction();
        }

        Expression Expression_QuickAnonymousFunction()
        {
            var exp = Expression_GroupedEquation();

            if (AcceptSymbol("=>"))
            {
                if (exp is GroupedEquation)
                    exp = (exp as GroupedEquation).expression;

                if(exp is Variable == false)
                    ThrowException("Invalid lambda parameter.");

                var paramList = new List<Tuple<Token, string, Expression>>();
                paramList.Add(new Tuple<Token, string, Expression>(exp.Token, (exp as Variable).name, null));

                if (AcceptSymbol("{"))
                {
                    var seq = Sequence(null, "}");
                    return new AnonymousFunction(exp.Token, paramList, seq);
                }

                var exp2 = Expression("Lambda function expression expected.");     

                return new AnonymousLambdaFunction(exp.Token, paramList, exp2);
            }

            return exp;
        }

        Expression Expression_GroupedEquation()
        {
            if (AcceptSymbol("("))
            {
                var startToken = token;

                var exp = Expression();

                // If a comma is present, it is a lambda function parameter list.
                if (AcceptSymbol(","))
                {
                    if (exp is Variable == false)
                        ThrowException("Invalid lambda parameter.");

                    List<Tuple<Token, string, Expression>> paramList = new List<Tuple<Token, string, Expression>>();
                    paramList.Add(new Tuple<Token, string, Expression>(token, (exp as Variable).name, null));

                    do
                    {
                        var token = RequireIdentifier("Lambda parameter expected.");
                        paramList.Add(new Tuple<Token, string, Expression>(token, token.Value, null));
                    } while (AcceptSymbol(","));

                    RequireSymbol(")", "')' required to close lambda parameter list.");

                    RequireSymbol("=>", "'=>' expected for lambda function.");

                    if (AcceptSymbol("{"))
                    {
                        var seq = Sequence(null, "}");
                        return new AnonymousFunction(exp.Token, paramList, seq);
                    }

                    var exp2 = Expression("Lambda function expression expected.");

                    return new AnonymousLambdaFunction(exp.Token, paramList, exp2);
                }

                RequireSymbol(")", "')' required to close grouped equation.");

                return new GroupedEquation(startToken, exp);
            }

            return Expression_BasicTypes();
        }

        Expression Expression_BasicTypes()
        {
            if (AcceptKeyword("true", "false"))
                return new Bool(token, bool.Parse(token.Value));
            else if (AcceptString())
                return new StringType(token, token.Value);
            else if (AcceptNumber())
                return new Number(token, ((TokenNumber)token).Number);
            else if (AcceptKeyword("null"))
                return new Null(token);
            else if (AcceptKeyword("this"))
                return new This(token);
            else if (AcceptSymbol("..."))
                return new Vararg(token);

            return Expression_Table();
        }

        Expression Expression_Table()
        {
            if (AcceptSymbol("{"))
            {
                Token startToken = token;

                List<Tuple<Expression, Expression>> values = new List<Tuple<Expression, Expression>>();

                bool foundComma = true;

                while (!AcceptSymbol("}"))
                {
                    if (!foundComma)
                        ThrowException("',' expected after table value.", ExceptionPosition.TokenEnd);

                    Expression key = null;

                    if (AcceptSymbol("["))
                    {
                        key = Expression();
                        RequireSymbol("]", "']' missing to close table key.");

                        RequireSymbol("=", "'=' required for table index.");

                        var value = Expression("Table value expected.");

                        values.Add(new Tuple<Expression, Expression>(key, value));
                    }
                    else
                    {
                        if (AcceptIdentifier())
                        {
                            key = new StringType(token, token.Value);

                            if (!AcceptSymbol("="))
                                PrevToken();
                            else
                            {
                                var keyValue = Expression("Table value expected after '='.");

                                values.Add(new Tuple<Expression, Expression>(key, keyValue));

                                foundComma = AcceptSymbol(",");

                                continue;
                            }
                        }

                        var value = Expression("Table value expected.");

                        values.Add(new Tuple<Expression, Expression>(null, value));
                    }

                    foundComma = AcceptSymbol(",");
                }

                return new Table(startToken, values.ToArray());
            }

            return Expression_AnonymousFunction();
        }

        Expression Expression_AnonymousFunction()
        {
            if (AcceptKeyword("function"))
            {
                var funcToken = token;

                RequireSymbol("(", "'(' required to specify function parameters.");

                List<Tuple<Token, string, Expression>> funcParameterList = FunctionParameterList();

                RequireSymbol(")", "')' required to close function parameters declaration.");

                if (AcceptSymbol("=>"))
                {
                    var exp = Expression("Lambda function return value expected.");

                    return new AnonymousLambdaFunction(funcToken, funcParameterList, exp);
                }

                var seq = Block(false);

                return new AnonymousFunction(funcToken, funcParameterList, seq);

            }

            return Expression_Variable();
        }

        Expression Expression_Variable()
        {
            if (AcceptIdentifier())
                return new Variable(token, token.Value);

            return Expression_Error();
        }

        Expression Expression_Error()
        {
            if (errorOnNoExpression)
            {
                NextToken();
                ThrowException($"{message} Got '{token.Value}'.");
            }

            return null;
        }

        /// <summary>
        /// Returns a list of expressions separated by comma or empty list, if no expressions found.
        /// </summary>
        internal List<Expression> ExpressionList()
        {
            List<Expression> list = new List<Expression>();

            do
            {
                var exp = Expression(errorOnNoExpression: list.Count != 0);
                if (exp == null && list.Count == 0)
                    return list;

                list.Add(exp);
            } while (AcceptSymbol(","));

            return list;
        }

        /// <summary>
        /// Throws exception, if expression isn't an instance of NamedVariable.
        /// </summary>
        void EnsureNamed(Expression expression)
        {
            if (!(expression is NamedVariable))
                ThrowException("Value cannot be referenced. Use a variable.", expression.Token.Line, expression.Token.Character);
        }

        /// <summary>
        /// Returns named variable.
        /// </summary>
        internal NamedVariable NamedVariable()
        {
            var exp = Expression_AfterValueOperations(true);

            EnsureNamed(exp);

            return (NamedVariable)exp;
        }
    }
}
