﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.SemanticAnalyzer1
{
    public class Scope
    {
        public Scope Parent { get; }

        public List<FunctionInformation> Functions = new List<FunctionInformation>();

        public Scope(Scope parent = null)
        {
            Parent = parent;
        }
    }
}
