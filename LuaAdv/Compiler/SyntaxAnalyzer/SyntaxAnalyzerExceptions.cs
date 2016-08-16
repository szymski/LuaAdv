using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.SyntaxAnalyzer
{
    public partial class SyntaxAnalyzer
    {
        enum ExceptionPosition
        {
            TokenBeginning,
            TokenEnd
        }

        void ThrowException(string message, int line, int position)
        {
            throw new SyntaxAnalyzerException(message, line + 1, position + 1);
        }

        void ThrowException(string message, ExceptionPosition position)
        {
            if (position == ExceptionPosition.TokenBeginning)
                throw new SyntaxAnalyzerException(message, token?.Line + 1 ?? 0, token?.Character + 1 ?? 0);
            else
                throw new SyntaxAnalyzerException(message, token?.Line + 1 + (token?.Value.Length ?? 0) ?? 0, token?.Character + 1 ?? 0);
        }

        void ThrowException(string message)
        {
            throw new SyntaxAnalyzerException(message, token?.Line + 1 ?? 0, token?.Character + 1 ?? 0);
        }
    }
}
