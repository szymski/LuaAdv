using System;
using LuaAdv.Compiler;
using LuaAdv.Compiler.Lexer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuaAdvTests
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void test_identifiers_and_keywords()
        {
            Lexer lexer = new Lexer(@"local function rzopa derp");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id++], typeof(TokenKeyword));
            Assert.IsInstanceOfType(lexer.Output[id++], typeof(TokenKeyword));
            Assert.IsInstanceOfType(lexer.Output[id++], typeof(TokenIdentifier));
            Assert.IsInstanceOfType(lexer.Output[id++], typeof(TokenIdentifier));

            Assert.AreEqual(0, lexer.Output[0].Character);
        }

        [TestMethod]
        public void test_numbers()
        {
            Lexer lexer = new Lexer(@"1234 0.0512 0xFF");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            Assert.AreEqual(1234, ((TokenNumber)lexer.Output[id]).Number);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            Assert.AreEqual(0.0512D, ((TokenNumber)lexer.Output[id]).Number);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            Assert.AreEqual(0xFF, ((TokenNumber)lexer.Output[id]).Number);
            id++;
        }
        
        [TestMethod]
        public void test_numbers_with_separators()
        {
            Lexer lexer = new Lexer(@"1_234 1_000_000 0.051_232 0xFF");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            Assert.AreEqual(1234, ((TokenNumber)lexer.Output[id]).Number);
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            Assert.AreEqual(1000000, ((TokenNumber)lexer.Output[id]).Number);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            Assert.AreEqual(0.051232D, ((TokenNumber)lexer.Output[id]).Number);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            Assert.AreEqual(0xFF, ((TokenNumber)lexer.Output[id]).Number);
            id++;
        }

        [TestMethod]
        public void test_strings()
        {
            Lexer lexer = new Lexer(@"""I am a string"" ""I am also a string""");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenString));
            Assert.AreEqual("I am a string", lexer.Output[id].Value);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenString));
            Assert.AreEqual("I am also a string", lexer.Output[id].Value);
            id++;
        }

        [TestMethod]
        public void test_strings_special_characters()
        {
            Lexer lexer = new Lexer(@" ""\t \\ \"""" ");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenString));
            Assert.AreEqual("\\t \\\\ \\\"", lexer.Output[id].Value);
            id++;
        }

        [TestMethod]
        public void test_strings_multiline()
        {
            Lexer lexer = new Lexer(" @\"no multi-line\" @\"multi\n-line\" @\"super\nm\nu\nlti\n-line\" ");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenString));
            Assert.AreEqual("no multi-line", lexer.Output[id].Value);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenString));
            Assert.AreEqual("multi\\n-line", lexer.Output[id].Value);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenString));
            Assert.AreEqual("super\\nm\\nu\\nlti\\n-line", lexer.Output[id].Value);
            id++;
        }

        [TestMethod]
        public void test_interpolated_strings_no_interpolation()
        {
            Lexer lexer = new Lexer(@"`No interpolation test` `123`");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringStart));
            Assert.AreEqual("", lexer.Output[id].Value);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringText));
            Assert.AreEqual("No interpolation test", lexer.Output[id].Value);
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringEnd));
            Assert.AreEqual("", lexer.Output[id].Value);
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringStart));
            Assert.AreEqual("", lexer.Output[id].Value);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringText));
            Assert.AreEqual("123", lexer.Output[id].Value);
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringEnd));
            Assert.AreEqual("", lexer.Output[id].Value);
            id++;
        }

        [TestMethod]
        public void test_interpolated_strings_empty()
        {
            Lexer lexer = new Lexer(@"``");

            var id = 0;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringStart));
            Assert.AreEqual("", lexer.Output[id].Value);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringEnd));
            Assert.AreEqual("", lexer.Output[id].Value);
            id++;
            
        }

        [TestMethod]
        public void test_interpolated_strings_with_single_interpolation()
        {
            Lexer lexer = new Lexer(@"`2 + 2 = ${2 + 2}`");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringStart));
            Assert.AreEqual("", lexer.Output[id].Value);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringText));
            Assert.AreEqual("2 + 2 = ", lexer.Output[id].Value);
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenSymbol));
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringEnd));
            Assert.AreEqual("", lexer.Output[id].Value);
            id++;
        }
        
        [TestMethod]
        public void test_interpolated_strings_with_single_interpolation_text_after()
        {
            Lexer lexer = new Lexer(@"`2 + 2 = ${2 + 2} innit?`");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringStart));
            Assert.AreEqual("", lexer.Output[id].Value);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringText));
            Assert.AreEqual("2 + 2 = ", lexer.Output[id].Value);
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenSymbol));
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringText));
            Assert.AreEqual(" innit?", lexer.Output[id].Value);
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringEnd));
            Assert.AreEqual("", lexer.Output[id].Value);
            id++;
        }
        
        [TestMethod]
        public void test_interpolated_strings_with_multiple_interpolations()
        {
            Lexer lexer = new Lexer(@"`${2} + ${3} = ${2 + 3}`");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringStart));
            Assert.AreEqual("", lexer.Output[id].Value);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringText));
            Assert.AreEqual(" + ", lexer.Output[id].Value);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringText));
            Assert.AreEqual(" = ", lexer.Output[id].Value);
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenSymbol));
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringEnd));
            Assert.AreEqual("", lexer.Output[id].Value);
            id++;
        }
        
        [TestMethod]
        public void test_interpolated_strings_nested()
        {
            Lexer lexer = new Lexer(@"`${`xd${123}`}`");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringStart));
            Assert.AreEqual("", lexer.Output[id].Value);
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringStart));
            Assert.AreEqual("", lexer.Output[id].Value);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringText));
            Assert.AreEqual("xd", lexer.Output[id].Value);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            id++;
            
            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringEnd));
            Assert.AreEqual("", lexer.Output[id].Value);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenInterpolatedStringEnd));
            Assert.AreEqual("", lexer.Output[id].Value);
            id++;
        }
        
        [TestMethod]
        public void test_symbols()
        {
            Lexer lexer = new Lexer("!!>=<<(){=>");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenSymbol));
            Assert.AreEqual(lexer.Output[id].Value, "!");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenSymbol));
            Assert.AreEqual(lexer.Output[id].Value, "!");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenSymbol));
            Assert.AreEqual(lexer.Output[id].Value, ">=");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenSymbol));
            Assert.AreEqual(lexer.Output[id].Value, "<<");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenSymbol));
            Assert.AreEqual(lexer.Output[id].Value, "(");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenSymbol));
            Assert.AreEqual(lexer.Output[id].Value, ")");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenSymbol));
            Assert.AreEqual(lexer.Output[id].Value, "{");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenSymbol));
            Assert.AreEqual(lexer.Output[id].Value, "=>");
            id++;
        }

        [TestMethod]
        public void test_comments()
        {
            Lexer lexer = new Lexer(@"// Comment // Derp
identifier// Next

/* Multiline in single line */ /* Real
multiline */

/* Another
multi
 line
comment */

/*
multi
line
*/ ident /*
*/

        RACING2.Hook.List = { }; // Value: { EventName, Identifier }

    TrackSettings = 2, // Also settings NPC position
");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenComment));
            Assert.AreEqual(lexer.Output[id].Value, " Comment // Derp");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenIdentifier));
            Assert.AreEqual(lexer.Output[id].Value, "identifier");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenComment));
            Assert.AreEqual(lexer.Output[id].Value, " Next");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenComment));
            Assert.AreEqual(lexer.Output[id].Value, " Multiline in single line ");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenComment));
            Assert.AreEqual(lexer.Output[id].Value, " Real\nmultiline ");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenComment));
            Assert.AreEqual(lexer.Output[id].Value, " Another\nmulti\n line\ncomment ");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenComment));
            Assert.AreEqual(lexer.Output[id].Value, "\nmulti\nline\n");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenIdentifier));
            Assert.AreEqual(lexer.Output[id].Value, "ident");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenComment));
            Assert.AreEqual(lexer.Output[id].Value, "\n");
            id++;
        }

        [TestMethod]
        public void test_comments_two_oneline()
        {
            Lexer lexer = new Lexer("/* asd */ /* asdf */\n"
                                    + "/** doc1 */ /** doc2 */");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenComment));
            Assert.AreEqual(lexer.Output[id].Value, " asd ");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenComment));
            Assert.AreEqual(lexer.Output[id].Value, " asdf ");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenDocumentationComment));
            Assert.AreEqual(lexer.Output[id].Value, " doc1 ");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenDocumentationComment));
            Assert.AreEqual(lexer.Output[id].Value, " doc2 ");
            id++;
        }

        [TestMethod]
        public void test_documentation_comments()
        {
            Lexer lexer = new Lexer(@"/// Comment /// Derp
identifier/// Next

/** Multiline in single line */ /** Real
multiline */

/** Another
multi
line
comment */

/**
multi
line
*/ ident /**
*/");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenDocumentationComment));
            Assert.AreEqual(lexer.Output[id].Value, " Comment /// Derp");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenIdentifier));
            Assert.AreEqual(lexer.Output[id].Value, "identifier");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenDocumentationComment));
            Assert.AreEqual(lexer.Output[id].Value, " Next");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenDocumentationComment));
            Assert.AreEqual(lexer.Output[id].Value, " Multiline in single line ");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenDocumentationComment));
            Assert.AreEqual(lexer.Output[id].Value, " Real\nmultiline ");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenDocumentationComment));
            Assert.AreEqual(lexer.Output[id].Value, " Another\nmulti\nline\ncomment ");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenDocumentationComment));
            Assert.AreEqual(lexer.Output[id].Value, "\nmulti\nline\n");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenIdentifier));
            Assert.AreEqual(lexer.Output[id].Value, "ident");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenDocumentationComment));
            Assert.AreEqual(lexer.Output[id].Value, "\n");
            id++;
        }

        [TestMethod]
        public void test_special_tokens()
        {
            Lexer lexer = new Lexer("__LINE__ __FILE__ __DATE__ __FUNCTION__");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenSpecial));
            Assert.AreEqual(lexer.Output[id].Value, "__LINE__");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenSpecial));
            Assert.AreEqual(lexer.Output[id].Value, "__FILE__");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenSpecial));
            Assert.AreEqual(lexer.Output[id].Value, "__DATE__");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenSpecial));
            Assert.AreEqual(lexer.Output[id].Value, "__FUNCTION__");
            id++;

        }
    }
}
