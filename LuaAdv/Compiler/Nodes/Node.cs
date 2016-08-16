using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.Nodes
{
    public abstract class Node
    {
        public void PrintTree(int depth)
        {
            var tabStr = "";
            for (int i = 0; i < depth; i++)
                tabStr += "    ";


            Debug.WriteLine($"{tabStr}└{GetType()}:");

            foreach (var child in Children ?? new Node[0])
                child.PrintTree(depth + 1);
        }

        public abstract Token Token { get; }
        public virtual Node[] Children => null;

        public Node Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this as dynamic);
        }

        public Node this[int key] => Children[key];
    }
}
