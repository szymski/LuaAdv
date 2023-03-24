using System;
using LuaAdv.Compiler.Lexer;
using LuaAdv.Compiler.Nodes;
using LuaAdv.Compiler.Nodes.Expressions;
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
        public void test_multi_enum_1()
        {
            var analyzer = Analyze(@"enum test { a, b, c }; var something = test.a; var something = test.c;");

            Assert.IsInstanceOfType(analyzer.MainNode[0][1][0], typeof(Number));
            Assert.AreEqual(0, ((Number)analyzer.MainNode[0][1][0]).value);

            Assert.IsInstanceOfType(analyzer.MainNode[0][2][0], typeof(Number));
            Assert.AreEqual(2, ((Number)analyzer.MainNode[0][2][0]).value);
        }

        [TestMethod]
        public void test_multi_enum_2()
        {
            var analyzer = Analyze(@"enum test { a = ""test"", b, c = 123 }; var something = test.a; var something = test.c;");

            Assert.IsInstanceOfType(analyzer.MainNode[0][1][0], typeof(StringType));
            Assert.AreEqual("test", ((StringType)analyzer.MainNode[0][1][0]).value);

            Assert.IsInstanceOfType(analyzer.MainNode[0][2][0], typeof(Number));
            Assert.AreEqual(123, ((Number)analyzer.MainNode[0][2][0]).value);
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
        
        [TestMethod]
        public void test_static_if_bool()
        {
            var analyzer = Analyze(@"
                enum value = true;

                static if(value) {
                    should_exist();
                }
                ");
            Assert.IsInstanceOfType(analyzer.MainNode[0][0], typeof(NullStatement));
            Assert.IsInstanceOfType(analyzer.MainNode[0][1][0][0], typeof(FunctionCall));
        }
        
        [TestMethod]
        public void test_static_if_negated_bool()
        {
            var analyzer = Analyze(@"
                enum value = true;

                static if(!value) {
                    should_exist();
                }

                enum value2 = false;

                static if(!value2) {
                    should_exist();
                }
                ");
            
            Assert.IsInstanceOfType(analyzer.MainNode[0][0], typeof(NullStatement));
            Assert.IsInstanceOfType(analyzer.MainNode[0][1], typeof(NullStatement));
            
            Assert.IsInstanceOfType(analyzer.MainNode[0][2], typeof(NullStatement));
            Assert.IsInstanceOfType(analyzer.MainNode[0][3], typeof(Sequence));
            Assert.IsInstanceOfType(analyzer.MainNode[0][3][0][0], typeof(FunctionCall));
        }
        
        [TestMethod]
        public void test_static_if_number_equality()
        {
            var analyzer = Analyze(@"
                static if(2 + 3 == 5) {
                    should_exist();
                }

                static if(2 + 3 == 8) {
                    should_not_exist();
                }
                ");
            Assert.IsInstanceOfType(analyzer.MainNode[0][0][0][0], typeof(FunctionCall));
            Assert.IsInstanceOfType(analyzer.MainNode[0][1], typeof(NullStatement));
        }
        
        [TestMethod]
        public void test_static_if_number_inequality()
        {
            var analyzer = Analyze(@"
                static if(2 + 3 != 8) {
                    should_exist();
                }

                static if(2 + 3 != 5) {
                    should_not_exist();
                }
                ");
            Assert.IsInstanceOfType(analyzer.MainNode[0][0][0][0], typeof(FunctionCall));
            Assert.IsInstanceOfType(analyzer.MainNode[0][1], typeof(NullStatement));
        }
        
        [TestMethod]
        public void test_static_if_string_equality()
        {
            var analyzer = Analyze("""
                enum version = "2.0";

                static if(version == "2.0") {
                    should_exist();
                }

                static if("1.0" == version) {
                    should_not_exist();
                }
                """);
            Assert.IsInstanceOfType(analyzer.MainNode[0][0], typeof(NullStatement));
            Assert.IsInstanceOfType(analyzer.MainNode[0][1][0][0], typeof(FunctionCall));
            Assert.IsInstanceOfType(analyzer.MainNode[0][2], typeof(NullStatement));
        }
        
        [TestMethod]
        public void test_equality_numbers()
        {
            var analyzer = Analyze("""
                var equal = (0.0001 == 0.0001);
                var notEqual = 2 == 3;
                var equal2 = (2 + 3) == 5;
                """);
            Assert.IsInstanceOfType(analyzer.MainNode[0][0][0], typeof(Bool));
            Assert.AreEqual(true, ((Bool)analyzer.MainNode[0][0][0]).value);
            Assert.IsInstanceOfType(analyzer.MainNode[0][1][0], typeof(Bool));
            Assert.AreEqual(false, ((Bool)analyzer.MainNode[0][1][0]).value);
            Assert.IsInstanceOfType(analyzer.MainNode[0][2][0], typeof(Bool));
            Assert.AreEqual(true, ((Bool)analyzer.MainNode[0][2][0]).value);
        }
        
        [TestMethod]
        public void test_non_equality_numbers()
        {
            var analyzer = Analyze("""
                var notEqual = 2 != 3;
                var equal = 5 != 5;
                var equal2 = !((2 + 2) != 4);
                """);
            Assert.IsInstanceOfType(analyzer.MainNode[0][0][0], typeof(Bool));
            Assert.AreEqual(true, ((Bool)analyzer.MainNode[0][0][0]).value);
            Assert.IsInstanceOfType(analyzer.MainNode[0][1][0], typeof(Bool));
            Assert.AreEqual(false, ((Bool)analyzer.MainNode[0][1][0]).value);
            Assert.IsInstanceOfType(analyzer.MainNode[0][2][0], typeof(Bool));
            Assert.AreEqual(true, ((Bool)analyzer.MainNode[0][2][0]).value);
        }
        
        [TestMethod]
        public void test_equality_strings()
        {
            var analyzer = Analyze("""
                var equal = "str" == "str";
                var notEqual = "left" == "right";
                """);
            Assert.IsInstanceOfType(analyzer.MainNode[0][0][0], typeof(Bool));
            Assert.AreEqual(true, ((Bool)analyzer.MainNode[0][0][0]).value);
            Assert.IsInstanceOfType(analyzer.MainNode[0][1][0], typeof(Bool));
            Assert.AreEqual(false, ((Bool)analyzer.MainNode[0][1][0]).value);
        }

        [TestMethod]
        public void test_non_equality_strings()
        {
            var analyzer = Analyze("""
                var notEqual = "left" != "right";
                var equal = "str" != "str";
                """);
            Assert.IsInstanceOfType(analyzer.MainNode[0][0][0], typeof(Bool));
            Assert.AreEqual(true, ((Bool)analyzer.MainNode[0][0][0]).value);
            Assert.IsInstanceOfType(analyzer.MainNode[0][1][0], typeof(Bool));
            Assert.AreEqual(false, ((Bool)analyzer.MainNode[0][1][0]).value);
        }

        [TestMethod]
        public void test_equality_bools()
        {
            var analyzer = Analyze("""
                var equal = true == true;
                var notEqual = false == true;
                """);
            Assert.IsInstanceOfType(analyzer.MainNode[0][0][0], typeof(Bool));
            Assert.AreEqual(true, ((Bool)analyzer.MainNode[0][0][0]).value);
            Assert.IsInstanceOfType(analyzer.MainNode[0][1][0], typeof(Bool));
            Assert.AreEqual(false, ((Bool)analyzer.MainNode[0][1][0]).value);
        }
        
        [TestMethod]
        public void test_non_equality_bools()
        {
            var analyzer = Analyze("""
                var notEqual = false != true;
                var equal = true != true;
                """);
            Assert.IsInstanceOfType(analyzer.MainNode[0][0][0], typeof(Bool));
            Assert.AreEqual(true, ((Bool)analyzer.MainNode[0][0][0]).value);
            Assert.IsInstanceOfType(analyzer.MainNode[0][1][0], typeof(Bool));
            Assert.AreEqual(false, ((Bool)analyzer.MainNode[0][1][0]).value);
        }
        
        [TestMethod]
        public void test_equality_nulls()
        {
            var analyzer = Analyze("""
                var equal = null == null;
                var notEqual = null == true;
                """);
            Assert.IsInstanceOfType(analyzer.MainNode[0][0][0], typeof(Bool));
            Assert.AreEqual(true, ((Bool)analyzer.MainNode[0][0][0]).value);
            Assert.IsInstanceOfType(analyzer.MainNode[0][1][0], typeof(Bool));
            Assert.AreEqual(false, ((Bool)analyzer.MainNode[0][1][0]).value);
        }

        [TestMethod]
        public void test_not()
        {
            var analyzer = Analyze("""
                var bool = !false;
                var str = !"str";
                var num = !0;
                var nil = !null;
                """);
            Assert.IsInstanceOfType(analyzer.MainNode[0][0][0], typeof(Bool));
            Assert.AreEqual(true, ((Bool)analyzer.MainNode[0][0][0]).value);
            Assert.IsInstanceOfType(analyzer.MainNode[0][1][0], typeof(Bool));
            Assert.AreEqual(false, ((Bool)analyzer.MainNode[0][1][0]).value);
            Assert.IsInstanceOfType(analyzer.MainNode[0][2][0], typeof(Bool));
            Assert.AreEqual(false, ((Bool)analyzer.MainNode[0][2][0]).value);
            Assert.IsInstanceOfType(analyzer.MainNode[0][3][0], typeof(Bool));
            Assert.AreEqual(true, ((Bool)analyzer.MainNode[0][3][0]).value);
        }
    }
}
