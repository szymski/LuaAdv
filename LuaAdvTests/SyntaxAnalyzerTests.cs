using System;
using System.Text;
using System.Collections.Generic;
using LuaAdv.Compiler;
using LuaAdv.Compiler.Lexer;
using LuaAdv.Compiler.Nodes;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Expressions.Assignment;
using LuaAdv.Compiler.Nodes.Expressions.BasicTypes;
using LuaAdv.Compiler.Nodes.Expressions.Comparison;
using LuaAdv.Compiler.Nodes.Expressions.Conditional;
using LuaAdv.Compiler.Nodes.Expressions.Logical;
using LuaAdv.Compiler.Nodes.Expressions.Unary;
using LuaAdv.Compiler.Nodes.Expressions.Unary.Post;
using LuaAdv.Compiler.Nodes.Expressions.Unary.Pre;
using LuaAdv.Compiler.Nodes.Math;
using LuaAdv.Compiler.Nodes.Statements;
using LuaAdv.Compiler.SyntaxAnalyzer;
using NUnit.Framework;
using Is = LuaAdv.Compiler.Nodes.Expressions.Comparison.Is;

namespace LuaAdvTests
{
    [TestFixture]
    public class SyntaxAnalyzerTests_Statements
    {
        public Node Compile(string code)
        {
            Lexer lexer = new Lexer(code);
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output);

            return analyzer.OutputNode;
        }

        [Test]
        public void test_analyze_nothing()
        {
            Lexer lexer = new Lexer("");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output);

            var output = analyzer.OutputNode;

