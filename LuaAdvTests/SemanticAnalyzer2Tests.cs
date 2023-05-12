using System;
using LuaAdv.Compiler.Lexer;
using LuaAdv.Compiler.Nodes;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Expressions.Assignment;
using LuaAdv.Compiler.Nodes.Expressions.BasicTypes;
using LuaAdv.Compiler.Nodes.Expressions.Comparison;
using LuaAdv.Compiler.Nodes.Math;
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
        public void test_equality_booleans()
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
        public void test_non_equality_booleans()
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
        public void test_unary_not()
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
        
        [TestMethod]
        public void test_interpolated_string_lowering()
        {
            /*
                GroupedEquation(
                    [0]Concat(
                        [0]StringType("2 + 3 = "),
                        [1]Concat(
                            [0]Number(5),
                            [1]StringType(", right?")
                        )
                    )
                )
            */
            var analyzer = Analyze("var str = `2 + 3 = ${2 + 3}, right?`;");
            var group = analyzer.MainNode[0][0][0] as GroupedEquation;
            Assert.IsInstanceOfType(group, typeof(GroupedEquation));
            
            Assert.IsInstanceOfType(group[0], typeof(Concat));
            
            Assert.IsInstanceOfType(group[0][0], typeof(StringType));
            Assert.AreEqual("2 + 3 = ", ((StringType)group[0][0]).value);
            
            Assert.IsInstanceOfType(group[0][1], typeof(Concat));
            Assert.IsInstanceOfType(group[0][1][0], typeof(Number));
            Assert.AreEqual(5, ((Number)group[0][1][0]).value);
            Assert.IsInstanceOfType(group[0][1][1], typeof(StringType));
            Assert.AreEqual(", right?", ((StringType)group[0][1][1]).value);
        }
        
        [TestMethod]
        public void test_table_item_traversal()
        {
            var analyzer = Analyze("""
                var tbl = {
                    1,
                    1 + 1,
                    three = () => 3,
                };
            """);
            var table = analyzer.MainNode[0][0][0] as Table;
            Assert.IsInstanceOfType<Table>(table);
            
            Assert.IsNull(table[0]);
            Assert.IsInstanceOfType<Number>(table[1]);
            Assert.AreEqual(1, ((Number)table[1]).value);
            
            Assert.IsNull(table[2]);
            Assert.IsInstanceOfType<Number>(table[3]);
            Assert.AreEqual(2, ((Number)table[3]).value);
            
            Assert.IsInstanceOfType<StringType>(table[4]);
            Assert.AreEqual("three", ((StringType)table[4]).value);
            Assert.IsInstanceOfType<AnonymousFunction>(table[5][0]);
            Assert.AreEqual(0, ((AnonymousFunction)table[5][0]).parameterList.Count);

            var funcExpr = table[5][0][0][0][0];
            Assert.IsInstanceOfType<Number>(funcExpr);
            Assert.AreEqual(3, ((Number)funcExpr).value);
        }

        [TestMethod]
        public void test_null_coalescing_lowering()
        {
            var analyzer = Analyze(
                """
                print(test ?? {});
            """
            );

            var func = analyzer.MainNode[0][0][0];
            Assert.IsInstanceOfType<FunctionCall>(func);
            
            Assert.IsInstanceOfType<Ternary>(func[1]);
            
            Assert.IsInstanceOfType<NotEquals>(func[1][0]);
            Assert.IsInstanceOfType<Variable>(func[1][0][0]);
            Assert.AreEqual("test", ((Variable)func[1][0][0]).name);
            Assert.IsInstanceOfType<Null>(func[1][0][1]);
            
            Assert.IsInstanceOfType<Variable>(func[1][1]);
            Assert.AreEqual("test", ((Variable)func[1][1]).name);
            
            Assert.IsInstanceOfType<Table>(func[1][2]);
        }

        [TestMethod]
        public void test_null_coalescing_assignment_lowering()
        {
            var analyzer = Analyze(
                """
                test ??= 1;
            """
            );
            
            Assert.IsInstanceOfType<ValueAssignmentOperator>(analyzer.MainNode[0][0][0]);
            
            var table = analyzer.MainNode[0][0][0][0] as Variable;
            Assert.IsInstanceOfType<Variable>(table);
            
            var ternary = analyzer.MainNode[0][0][0][1] as Ternary;
            Assert.IsInstanceOfType<Ternary>(ternary);
            
            Assert.IsInstanceOfType<NotEquals>(ternary[0]);
            Assert.IsInstanceOfType<Variable>(ternary[0][0]);
            Assert.AreEqual("test", ((Variable)ternary[0][0]).name);
            Assert.IsInstanceOfType<Null>(ternary[0][1]);

            Assert.IsInstanceOfType<Variable>(ternary[1]);
            Assert.AreEqual("test", ((Variable)ternary[1]).name);
            
            Assert.IsInstanceOfType<Number>(ternary[2]);
            Assert.AreEqual(1, ((Number)ternary[2]).value);
        }
    }
}
