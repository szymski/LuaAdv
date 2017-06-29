using System;
using LuaAdv.Compiler.Lexer;
using LuaAdv.Compiler.SemanticAnalyzer1;
using LuaAdv.Compiler.SyntaxAnalyzer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuaAdvTests
{
    [TestClass]
    public class SemanticAnalyzer1Tests
    {
        public SemanticAnalyzer1 Analyze(string code)
        {
            Lexer lexer = new Lexer(code);
            SyntaxAnalyzer syntaxAnalyzer = new SyntaxAnalyzer(lexer.Output);
            SemanticAnalyzer1 semanticAnalyzer1 = new SemanticAnalyzer1(syntaxAnalyzer.OutputNode);
            syntaxAnalyzer.OutputNode.Accept(semanticAnalyzer1);

            return semanticAnalyzer1;
        }

        [TestMethod]
        public void test_function_informations()
        {
            var analyzer = Analyze("  function simpleFunc(param1, param2) { }");

            var funcInfo = analyzer.MainScope.Functions[0];

            Assert.AreEqual(0, funcInfo.Line);
            Assert.AreEqual(2, funcInfo.Character);
            Assert.AreEqual("simpleFunc", funcInfo.Name);
            Assert.AreEqual("param1", funcInfo.ParameterList[0].Item1);
            Assert.AreEqual("param2", funcInfo.ParameterList[1].Item1);
        }
    }
}
