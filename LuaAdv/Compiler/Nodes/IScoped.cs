using LuaAdv.Compiler.SemanticAnalyzer1;

namespace LuaAdv.Compiler.Nodes; 

public interface IScoped {
    Scope scope { get; set; }
}