using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes;

namespace LuaAdv.Compiler.SyntaxAnalyzer
{
    public partial class SyntaxAnalyzer
    {
        List<Token> tokens;

        Token token = null;
        public int tokenIndex = -1;

        public Node OutputNode { get; set; }

        public SyntaxAnalyzer(List<Token> tokens)
        {
            this.tokens = tokens;
            Pass();
        }

        public SyntaxAnalyzer(List<Token> tokens, bool debug)
        {
            this.tokens = tokens;
        }

        bool NextToken()
        {
            tokenIndex++;
            if (tokenIndex >= tokens.Count)
                return false;

            token = tokens[tokenIndex];

            return true;
        }

        void PrevToken()
        {
            tokenIndex--;
            if (tokenIndex < 0)
                return;

            token = tokens[tokenIndex];
        }

        bool AcceptToken<T>(string value)
        {
            if (tokenIndex + 1 >= tokens.Count || !(tokens[tokenIndex + 1] is T) || tokens[tokenIndex + 1].Value != value)
                return false;

            token = tokens[++tokenIndex];

            return true;
        }

        bool AcceptToken<T>()
        {
            if (tokenIndex + 1 >= tokens.Count || !(tokens[tokenIndex + 1] is T))
                return false;

            token = tokens[++tokenIndex];

            return true;
        }

        bool AcceptKeyword(params string[] values)
        {
            foreach (var value in values)
                if (AcceptKeyword(value)) return true;

            return false;
        }

        bool AcceptSymbol(params string[] values)
        {
            foreach (var value in values)
                if (AcceptSymbol(value)) return true;

            return false;
        }

        bool AcceptSymbolExcept(params string[] except)
        {
            if (AcceptSymbol())
            {
                if (except.Contains(token.Value))
                {
                    PrevToken();
                    return false;
                }
                return true;
            }
            return false;
        }

        internal bool AcceptKeyword(string value) => AcceptToken<TokenKeyword>(value);
        internal bool AcceptSymbol(string value) => AcceptToken<TokenSymbol>(value);

        internal bool AcceptIdentifier() => AcceptToken<TokenIdentifier>();
        internal bool AcceptString() => AcceptToken<TokenString>();
        internal bool AcceptNumber() => AcceptToken<TokenNumber>();

        Token RequireSymbol(string value, string message)
        {
            if (!AcceptSymbol(value))
                ThrowException(message, token.Line, token.Character + token.Value.Length);

            return token;
        }

        Token RequireSymbol(string value)
        {
            if (!AcceptSymbol(value))
                ThrowException($"({value}) expected.", token.Line, token.Character + token.Value.Length);

            return token;
        }

        Token RequireKeyword(string value, string message)
        {
            if (!AcceptKeyword(value))
                ThrowException(message, token.Line, token.Character + token.Value.Length);

            return token;
        }

        Token RequireKeyword(string value)
        {
            if (!AcceptKeyword(value))
                ThrowException($"({value}) expected.", token.Line, token.Character + token.Value.Length);

            return token;
        }

        Token RequireIdentifier(string message)
        {
            if (!AcceptIdentifier())
                ThrowException(message, token.Line, token.Character + token.Value.Length);

            return token;
        }

        Token RequireIdentifier()
        {
            if (!AcceptIdentifier())
                ThrowException($"Identifier expected.", token.Line, token.Character + token.Value.Length);

            return token;
        }

        bool IsEof() => tokenIndex >= tokens.Count - 1;
    }
}
