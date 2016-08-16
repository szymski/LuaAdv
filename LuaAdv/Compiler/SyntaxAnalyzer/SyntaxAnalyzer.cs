using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Statements;

[assembly: InternalsVisibleTo("LuaAdvTests")] // TODO: Move this somewhere else

namespace LuaAdv.Compiler.SyntaxAnalyzer
{
    public partial class SyntaxAnalyzer
    {
        void Pass()
        {
            OutputNode = Sequence();
        }

        Sequence Sequence(Token startToken = null, string endToken = null)
        {
            List<Node> nodes = new List<Node>();

            bool brk = false;
            while (!IsEof())
            {
                if (AcceptToken<TokenSymbol>(endToken))
                {
                    brk = true;
                    break;
                }

                nodes.Add(Statement());
            }

            if (endToken != null && !brk)
                ThrowException($"'{endToken}' expected to close block.");

            return new Sequence(startToken, nodes.ToArray());
        }

        Sequence Block(bool allowSingleStatement = true)
        {
            if (AcceptSymbol("{"))
            {
                var seq = Sequence(token, "}");
                return seq;
            }

            if (allowSingleStatement)
                return new Sequence(null, new[] { Statement() });

            ThrowException("Block of code expected.");
            return null;
        }

        /// <summary>
        /// Returns a list of identifiers with default values, if there are any.
        /// </summary>
        internal List<Tuple<Token, string, Expression>> FunctionParameterList()
        {
            List<Tuple<Token, string, Expression>> list = new List<Tuple<Token, string, Expression>>();

            do
            {
                var identAccepted = AcceptIdentifier();
                if (!identAccepted && list.Count == 0)
                    return list;

                var identToken = token;
                var ident = token.Value;

                Expression defaultValue = null;

                if (AcceptSymbol("="))
                    defaultValue = Expression("Parameter default name expected.");

                list.Add(new Tuple<Token, string, Expression>(identToken, ident, defaultValue));
            } while (AcceptSymbol(","));

            return list;
        }

        /// <summary>
        /// Returns a list of expressions that inherit from NamedVariable.
        /// </summary>
        internal List<NamedVariable> NamedVariableList(bool returnNullIfNotNamed = false)
        {
            int startIndex = tokenIndex;

            List<NamedVariable> list = new List<NamedVariable>();

            var before = requiresNamedVariable;
            requiresNamedVariable = true;

            var expList = ExpressionList();

            requiresNamedVariable = before;

            foreach (var expression in expList)
                if (expression is NamedVariable)
                    list.Add((NamedVariable) expression);
                else
                {
                    if (returnNullIfNotNamed)
                    {
                        tokenIndex = startIndex;
                        return null;
                    }

                    ThrowException("Variable cannot be referenced.");
                }

            return list;
        }

        /// <summary>
        /// Return a list of identifiers. Can return empty list, if no identifiers found.
        /// </summary>
        internal List<Tuple<Token, string>> IdentifierList()
        {
            List<Tuple<Token, string>> list = new List<Tuple<Token, string>>();

            do
            {
                var identAccepted = AcceptIdentifier();
                if (!identAccepted)
                    if(list.Count == 0)
                        return list;
                    else
                        ThrowException("Identifier expected.", ExceptionPosition.TokenEnd);

                var identToken = token;
                var ident = token.Value;

                list.Add(new Tuple<Token, string>(identToken, ident));
            } while (AcceptSymbol(","));

            return list;
        }
    }
}
