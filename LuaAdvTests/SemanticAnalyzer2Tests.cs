using System;
using LuaAdv.Compiler.Lexer;
using LuaAdv.Compiler.Nodes;
using LuaAdv.Compiler.Nodes.Expressions.BasicTypes;
using LuaAdv.Compiler.Nodes.Statements;
using LuaAdv.Compiler.SemanticAnalyzer;
using LuaAdv.Compiler.SemanticAnalyzer1;
using LuaAdv.Compiler.SyntaxAnalyzer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuaAdvTests
{
    [TestClass]
    public class SemanticAnalyzer2Tests
    {
        public SemanticAnalyzer2 Analyze(string code)
        {
            Lexer lexer = new Lexer(code);
            SyntaxAnalyzer syntaxAnalyzer = new SyntaxAnalyzer(lexer.Output);
            SemanticAnalyzer1 semanticAnalyzer1 = new SemanticAnalyzer1(syntaxAnalyzer.OutputNode);
            syntaxAnalyzer.OutputNode.Accept(semanticAnalyzer1);
            SemanticAnalyzer2 semanticAnalyzer2 = new SemanticAnalyzer2(semanticAnalyzer1.MainNode);
            semanticAnalyzer1.MainNode.Accept(semanticAnalyzer2);

            return semanticAnalyzer2;
        }

        [TestMethod]
        public void test_enums()
        {
            var analyzer = Analyze(@"enum test = 123; var something = test;");

            Assert.IsInstanceOfType(analyzer.MainNode[0][1][0], typeof(Number));
        }

        [TestMethod]
        public void test_static_if()
        {
            var analyzer = Analyze(@"
static if(something) {
    should_not_exist();
}

enum compile = true;

static if(compile) {
    should_exist();
}

static if(something)
    should_not_exist();
else
    but_this_should();
");

            Assert.IsInstanceOfType(analyzer.MainNode[0][0], typeof(NullStatement));
            Assert.IsInstanceOfType(analyzer.MainNode[0][3][0], typeof(StatementExpression));
        }
    }
}
