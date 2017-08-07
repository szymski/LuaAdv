using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler
{
    public class CompilerException : Exception
    {
        public override string Message { get; }
        public int Line { get; }
        public int Character { get; }

        public CompilerException(string message, Token token)
        {
            Message = message;
            Line = token.Line + 1;
            Character = token.Character + 1;
        }

        public CompilerException(string message, int line, int character)
        {
            Message = message;
            Line = line;
            Character = character;
        }
    }

    public class LexerException : CompilerException
    {
        public LexerException(string message, int line, int character) : base(message, line, character)
        {
        }
    }

    public class SyntaxAnalyzerException : CompilerException
    {
        public SyntaxAnalyzerException(string message, int line, int character) : base(message, line, character)
        {
            
        }
    }
}