            Assert.IsInstanceOf<Sequence>(output);
        }

        [Test]
        public void test_analyze_ifs()
        {
            Lexer lexer = new Lexer("if(true) { } else if(false) { } else { }");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output);

            var output = analyzer.OutputNode;

            Assert.IsInstanceOf<Sequence>(output);
            Assert.IsInstanceOf<If>(output.Children[0]);
            Assert.IsInstanceOf<Bool>(output.Children[0][0]);
            Assert.IsInstanceOf<Sequence>(output.Children[0][1]);
            Assert.IsInstanceOf<Bool>(output.Children[0][2]);
            Assert.IsInstanceOf<Sequence>(output.Children[0][3]);
            Assert.IsInstanceOf<Sequence>(output.Children[0][5]);
        }

        [Test]
        public void test_analyze_for_loop()
        {
            Lexer lexer = new Lexer("for(;false;) { } " +
                                    "for(var val = 0; val < 10; val++) { } ");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Node node;

            node = analyzer.Statement();
            Assert.IsInstanceOf<For>(node);
            Assert.IsInstanceOf<Bool>(node[1]);

            node = analyzer.Statement();
            Assert.IsInstanceOf<For>(node);
            Assert.IsInstanceOf<LocalVariablesDeclaration>(node[0]);
            Assert.IsInstanceOf<Less>(node[1]);
            Assert.IsInstanceOf<PostIncrement>(node[2][0]);
        }

        [Test]
        public void test_expression_list()
        {
            Lexer lexer = new Lexer("return true, false, true");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            analyzer.AcceptKeyword("return");

            var expList = analyzer.ExpressionList();
            Assert.AreEqual(3, expList.Count);
        }

        [Test]
        public void test_expression_list_error()
        {
            Assert.Throws<SyntaxAnalyzerException>(() =>
            {
                Lexer lexer = new Lexer("return true, false,");
                SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

                analyzer.AcceptKeyword("return");

                analyzer.ExpressionList();
            });
        }

        [Test]
        public void test_expression_list_empty()
        {
            Lexer lexer = new Lexer("return ");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            analyzer.AcceptKeyword("return");

            var expList = analyzer.ExpressionList();
            Assert.AreEqual(0, expList.Count);
        }

        [Test]
        public void test_function_param_list()
        {
            Lexer lexer = new Lexer("param1, param2 = true, param3");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            var paramList = analyzer.FunctionParameterList();
            Assert.AreEqual(3, paramList.Count);

            Assert.AreEqual("param1", paramList[0].Item2);
            Assert.AreEqual("param2", paramList[1].Item2);
            Assert.IsInstanceOf<Bool>(paramList[1].Item3);
            Assert.AreEqual("param3", paramList[2].Item2);
        }

        [Test]
        public void test_function_param_error()
        {
            Assert.Throws<SyntaxAnalyzerException>(() =>
            {
                Lexer lexer = new Lexer("param1, param2 = , param3");
                SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

                analyzer.FunctionParameterList();
            });
        }

        [Test]
        public void test_function_param_error_empty()
        {
            Lexer lexer = new Lexer("");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            var paramList = analyzer.FunctionParameterList();
            Assert.AreEqual(0, paramList.Count);
        }

        [Test]
        public void test_functions()
        {
            Lexer lexer = new Lexer("function Derp.Asd(param1 = true, param2) { }\nlocal function Func() { }");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output);

            var output = analyzer.OutputNode;

            Assert.IsInstanceOf<StatementFunctionDeclaration>(output[0]);
            var func1 = (StatementFunctionDeclaration)output[0];
            Assert.AreEqual(2, func1.parameterList.Count);

            Assert.IsInstanceOf<StatementFunctionDeclaration>(output[1]);
            var func2 = (StatementFunctionDeclaration)output[1];
            Assert.AreEqual(0, func2.parameterList.Count);
            Assert.IsTrue(func2.local);
        }

        [Test]
        public void test_function_errors()
        {
            Assert.Throws<SyntaxAnalyzerException>(() =>
            {
                Compile("function Test( { }");
            });

            Assert.Throws<SyntaxAnalyzerException>(() =>
            {
                Compile("function () {}");
            });

            Assert.Throws<SyntaxAnalyzerException>(() =>
            {
                Compile("function (return) {}");
            });

            Assert.Throws<SyntaxAnalyzerException>(() =>
            {
                Compile("function Test()");
            });
        }

        [Test]
        public void test_variable_name_list()
        {
            Lexer lexer = new Lexer("rzopa, (rzopa).nierzopa, func().func2().a, rzopa2[true][false], rzopa3.y.z");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            var varNameList = analyzer.NamedVariableList();
            Assert.AreEqual(5, varNameList.Count);
        }

        [Test]
        public void test_identifier_list()
        {
            Lexer lexer = new Lexer("a, b, abc");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            var identList = analyzer.IdentifierList();
            Assert.AreEqual(3, identList.Count);

            Assert.AreEqual("a", identList[0].Item2);
            Assert.AreEqual("b", identList[1].Item2);
            Assert.AreEqual("abc", identList[2].Item2);
        }

        [Test]
        public void test_identifier_error()
        {
            Assert.Throws<SyntaxAnalyzerException>(() =>
            {
                Lexer lexer = new Lexer("a, b,");
                SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

                analyzer.IdentifierList();
            });
        }

        [Test]
        public void test_local_variable_declaration()
        {
            var output = Compile("var oneVar;");

            Assert.IsInstanceOf<LocalVariablesDeclaration>(output[0]);
            Assert.AreEqual("oneVar", (output[0] as LocalVariablesDeclaration).variables[0].Item2);

            output = Compile("var var1, secVar, thirdVar = true, false, true;");

            Assert.IsInstanceOf<LocalVariablesDeclaration>(output[0]);
            Assert.AreEqual("var1", (output[0] as LocalVariablesDeclaration).variables[0].Item2);
            Assert.AreEqual("secVar", (output[0] as LocalVariablesDeclaration).variables[1].Item2);
            Assert.AreEqual("thirdVar", (output[0] as LocalVariablesDeclaration).variables[2].Item2);
            Assert.IsInstanceOf<Bool>((output[0] as LocalVariablesDeclaration).values[0]);
            Assert.IsInstanceOf<Bool>((output[0] as LocalVariablesDeclaration).values[1]);
            Assert.IsInstanceOf<Bool>((output[0] as LocalVariablesDeclaration).values[2]);

            output = Compile("var var1, secVar, thirdVar = true;");

            Assert.IsInstanceOf<LocalVariablesDeclaration>(output[0]);
            Assert.AreEqual("var1", (output[0] as LocalVariablesDeclaration).variables[0].Item2);
            Assert.AreEqual("secVar", (output[0] as LocalVariablesDeclaration).variables[1].Item2);
            Assert.AreEqual("thirdVar", (output[0] as LocalVariablesDeclaration).variables[2].Item2);
            Assert.IsInstanceOf<Bool>((output[0] as LocalVariablesDeclaration).values[0]);
        }

        [Test]
        public void test_local_variable_declaration_errors()
        {
            Assert.Throws<SyntaxAnalyzerException>(() =>
            {
                Compile("var");
            });

            Assert.Throws<SyntaxAnalyzerException>(() =>
            {
                Compile("var dupa.rzopa");
            });

            Assert.Throws<SyntaxAnalyzerException>(() =>
            {
                Compile("var asd, asd,");
            });

            Assert.Throws<SyntaxAnalyzerException>(() =>
            {
                Compile("var asd1, asd2 = true, true, false, false");
            });

            Assert.Throws<SyntaxAnalyzerException>(() =>
            {
                Compile("var asd1, asd2 = ");
            });
        }

        [Test]
        public void test_lambda_functions()
        {
            Lexer lexer = new Lexer("function Derp(asd) => 123");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Statement node;

            node = analyzer.Statement();
            Assert.IsInstanceOf<StatementLambdaFunctionDeclaration>(node);
        }

        [Test]
        public void test_methods()
        {
            Lexer lexer = new Lexer("function lol:derp() { } " +
                                    "function asdf.asd:lol() { } " +
                                    "function this[asd].isa:derp():AFunc() { } " +
                                    "function lol:derp() => 5" +
                                    "local function derp:asd() { }");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Statement node;

            node = analyzer.Statement();
            Assert.IsInstanceOf<StatementMethodDeclaration>(node);

            node = analyzer.Statement();
            Assert.IsInstanceOf<StatementMethodDeclaration>(node);

            node = analyzer.Statement();
            Assert.IsInstanceOf<StatementMethodDeclaration>(node);

            node = analyzer.Statement();
            Assert.IsInstanceOf<StatementLambdaMethodDeclaration>(node);

            Assert.Throws<SyntaxAnalyzerException>(() =>
            {
                analyzer.Statement();
            });
        }

        [Test]
        public void test_global_variables_declaration()
        {
            Lexer lexer = new Lexer("a = 5; " +
                                    "a, b, c = 1, 2, 3; " +
                                    "table.a[123], derp = 1, 2; " +
                                    "a = 1, 2; " +
                                    "func() = 123;");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Statement node;

            node = analyzer.Statement();
            Assert.IsInstanceOf<GlobalVariablesDeclaration>(node);

            node = analyzer.Statement();
            Assert.IsInstanceOf<GlobalVariablesDeclaration>(node);

            node = analyzer.Statement();
            Assert.IsInstanceOf<GlobalVariablesDeclaration>(node);

            Assert.Throws<SyntaxAnalyzerException>(() =>
            {
                analyzer.Statement();
            });
        }

        [Test]
        public void test_statement_expressions()
        {
            Lexer lexer = new Lexer("func(); " +
                                    "lol[123].asd(); " +
                                    "method:call(); " +
                                    "a++; " +
                                    "--b; " +
                                    "a += 10;");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Statement node;

            node = analyzer.Statement();
            Assert.IsInstanceOf<StatementExpression>(node);

            node = analyzer.Statement();
            Assert.IsInstanceOf<StatementExpression>(node);

            node = analyzer.Statement();
            Assert.IsInstanceOf<StatementExpression>(node);

            node = analyzer.Statement();
            Assert.IsInstanceOf<StatementExpression>(node);

            node = analyzer.Statement();
            Assert.IsInstanceOf<StatementExpression>(node);

            node = analyzer.Statement();
            Assert.IsInstanceOf<StatementExpression>(node);

            Assert.Throws<SyntaxAnalyzerException>(() =>
            {
                Compile("should.cause.an.error;");
            });

            Assert.Throws<SyntaxAnalyzerException>(() =>
            {
                Compile("andthis.one().too;");
            });
        }
    }

    [TestFixture]
    public class SyntaxAnalyzerTests_Expressions
    {
        public Node Compile(string code)
        {
            Lexer lexer = new Lexer(code);
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output);

            return analyzer.OutputNode;
        }

        [Test]
        public void test_grouped_equation()
        {
            Lexer lexer = new Lexer("((true))");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            var output = analyzer.Expression();

            Assert.IsInstanceOf<GroupedEquation>(output);
            Assert.IsInstanceOf<GroupedEquation>(output[0]);
            Assert.IsInstanceOf<Bool>(output[0][0]);
        }

        [Test]
        public void test_basic_types()
        {
            Lexer lexer = new Lexer("true false 123 12.50 \"DERP\" this null");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Assert.IsInstanceOf<Bool>(analyzer.Expression());
            Assert.IsInstanceOf<Bool>(analyzer.Expression());
            Assert.IsInstanceOf<Number>(analyzer.Expression());
            Assert.IsInstanceOf<Number>(analyzer.Expression());
            Assert.IsInstanceOf<StringType>(analyzer.Expression());
            Assert.IsInstanceOf<This>(analyzer.Expression());
            Assert.IsInstanceOf<Null>(analyzer.Expression());
        }

        [Test]
        public void test_variables()
        {
            Lexer lexer = new Lexer("var1 var2");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Assert.IsInstanceOf<Variable>(analyzer.Expression());
            Assert.IsInstanceOf<Variable>(analyzer.Expression());
        }

        [Test]
        public void test_table_index()
        {
            Lexer lexer = new Lexer("var1.somevar var2.somevar.another");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Assert.IsInstanceOf<TableDotIndex>(analyzer.Expression());
            Assert.IsInstanceOf<TableDotIndex>(analyzer.Expression()[0]);

            lexer = new Lexer("var1[1] var2[\"derp\"][lol] var3.lol[5]");
            analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Assert.IsInstanceOf<TableIndex>(analyzer.Expression());
            Assert.IsInstanceOf<TableIndex>(analyzer.Expression()[0]);
            Assert.IsInstanceOf<TableDotIndex>(analyzer.Expression()[0]);
        }

        [Test]
        public void test_function_call()
        {
            Lexer lexer = new Lexer("func(123, 123) derp.func() derp()(5)(123)");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Assert.IsInstanceOf<FunctionCall>(analyzer.Expression());
            Assert.IsInstanceOf<FunctionCall>(analyzer.Expression());
            Assert.IsInstanceOf<FunctionCall>(analyzer.Expression()[0][0]);
        }

        [Test]
        public void test_method_call()
        {
            Lexer lexer = new Lexer("dupa:Func(123, 123) a:a(5):a(5)");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Assert.IsInstanceOf<MethodCall>(analyzer.Expression());
            Assert.IsInstanceOf<MethodCall>(analyzer.Expression()[0]);
        }

        [Test]
        public void test_conditional()
        {
            Lexer lexer = new Lexer("true || false true && false || true false || false || false");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Expression exp;

            exp = analyzer.Expression();
            Assert.IsInstanceOf<ConditionalOr>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<ConditionalOr>(exp);
            Assert.IsInstanceOf<ConditionalAnd>(exp[0]);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<ConditionalOr>(exp);
            Assert.IsInstanceOf<ConditionalOr>(exp[1]);
        }

        [Test]
        public void test_logical()
        {
            Lexer lexer = new Lexer("5 ^ 7 " +
                                    "2 | 3 " +
                                    "a & b " +
                                    "123 | 123 ^ lol ^ lol");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Expression exp;

            exp = analyzer.Expression();
            Assert.IsInstanceOf<LogicalXor>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<LogicalOr>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<LogicalAnd>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<LogicalOr>(exp);
            Assert.IsInstanceOf<LogicalXor>(exp[1]);
            Assert.IsInstanceOf<LogicalXor>(exp[1][1]);
        }

        [Test]
        public void test_comparison_equals_notequals()
        {
            Lexer lexer = new Lexer("5 == 5 " +
                                    "5 == derp != false " +
                                    "5 != 2 ^ 8");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Expression exp;

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Equals>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Equals>(exp);
            Assert.IsInstanceOf<NotEquals>(exp[1]);
            Assert.IsInstanceOf<Variable>(exp[1][0]);
            Assert.IsInstanceOf<Bool>(exp[1][1]);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<LogicalXor>(exp);
            Assert.IsInstanceOf<NotEquals>(exp[0]);
        }

        [Test]
        public void test_comparison_less_greater_is()
        {
            Lexer lexer = new Lexer("5 < 5 " +
                                    "asd > asdf " +
                                    "lol <= derp " +
                                    "lol >= derp " +
                                    "something is number");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Expression exp;

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Less>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Greater>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<LessOrEqual>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<GreaterOrEqual>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Is>(exp);
        }

        [Test]
        public void test_shift()
        {
            Lexer lexer = new Lexer("10 >> 2 " +
                                    "20 << 3 " +
                                    "5 >> 5 >> 2 " +
                                    "10 << 5 >> 2");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Expression exp;

            exp = analyzer.Expression();
            Assert.IsInstanceOf<ShiftRight>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<ShiftLeft>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<ShiftRight>(exp);
            Assert.IsInstanceOf<ShiftRight>(exp[1]);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<ShiftLeft>(exp);
            Assert.IsInstanceOf<ShiftRight>(exp[1]);
        }

        [Test]
        public void test_math()
        {
            Lexer lexer = new Lexer("10 + 2 " +
                                    "20 - 3 " +
                                    "5 * 5 " +
                                    "10 / 5 " +
                                    "12 % 2 " +
                                    "10 ^^ 2 " +
                                    "2 + 2 * 2 " +
                                    "10 + (2 / 5) - 8 ^^ 2 " +
                                    "\"something\" .. derp .. 5 + 5");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Expression exp;

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Add>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Subtract>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Multiply>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Divide>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Modulo>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Power>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Add>(exp);
            Assert.IsInstanceOf<Multiply>(exp[1]);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Add>(exp);
            Assert.IsInstanceOf<Subtract>(exp[1]);
            Assert.IsInstanceOf<GroupedEquation>(exp[1][0]);
            Assert.IsInstanceOf<Power>(exp[1][1]);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Concat>(exp);
            Assert.IsInstanceOf<Concat>(exp[1]);
            Assert.IsInstanceOf<Variable>(exp[1][0]);
            Assert.IsInstanceOf<Add>(exp[1][1]);
        }

        [Test]
        public void test_unary()
        {
            Lexer lexer = new Lexer("-5 " +
                                    "!false " +
                                    "~10 " +
                                    "2 - -2 " +
                                    "++derp " +
                                    "--derp " +
                                    "derp++ " +
                                    "derp-- " +
                                    "--func().b + (derp).a++");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Expression exp;

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Negative>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Not>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Negation>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Subtract>(exp);
            Assert.IsInstanceOf<Negative>(exp[1]);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<PreIncrement>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<PreDecrement>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<PostIncrement>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<PostDecrement>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Add>(exp);
            Assert.IsInstanceOf<PreDecrement>(exp[0]);
            Assert.IsInstanceOf<PostIncrement>(exp[1]);
        }

        [Test]
        public void test_anonymous_functions()
        {
            Lexer lexer = new Lexer("function() { } " +
                                    "function() => 123 ");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Expression exp;

            exp = analyzer.Expression();
            Assert.IsInstanceOf<AnonymousFunction>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<AnonymousLambdaFunction>(exp);
        }

        [Test]
        public void test_single_named_variable()
        {
            Lexer lexer = new Lexer("derp " +
                                    "derp.derp " +
                                    "derp().derp " +
                                    "derp[123].lol " +
                                    "derp[150].derp () " +
                                    "derp:GetTbl().asd ()");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Expression exp;

            exp = analyzer.NamedVariable();
            Assert.IsInstanceOf<NamedVariable>(exp);

            exp = analyzer.NamedVariable();
            Assert.IsInstanceOf<NamedVariable>(exp);

            exp = analyzer.NamedVariable();
            Assert.IsInstanceOf<NamedVariable>(exp);

            exp = analyzer.NamedVariable();
            Assert.IsInstanceOf<NamedVariable>(exp);

            exp = analyzer.NamedVariable();
            Assert.IsInstanceOf<NamedVariable>(exp);

            analyzer.tokenIndex += 2;

            exp = analyzer.NamedVariable();
            Assert.IsInstanceOf<NamedVariable>(exp);
        }

        [Test]
        public void test_table()
        {
            Lexer lexer = new Lexer("{ " +
                                    "test = 123," +
                                    "[123] = abc," +
                                    "123, " +
                                    "} ");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Expression exp;

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Table>(exp);
        }

        [Test]
        public void test_ternary()
        {
            Lexer lexer = new Lexer("true ? 1 : 0 " +
                                    "2 + 2 * 2 ? abc : asdf " +
                                    "asd ? tbl:method() : asd " +
                                    "asd ? tbl:method():another()[1]:derp() : asd");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Expression exp;

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Ternary>(exp);
            Assert.IsInstanceOf<Bool>(exp[0]);
            Assert.IsInstanceOf<Number>(exp[1]);
            Assert.IsInstanceOf<Number>(exp[2]);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Ternary>(exp);
            Assert.IsInstanceOf<Add>(exp[0]);
            Assert.IsInstanceOf<Variable>(exp[1]);
            Assert.IsInstanceOf<Variable>(exp[2]);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Ternary>(exp);
            Assert.IsInstanceOf<Variable>(exp[0]);
            Assert.IsInstanceOf<MethodCall>(exp[1]);
            Assert.IsInstanceOf<Variable>(exp[2]);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<Ternary>(exp);
            Assert.IsInstanceOf<Variable>(exp[0]);
            Assert.IsInstanceOf<MethodCall>(exp[1]);
            Assert.IsInstanceOf<Variable>(exp[2]);
        }

        [Test]
        public void test_null_propagation()
        {
            Lexer lexer = new Lexer("abc ?? asdf");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Expression exp;

            exp = analyzer.Expression();
            Assert.IsInstanceOf<NullPropagation>(exp);
        }

        [Test]
        public void test_assignment_operators()
        {
            Lexer lexer = new Lexer("a = 5 " +
                                    "a += 10 " +
                                    "a -= 10 " +
                                    "a *= 10 " +
                                    "a /= 10 " +
                                    "a %= 10 " +
                                    "a ..= 10 ");
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output, true);

            Expression exp;

            exp = analyzer.Expression();
            Assert.IsInstanceOf<ValueAssignmentOperator>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<AddAssignmentOperator>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<SubtractAssignmentOperator>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<MultiplyAssignmentOperator>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<DivideAssignmentOperator>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<ModuloAssignmentOperator>(exp);

            exp = analyzer.Expression();
            Assert.IsInstanceOf<ConcatAssignmentOperator>(exp);

            Assert.Throws<SyntaxAnalyzerException>(() => Compile("5 += 10"));
        }
    }

    [TestFixture]
    public class SyntaxAnalyzerTests_RealLifeCodeExamples
    {
        public Node Compile(string code)
        {
            Lexer lexer = new Lexer(code);
            SyntaxAnalyzer analyzer = new SyntaxAnalyzer(lexer.Output);

            return analyzer.OutputNode;
        }

        [Test]
        public void test_code_1()
        {
            Compile(@"var headPos = this.Model.Entity:GetBonePosition(bone);
                this.Model:SetLookAt(headPos + Vector(0, 0, 0));
                this.Model.Entity:SetPos(Vector(0, -1, -1));
                this.Model:SetCamPos(headPos - Vector(-22, 0, -1));
                this.Model.Entity:SetAngles(Angle(0, 20, 0));");
        }

        [Test]
        public void test_code_2()
        {
            Compile(@"CBF1HUD_PlayerInfo.__index = null;
                derma.DefineControl(""BF1HUD_PlayerInfo"", ""BF1HUD_PlayerInfo"", CBF1HUD_PlayerInfo, ""EditablePanel"");

                local function CreatePlayerInfoPanel() {
                                if (IsValid(BF1HUD_PLAYERINFO))
                                    BF1HUD_PLAYERINFO: Remove();

                                BF1HUD_PLAYERINFO = vgui.Create(""BF1HUD_PlayerInfo"");
                                BF1HUD_PLAYERINFO: Setup();
                            }
            ");
        }

        [Test]
        public void test_code_3()
        {
            Compile(@"if(this.Ammo == -1)
                if(this.MaxAmmo[weapon][2] > 0)
                    this.AmmoDisplayType = 2;
                else if(this.MaxAmmo[weapon][3] > 0)
                    this.AmmoDisplayType = 4;
                else
                    this.AmmoDisplayType = 3;

            if(this.MaxAmmo[weapon][1] == 0)
                this.AmmoDisplayType = 3;
        
            this.HasSecondaryAmmo = weapon:GetSecondaryAmmoType() != -1;");
        }

        [Test]
        public void test_code_4()
        {
            Compile(@"var config = BF1HUDConfig;

                BF1HUD.LanguageTables = {
                    [""en""] = {

                    },

                    [""pl""] = {
                        [""Gun license""] = ""Licencja na broń"",
                        [""Wanted""] = ""Poszukiwany"", 
                        [""Reload""] = ""Przeladuj"",
                        [""Arrest""] = ""Areszt"",
                    },
                };

                var currentLanguageTable = BF1HUD.LanguageTables[config.Language];
                if(!currentLanguageTable) {
                    print(""BF1HUD: No such language "" .. config.Language);
                    currentLanguageTable = BF1HUD.LanguageTables[""en""];
                }

                BF1HUD.Language = { };
                BF1HUD.Language.__index = function(self, value) {
                    return currentLanguageTable[value:lower()] ?? (currentLanguageTable[value] ?? value);
                };
                setmetatable(BF1HUD.Language, BF1HUD.Language);");
        }
    }
}
