using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Statements
{
    public class ClassMethod : Statement
    {
        /// <summary>
        /// Only for reference. Class node stays in the AST and should not be analyzed again.
        /// </summary>
        public Class classNode;
        public StatementMethodDeclaration method;

        public override Token Token => method.funcToken;
        public override Node[] Children => new[] { method };

        public ClassMethod(Class classNode, StatementMethodDeclaration method)
        {
            this.classNode = classNode;
            this.method = method;
        }
    }
}
