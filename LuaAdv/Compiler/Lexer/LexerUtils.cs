using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Lexer
{
    public partial class Lexer
    {
        string[] inputLines;

        int line = -1, character = 1;

        char currentChar = '\0';
        string currentLine = "";
        Match patternMatch = null;

        struct PatternMatchSave
        {
            public int line, position;
            public Match patternMatch;
        }

        Stack<PatternMatchSave> patternStack = new Stack<PatternMatchSave>();

        public List<Token> Output { get; } = new List<Token>();

        public Lexer(string input)
        {
            inputLines = input.Replace("\r", "").Split('\n');
            Pass();
        }

        bool NextChar()
        {
            character++;

            if (character >= currentLine.Length) // Go to next line, if current one has ended
            {
                character = 0;
                do
                {
                    line++;
                } while (line < inputLines.Length && inputLines[line].Length == 0);
                if (line >= inputLines.Length) // Return false at the end of file
                    return false;
                currentLine = inputLines[line];
            }

            currentChar = currentLine[character];
            return true;
        }

        bool NextLine()
        {
            character = 0;

            do
            {
                line++;
            } while (line < inputLines.Length && inputLines[line].Length == 0);
            if (line >= inputLines.Length) // Return false at the end of file
                return false;

            currentLine = inputLines[line];
            currentChar = currentLine[character];

            return true;
        }

        Regex whitespaceRegex = new Regex(@"\S+");

        bool SkipWhitespace()
        {
            Match match = null;

            while (!(match = whitespaceRegex.Match(currentLine.Substring(character))).Success)
                return NextLine(); // Go to next line, if the current one doesn't have any characters

            character += match.Index - 1;
            NextChar();

            return true;
        }

        bool AcceptPattern(string pattern)
        {
            var regex = new Regex(pattern);
            var match = regex.Match(currentLine.Substring(character));
            if (match.Success && match.Index == 0)
            {
                patternStack.Push(new PatternMatchSave()
                {
                    line = line,
                    position = character,
                    patternMatch = patternMatch
                });
                patternMatch = match;
                character += patternMatch.Length - 1;

                return true;
            }

            return false;
        }

        void PreviousPattern()
        {
            var save = patternStack.Pop();
            line = save.line;
            character = save.position;
            patternMatch = save.patternMatch;
        }

        void PushToken(Token token, string value, int line, int character, int endLine, int endCharacter)
        {
            token.Value = value;

            token.Line = line;
            token.Character = character;

            token.EndLine = endLine;
            token.EndCharacter = endCharacter;

            Output.Add(token);
        }

        void PushToken(Token token, string value)
        {
            PushToken(token, value, line, patternStack.Last().position, line, line + value.Length);   
        }

        void ThrowException(string message)
        {
            throw new LexerException(message, line + 1, character + 1);
        }
    }
}
