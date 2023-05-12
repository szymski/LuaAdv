using LuaAdv.Compiler.Nodes.Expressions;
using NUnit.Framework;
using Assert=Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace LuaAdvTests; 

public static class AssertExtensions {
    // public static bool IsType<T>(this Assert assert, object obj, out T ret) where T : class {
    //     Assert.IsInstanceOfType<T>(obj);
    //     ret = (T)obj; 
    //     return true;
    // }
    
    public static void IsType<T>(this Assert assert, out T ret, object obj) where T : class {
        Assert.IsInstanceOfType<T>(obj);
        ret = (T)obj;
    }
    
    public static void IsVariable(this Assert assert, object obj, string name = null) {
        Assert.IsInstanceOfType<Variable>(obj);
        if (name is not null)
        {
            Assert.AreEqual(name, ((Variable)obj).name);
        }
    }
}