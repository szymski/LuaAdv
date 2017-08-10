using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler
{
    public abstract class Token
    {
        public string Value { get; set; }

        public int Line { get; set; }
        public int Character { get; set; }

        public int EndLine { get; set; }
        public int EndCharacter { get; set; }
    }

    public class TokenKeyword : Token { }

    public class TokenIdentifier : Token { }

    public class TokenSymbol : Token { }

    public class TokenString : Token { }

    public class TokenNumber : Token
    {
        public double Number { get; set; }

        public TokenNumber(double number)
        {
            Number = number;
        }
    }

    public class TokenSpecial : Token { }

    public class TokenComment : Token { }

    public class TokenDocumentationComment : Token { }


}
