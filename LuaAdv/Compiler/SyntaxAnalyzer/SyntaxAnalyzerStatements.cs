﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Expressions.BasicTypes;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.SyntaxAnalyzer
{
    public partial class SyntaxAnalyzer
    {
        private string endToken = ";";

        internal Statement Statement(string endToken = ";")
        {
            var prev = endToken;
            this.endToken = endToken;

            var statement = Statement_Comment();

            endToken = prev;

            return statement;
        }

        Statement Statement_Comment()
        {
            if (AcceptToken<TokenComment>())
                return new CommentNode(token);

            return Statement_DocumentationComment();
        }

        Statement Statement_DocumentationComment()
        {
            if (AcceptToken<TokenDocumentationComment>())
                return new DocumentationCommentNode(token);

            return Statement_If();
        }

        Statement Statement_If()
        {
            List<Tuple<Token, Expression, Sequence>> ifs = new List<Tuple<Token, Expression, Sequence>>();

            while (ifs.Count == 0 ? AcceptKeyword("if") : AcceptKeyword("if", "else"))
            {
                var ifToken = token;

                if (token.Value == "if")
                {
                    if (ifs.Count > 0)
                    {
                        PrevToken();
                        break;
                    }

                    RequireSymbol("(");
                    var cond = Expression();
                    RequireSymbol(")");
                    ifs.Add(new Tuple<Token, Expression, Sequence>(ifToken, cond, Block()));
                }
                else
                {
                    if (AcceptKeyword("if"))
                    {
                        RequireSymbol("(");
                        var cond = Expression();
                        RequireSymbol(")");
                        ifs.Add(new Tuple<Token, Expression, Sequence>(ifToken, cond, Block()));
                        continue;
                    }

                    ifs.Add(new Tuple<Token, Expression, Sequence>(ifToken, null, Block()));
                    break;
                }
            }

            if (ifs.Count > 0)
                return new If(ifs);

            return Statement_Static_If();
        }

        Statement Statement_Static_If()
        {
            if (AcceptKeyword("static"))
            {
                List<Tuple<Token, Expression, Sequence>> ifs = new List<Tuple<Token, Expression, Sequence>>();

                while (ifs.Count == 0 ? AcceptKeyword("if") : AcceptKeyword("if", "else"))
                {
                    var ifToken = token;

                    if (token.Value == "if")
                    {
                        if (ifs.Count > 0)
                        {
                            PrevToken();
                            break;
                        }

                        RequireSymbol("(");
                        var cond = Expression();
                        RequireSymbol(")");
                        ifs.Add(new Tuple<Token, Expression, Sequence>(ifToken, cond, Block()));
                    }
                    else
                    {
                        if (AcceptKeyword("if"))
                        {
                            RequireSymbol("(");
                            var cond = Expression();
                            RequireSymbol(")");
                            ifs.Add(new Tuple<Token, Expression, Sequence>(ifToken, cond, Block()));
                            continue;
                        }

                        ifs.Add(new Tuple<Token, Expression, Sequence>(ifToken, null, Block()));
                        break;
                    }
                }

                if (ifs.Count == 0)
                    ThrowException("Static if statement expected.");

                return new StaticIf(ifs);
            }

            return Statement_WhileLoop();
        }

        Statement Statement_WhileLoop()
        {
            if (AcceptKeyword("while"))
            {
                var whileToken = token;

                RequireSymbol("(");
                var cond = Expression();
                RequireSymbol(")");
                var seq = Block();

                return new While(whileToken, cond, seq);
            }

            return Statement_ForLoop();
        }

        Statement Statement_ForLoop()
        {
            if (AcceptKeyword("for"))
            {
                var forToken = token;

                RequireSymbol("(");

                var init = Statement();

                Token condToken = token;
                Expression cond = null;
                if (!AcceptSymbol(";"))
                {
                    cond = Expression();
                    RequireSymbol(";");
                }

                Token afterToken = token;
                Statement after = null;
                if (!AcceptSymbol(")"))
                {
                    after = Statement("");
                    RequireSymbol(")");
                }

                var seq = Block();

                return new For(forToken, init, cond, after, seq);
            }

            return Statement_ForeachLoop();
        }

        Statement Statement_ForeachLoop()
        {
            if (AcceptKeyword("foreach"))
            {
                var foreachToken = token;

                RequireSymbol("(", "'(' expected to open foreach.");

                RequireKeyword("var", "Variable expected.");
                string keyName = null;
                string varName = RequireIdentifier("Variable name expected.").Value;
                if (AcceptSymbol(","))
                {
                    keyName = varName;
                    varName = RequireIdentifier("Key variable name expected.").Value;
                }

                RequireKeyword("in", "'in' required for foreach loop.");

                var table = Expression();

                RequireSymbol(")");

                var seq = Block();

                return new Foreach(foreachToken, keyName, varName, table, seq);
            }

            return Statement_BreakContinueReturn();
        }

        Statement Statement_BreakContinueReturn()
        {
            if (AcceptKeyword("break"))
            {
                var breakToken = token;

                RequireSymbol(";");

                return new Break(breakToken);
            }

            if (AcceptKeyword("continue"))
            {
                var continueToken = token;

                RequireSymbol(";");

                return new Continue(continueToken);
            }

            if (AcceptKeyword("return"))
            {
                var returnToken = token;

                List<Expression> values = new List<Expression>();

                var expList = ExpressionList();

                RequireSymbol(";");

                return new Return(returnToken, expList.ToArray());
            }

            return Statement_Decorator();
        }

        Statement Statement_Decorator()
        {
            if (AcceptSymbol("@"))
            {
                var decoratorToken = token;

                var namedVariable = NamedVariable();

                Node[] parameters = { };

                if (AcceptSymbol("("))
                {
                    parameters = ExpressionList().Cast<Node>().ToArray();

                    RequireSymbol(")", "')' required to close decorator parameters declaration.");

                    //return new Decorator(decoratorToken, namedVariable, parameters);
                }

                SkipComments(false, false);

                if (AcceptSymbol("@"))
                {
                    PrevToken();
                    return new Decorator(decoratorToken, Statement_Decorator(), namedVariable, parameters);
                }

                if (AcceptKeyword("local"))
                {
                    if (AcceptKeyword("function"))
                    {
                        PrevToken();
                        PrevToken();
                        return new Decorator(decoratorToken, Statement_Function(), namedVariable, parameters);
                    }

                    if (AcceptKeyword("class"))
                    {
                        PrevToken();
                        PrevToken();
                        return new Decorator(decoratorToken, Statement_Class(), namedVariable, parameters);
                    }

                    ThrowException("Unexpected keyword 'local'.");
                }

                if (AcceptKeyword("function"))
                {
                    PrevToken();
                    return new Decorator(decoratorToken, Statement_Function(), namedVariable, parameters);
                }

                if (AcceptKeyword("class"))
                {
                    PrevToken();
                    return new Decorator(decoratorToken, Statement_Class(), namedVariable, parameters);
                }

                ThrowException("Function or class expected.");
            }

            return Statement_Function();
        }

        // TODO: Switch

        /// <summary>
        /// Parses function or method
        /// </summary>
        Statement Statement_Function()
        {
            bool local = AcceptKeyword("local");
            Token localToken = token;

            if (AcceptKeyword("function"))
            {
                var funcToken = token;

                NamedVariable name;

                if (local)
                {
                    RequireIdentifier("Local function name expected.");
                    name = new Variable(token, token.Value);
                }
                else
                    name = NamedVariable();

                if (AcceptSymbol(":"))
                {
                    if (local)
                        ThrowException("A method cannot be local.", localToken.Line, localToken.Character);

                    return Statement_Method(funcToken, name);
                }

                RequireSymbol("(", "'(' required to specify function parameters.");

                List<Tuple<Token, string, Expression>> funcParameterList = FunctionParameterList();

                RequireSymbol(")", "')' required to close function parameters declaration.");

                if (AcceptSymbol("=>"))
                {
                    var exp = Expression();
                    RequireEndToken("';' required to end lambda function declaration.");

                    return new StatementLambdaFunctionDeclaration(local, funcToken, name, funcParameterList, exp);
                }

                var seq = Block(false);

                return new StatementFunctionDeclaration(local, funcToken, name, funcParameterList, seq);
            }
            else if (local)
                PrevToken();

            return Statement_Class();
        }

        Statement Statement_Method(Token funcToken, NamedVariable tableName)
        {
            var name = RequireIdentifier("Method name expected").Value;

            RequireSymbol("(", "'(' required to specify function parameters.");

            List<Tuple<Token, string, Expression>> funcParameterList = FunctionParameterList();

            RequireSymbol(")", "')' required to close function parameters declaration.");

            if (AcceptSymbol("=>"))
            {
                var exp = Expression();

                return new StatementLambdaMethodDeclaration(funcToken, tableName, name, funcParameterList, exp);
            }

            var seq = Block(false);

            return new StatementMethodDeclaration(funcToken, tableName, name, funcParameterList, seq);
        }

        Statement Statement_Class()
        {
            var local = AcceptKeyword("local");

            if (AcceptKeyword("class"))
                return ParseClass(token, local);
            else if (local)
                PrevToken();

            return Statement_Enum();
        }

        Statement Statement_Enum()
        {
            if (AcceptKeyword("enum"))
            {
                var enumToken = token;
                var name = RequireIdentifier("Enum identifier expected.").Value;

                if (AcceptSymbol("{")) // TODO: Multiple value enum
                {
                    var values = new List<Tuple<string, Node>>();

                    int index = 0;
                    bool gotComma = true;
                    while (!AcceptSymbol("}"))
                    {
                        if(!gotComma)
                            ThrowException("Comma expected after enum key.");

                        var keyName = RequireIdentifier("Enum key expected.");

                        if (AcceptSymbol("="))
                        {
                            var keyValue = Expression("Enum value expected.");
                            values.Add(new Tuple<string, Node>(keyName.Value, keyValue));
                        }
                        else
                            values.Add(new Tuple<string, Node>(keyName.Value, new Number(keyName, index)));

                        index++;
                        gotComma = AcceptSymbol(",");
                    }

                    RequireEndToken("';' required to end variable declaration.");

                    return new MultiEnum(enumToken, name, values.ToArray());
                }

                RequireSymbol("=", "Enum value assignment expected.");

                var value = Expression("Enum value expected.");

                RequireEndToken("';' required to end enum declaration.");

                return new SingleEnum(enumToken, name, value);
            }

            return Statement_LocalVariablesDeclaration();
        }

        Statement Statement_LocalVariablesDeclaration()
        {
            if (AcceptKeyword("var"))
            {
                var identList = IdentifierList();

                if (identList.Count == 0)
                    ThrowException("Variable name expected.", ExceptionPosition.TokenEnd);

                Node[] expArray = { };

                if (AcceptSymbol("="))
                {
                    expArray = ExpressionList().Cast<Node>().ToArray();

                    if (expArray.Length == 0)
                        ThrowException("At least one value expected, after '='.", ExceptionPosition.TokenEnd);

                    if (expArray.Length > identList.Count)
                        ThrowException($"Too many values after variable declaration. Expected: {identList.Count}, got: {expArray.Length}.", ExceptionPosition.TokenBeginning);
                }

                RequireEndToken("';' required to end variable declaration.");

                return new LocalVariablesDeclaration(identList.ToArray(), expArray);
            }

            return Statement_GlobalVariablesDeclaration();
        }

        Statement Statement_GlobalVariablesDeclaration()
        {
            int startIndex = tokenIndex;

            var namedVariableList = NamedVariableList(true);

            if (namedVariableList?.Count != 0)
            {
                Node[] expArray = null;

                if (AcceptSymbol("="))
                {
                    expArray = ExpressionList().Cast<Node>().ToArray();

                    if (expArray.Length == 0)
                        ThrowException("At least one value expected, after '='.", ExceptionPosition.TokenEnd);

                    if (expArray.Length > namedVariableList.Count)
                        ThrowException(
                            $"Too many values after variable assignment. Expected: {namedVariableList.Count}, got: {expArray.Length}.",
                            ExceptionPosition.TokenBeginning);
                }
                else
                {
                    tokenIndex = startIndex;
                    return Statement_Expression();
                }

                RequireEndToken("';' required to end variable assignment.");

                return new GlobalVariablesDeclaration(namedVariableList.ToArray(), expArray);
            }

            tokenIndex = startIndex;

            return Statement_Expression();
        }

        Statement Statement_Expression()
        {
            var exp = Expression("", false);

            if (exp != null)
            {
                if (!(exp is IStatementable))
                    ThrowException("This operation has no effect and cannot be used as a statement.", exp.Token.Line, exp.Token.Character);

                RequireEndToken("';' required to end statement.");

                return new StatementExpression(exp);
            }

            return Statement_Null();
        }

        Statement Statement_Null()
        {
            if (AcceptSymbol(";"))
                return new NullStatement(token);

            return Statement_Error();
        }

        Statement Statement_Error()
        {
            NextToken();
            ThrowException($"Statement expected. Got '{token.Value}'.");
            return null;
        }

        void RequireEndToken(string message)
        {
            if (endToken != "")
                RequireSymbol(endToken, message);
        }
    }
}
