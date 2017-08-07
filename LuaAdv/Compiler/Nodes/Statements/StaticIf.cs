using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Statements;

namespace LuaAdv.Compiler.Nodes
{
    public class StaticIf : Statement
    {
        public List<Tuple<Token, Expression, Sequence>> ifs;

        public StaticIf(List<Tuple<Token, Expression, Sequence>> ifs)
        {
            this.ifs = ifs;
        }

        public override Token Token => ifs[0].Item1;

        public override Node[] Children => ifs.SelectMany(a => new Node[] {a.Item2, a.Item3}).ToArray();
    }
}
