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
            Lexer lexer = new Lexer(@"123 0.0512 0xFF");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            Assert.AreEqual(((TokenNumber)lexer.Output[id]).Number, 123);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            Assert.AreEqual(((TokenNumber)lexer.Output[id]).Number, 0.0512D);
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenNumber));
            Assert.AreEqual(((TokenNumber)lexer.Output[id]).Number, 0xFF);
            id++;
        }

        [TestMethod]
        public void test_strings()
        {
            Lexer lexer = new Lexer(@"""I am a string"" ""I am also a string""");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenString));
            Assert.AreEqual(lexer.Output[id].Value, "I am a string");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenString));
            Assert.AreEqual(lexer.Output[id].Value, "I am also a string");
            id++;
        }

        [TestMethod]
        public void test_strings_special_characters()
        {
            Lexer lexer = new Lexer(@" ""\t \\ \"""" ");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenString));
            Assert.AreEqual(lexer.Output[id].Value, "\\t \\\\ \\\"");
            id++;
        }

        [TestMethod]
        public void test_strings_multiline()
        {
            Lexer lexer = new Lexer(" @\"no multi-line\" @\"multi\n-line\" @\"super\nm\nu\nlti\n-line\" ");

            int id = 0;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenString));
            Assert.AreEqual(lexer.Output[id].Value, "no multi-line");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenString));
            Assert.AreEqual(lexer.Output[id].Value, "multi\\n-line");
            id++;

            Assert.IsInstanceOfType(lexer.Output[id], typeof(TokenString));
            Assert.AreEqual(lexer.Output[id].Value, "super\\nm\\nu\\nlti\\n-line");
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
            Assert.AreEqual(lexer.Output[id].Value, " Another\nmulti\nline\ncomment ");
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
        public void test_documentation_comments()
        {
            Lexer lexer = new Lexer(@"/// Comment /// Derp
identifier/// Next

/** Multiline in single line *//** Real
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
