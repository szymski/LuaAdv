using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler
{
    public static class Specification
    {
        public static string Version => "0.0.1";

        public static Hashtable Keywords = new Hashtable()
        {
            { "var", "variable" },
            { "local", "local variable" },

            { "if", "if statement" },
            { "else", "else statement" },

            { "while", "while loop" },
            { "for", "for loop" },
            { "foreach", "foreach loop" },
            { "break", "loop break" },
            { "continue", "loop continue" },
            { "in", "foreach in" },

            { "switch", "switch" },
            { "case", "switch case" },
            { "default", "switch default case" },

            { "function", "function" },
            { "return", "return" },

            { "this", "this" },

            { "true", "true" },
            { "false", "false" },
            { "null", "null" },

            { "class", "class" },
            { "partial", "partial" },
            { "public", "public" },
            { "new", "new" },
            { "is", "is" },
        };

        public static Hashtable Symbols = new Hashtable()
        {
            // MATH:

		    { "+", "addition" },
            { "-", "subtract" },
            { "*", "multiplier" },
            { "/", "division" },
            { "%", "modulus" },
            { "^", "power" },
            { "=", "assign" },
            { "+=", "increase" },
            { "-=", "decrease" },
            { "*=", "multiplier" },
            { "/=", "division" },
            { "^=", "power" },
            { "%=", "power" },
            { "++", "increment" },
            { "--", "decrement" },

	        // COMPARISON:

		    { "==", "equal" },
            { "!=", "unequal" },
            { "<", "less" },
            { "<=", "less or equal" },
            { ">", "greater" },
            { ">=", "greater or equal" },

	        // BITWISE:

		    { "&", "and" },
            { "|", "or" },
            { "^^", "xor" },
            { ">>", "shift right" },
            { "<<", "shift left" },
            { "~", "negate" },

	        // CONDITION:

		    { "!", "not" },
            { "&&", "and" },
            { "||", "or" },

	        // SYMBOLS:
		
            { "?", "ternary" },
            { ":", "colon" },
            { "??", "null propagation" },
            { ";", "semicolon" },
            { ",", "comma" },
            { "#", "length" },
            { ".", "period" },

	        // BRACKETS:

		    { "(", "left parenthesis" },
            { ")", "right parenthesis" },
            { "{", "left curly bracket" },
            { "}", "right curly bracket" },
            { "[", "left square bracket" },
            { "]", "right square bracket" },

            // STRINGS:
            { "@", "multi-line string prefix" },
            { "..", "string join" },
            { "..=", "string assignment join" },

	        // MISC:

		    { "=>", "lambda" },
            { "...", "varargs" },
        };

        public static Hashtable SpecialTokens = new Hashtable()
        {
            { "__FILE__", "File" },
            { "__LINE__", "Line" },
            { "__FUNCTION__", "Function" },

            { "__DATE__", "Date" },
            { "__TIME__", "Time" },
            { "__TIMESTAMP__", "Date and time" },
        };

        public static bool IsKeyword(string word) => Keywords.ContainsKey(word);
        public static bool IsSymbol(string str) => Symbols.ContainsKey(str);
        public static bool IsSpecialToken(string str) => SpecialTokens.ContainsKey(str);
    }
}
