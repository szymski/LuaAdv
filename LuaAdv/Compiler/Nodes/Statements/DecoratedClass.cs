using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes.Statements
{
    public class DecoratedClass : Statement
    {
        public Class @class;
        public Node[] classSequence;
        public Node[] decoratorSequence;

        public DecoratedClass(Class @class, Node[] classSequence, Node[] decoratorSequence)
        {
            this.@class = @class;
            this.classSequence = classSequence;
            this.decoratorSequence = decoratorSequence;
        }

        public override Token Token => null;

        public override Node[] Children => (new Node[] { @class }).Concat(classSequence).Concat(decoratorSequence).ToArray();
    }
}
