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
        void Pass()
        {
            while (NextChar())
            {
                if (!SkipWhitespace())
                    break;

                // Keywords and identifiers
                if (AcceptPattern(@"[a-zA-Z_][a-zA-Z0-9_]*"))
                {
                    if (Specification.IsKeyword(patternMatch.Value))
                        PushToken(new TokenKeyword(), patternMatch.Value);
                    else if (Specification.IsSpecialToken(patternMatch.Value))
                        PushToken(new TokenSpecial(), patternMatch.Value);
                    else
                        PushToken(new TokenIdentifier(), patternMatch.Value);
                }

                // Hex number
                else if (AcceptPattern(@"0x([0-9a-fA-F]+)"))
                    PushToken(new TokenNumber(Convert.ToInt32(patternMatch.Groups[1].Value, 16)),
                        Convert.ToInt32(patternMatch.Groups[1].Value, 16).ToString());

                // Numbers // TODO: Number _ separator
                else if (AcceptPattern(@"[0-9]+\.[0-9]+") || AcceptPattern(@"[0-9]+"))
                    PushToken(new TokenNumber(double.Parse(patternMatch.Value.Replace('.', ','))), patternMatch.Value);

                // Multi-line strings
                else if (AcceptPattern("@\""))
                {
                    string str = "";
                    int startLine = line, lastLine = line;
                    while (NextChar() && currentChar != '"')
                    {
                        if (line != lastLine)
                            str += "\\n";

                        str += currentChar;

                        if (currentChar == '\\')
                        {
                            NextChar();
                            str += currentChar;
                        }

                        lastLine = line;
                    }
                    if (!AcceptPattern("\""))
                        ThrowException("Unexpected end of file.");

                    PushToken(new TokenString(), str);
                }

                // Single-line strings
                else if (AcceptPattern("\""))
                {
                    string str = "";
                    int startLine = line;
                    while (NextChar() && currentChar != '"')
                    {
                        str += currentChar;

                        if (currentChar == '\\')
                        {
                            NextChar();
                            str += currentChar;
                        }
                    }
                    if (!AcceptPattern("\""))
                        ThrowException("Unexpected end of file.");

                    if (line != startLine)
                        ThrowException("Line break in single-line string. For multi-line string use @ prefix.");

                    PushToken(new TokenString(), str);
                }

                // Single-line documentation comments
                else if (AcceptPattern(@"///"))
                {
                    string value = currentLine.Substring(character + 1);
                    character = currentLine.Length;
                    PushToken(new TokenDocumentationComment(), value);
                }

                // Multi-line documentation comments
                else if (AcceptPattern(@"/\*\*"))
                {
                    string value = "";
                    int lines = 0;

                    while (true)
                    {
                        if (lines > 0)
                            value += '\n';

                        Match match;
                        if ((match = new Regex(@"\*/").Match(currentLine.Substring(character + (lines == 0 ? 1 : 0)))).Success)
                        {
                            value += currentLine.Substring(character + (lines == 0 ? 1 : 0),
                                match.Index);
                            character += (lines == 0 ? 2 : 0) + match.Index + 1;
                            break;
                        }
                        else
                        {
                            value += currentLine.Substring(character + (lines == 0 ? 1 : 0));
                        }

                        if (!NextLine())
                            ThrowException("Unexpected end of file");

                        lines++;
                    }

                    PushToken(new TokenDocumentationComment(), value);
                }

                // Single-line comments
                else if (AcceptPattern(@"//"))
                {
                    string value = currentLine.Substring(character + 1);
                    character = currentLine.Length;
                    PushToken(new TokenComment(), value);
                }

                // Multi-line comments
                else if (AcceptPattern(@"/\*"))
                {
                    string value = "";
                    int lines = 0;

                    while (true)
                    {
                        if (lines > 0)
                            value += '\n';

                        Match match;
                        if (
                            (match = new Regex(@"\*/").Match(currentLine.Substring(character + (lines == 0 ? 1 : 0)))).Success)
                        {
                            value += currentLine.Substring(character + (lines == 0 ? 1 : 0),
                                match.Index);
                            character += (lines == 0 ? 1 : 0) + match.Index + 1;
                            break;
                        }
                        else
                        {
                            value += currentLine.Substring(character + (lines == 0 ? 1 : 0));
                        }

                        if (!NextLine())
                            ThrowException("Unexpected end of file");

                        lines++;
                    }

                    PushToken(new TokenComment(), value);
                }

                // Triple character symbols
                else if (AcceptPattern(@"[^a-zA-Z0-9\s]{3}"))
                    if (Specification.IsSymbol(patternMatch.Value))
                        PushToken(new TokenSymbol(), patternMatch.Value);
                    else
                    {
                        PreviousPattern();
                        if (AcceptPattern(@"[^a-zA-Z0-9\s]{2}") && Specification.IsSymbol(patternMatch.Value))
                            PushToken(new TokenSymbol(), patternMatch.Value);
                        else
                        {
                            PreviousPattern();
                            if (AcceptPattern(@"[^a-zA-Z0-9\s]") && Specification.IsSymbol(patternMatch.Value))
                                PushToken(new TokenSymbol(), patternMatch.Value);
                            else
                                ThrowException($"Unknown symbol ({currentChar}).");
                        }
                    }

                // Double character symbols
                else if (AcceptPattern(@"[^a-zA-Z0-9\s]{2}"))
                    if (Specification.IsSymbol(patternMatch.Value))
                        PushToken(new TokenSymbol(), patternMatch.Value);
                    else
                    {
                        PreviousPattern();
                        if (AcceptPattern(@"[^a-zA-Z0-9\s]") && Specification.IsSymbol(patternMatch.Value))
                            PushToken(new TokenSymbol(), patternMatch.Value);
                        else
                            ThrowException($"Unknown symbol ({currentChar}).");
                    }

                // Single character symbols
                else if (AcceptPattern(@"[^a-zA-Z0-9\s]") && Specification.IsSymbol(patternMatch.Value))
                    PushToken(new TokenSymbol(), patternMatch.Value);

                // Unknown characters or whitespaces - the thing at the top doesn't work properly sometimes
                else if (!AcceptPattern(@"\s+"))
                    ThrowException($"Unknown character ({currentChar}).");
            }
        }
    }
}
