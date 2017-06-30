using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.SyntaxAnalyzer
{
    public partial class SyntaxAnalyzer
    {
        public Statement ParseClass(Token classToken, bool local)
        {
            var name = RequireIdentifier("Class name expected.").Value;

            string baseClass = null;

            if (AcceptSymbol(":"))
                baseClass = RequireIdentifier("Base class expected.").Value;

            RequireSymbol("{", "'{' required to open class definition.");

            var methods = new List<Tuple<string, Tuple<Token, string, Expression>[], Sequence>>();
            var fields = new List<Tuple<string, Expression>>();

            while (!AcceptSymbol("}"))
            {
                if (AcceptKeyword("function"))
                {
                    var func = ParseClassMethod();
                    methods.Add(func);
                }
                else if (AcceptKeyword("var"))
                {
                    var identList = IdentifierList();

                    if (identList.Count == 0)
                        ThrowException("Variable name expected.", ExceptionPosition.TokenEnd);

                    Expression[] expArray = { };

                    if (AcceptSymbol("="))
                    {
                        expArray = ExpressionList().ToArray();

                        if (expArray.Length == 0)
                            ThrowException("At least one value expected, after '='.", ExceptionPosition.TokenEnd);

                        if (expArray.Length > identList.Count)
                            ThrowException($"Too many values after variable declaration. Expected: {identList.Count}, got: {expArray.Length}.", ExceptionPosition.TokenBeginning);
                    }

                    RequireEndToken("';' required to end the field declaration.");

                    for (var i = 0; i < identList.Count; i++)
                    {
                        var ident = identList[i];
                        fields.Add(new Tuple<string, Expression>(ident.Item2, expArray.Length >= i + 1 ? expArray[i] : null));
                    }
                }
                else
                    return Statement_Error();
            }

            return new Class(classToken, local, name, baseClass, methods.ToArray(), fields.ToArray());
        }

        public Tuple<string, Tuple<Token, string, Expression>[], Sequence> ParseClassMethod()
        {
            if (!AcceptKeyword("this"))
                RequireIdentifier("Method name expected.");

            var name = token.Value;

            RequireSymbol("(", "'(' required to specify function parameters.");

            List<Tuple<Token, string, Expression>> funcParameterList = FunctionParameterList();

            RequireSymbol(")", "')' required to close function parameters declaration.");

            if (AcceptSymbol("=>"))
            {
                var exp = Expression();
                RequireSymbol(";", "';' required to close lambda function declaration.");
                return new Tuple<string, Tuple<Token, string, Expression>[], Sequence>(name, funcParameterList.ToArray(), new Sequence(null, new Node[] { new Return(null, new[] { exp }) }));
            }

            var seq = Block(false);

            return new Tuple<string, Tuple<Token, string, Expression>[], Sequence>(name, funcParameterList.ToArray(), seq);
        }
    }
}
