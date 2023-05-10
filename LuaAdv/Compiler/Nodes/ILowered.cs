namespace LuaAdv.Compiler.Nodes; 

/// <summary>
/// Every node that is supposed to be lowered (de-sugared) before going to the code generator, should implement this interface.
/// The compiler will throw if a node that's supposed to be lowered ends up in the code generator.
/// </summary>
public interface ILowered {
    
}