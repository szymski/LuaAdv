using LuaAdv.Compiler.Nodes;

namespace LuaAdv.Compiler; 

public interface IAstVisitorProxy {
    Node ProxyBefore(Node node);
    Node ProxyAfter(Node node);
}