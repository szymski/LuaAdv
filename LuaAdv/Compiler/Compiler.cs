using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.CodeGenerators.Lua;
using LuaAdv.Compiler.SemanticAnalyzer;
using LuaAdv.Compiler.SemanticAnalyzer1;

namespace LuaAdv.Compiler
{
    public class Compiler
    {
        class IncludeCacheElement
        {
            public string filename;
            public string source;
            public DateTime lastUpdate;
            public Scope scope;
            public bool isGlobal;
        }

        private Dictionary<string, IncludeCacheElement> _includeCache = new Dictionary<string, IncludeCacheElement>();

        public string Compile(string filename, string source)
        {
            var lexer = new Lexer.Lexer(source);
            var syntaxAnalyzer = new SyntaxAnalyzer.SyntaxAnalyzer(lexer.Output);

            List<Scope> toJoinScopes = new List<Scope>();
            foreach(var cacheElement in _includeCache)
                if(cacheElement.Value.isGlobal && cacheElement.Value.filename != filename)
                    toJoinScopes.Add(cacheElement.Value.scope);

            var semanticAnalyzer1 = new SemanticAnalyzer1.SemanticAnalyzer1(syntaxAnalyzer.OutputNode, toJoinScopes.ToArray());
            syntaxAnalyzer.OutputNode.Accept(semanticAnalyzer1);
            var semanticAnalyzer2 = new SemanticAnalyzer2(semanticAnalyzer1.MainNode);
            semanticAnalyzer1.MainNode.Accept(semanticAnalyzer2);
            var codeGenerator = new LuaCodeGenerator(semanticAnalyzer2.MainNode);

            return codeGenerator.Output;
        }

        public void AddInclude(string filename, string source, bool global, DateTime lastModified)
        {
            if (!_includeCache.ContainsKey(filename))
                _includeCache.Add(filename, new IncludeCacheElement()
                {
                    filename = filename,
                    source = source,
                    lastUpdate = lastModified,
                    scope = CompileAndGetScope(source),
                    isGlobal = global,
                });
            else if (_includeCache[filename].lastUpdate < DateTime.Now)
            {
                _includeCache[filename].lastUpdate = lastModified;
                _includeCache[filename].scope = CompileAndGetScope(source);
            }
        }

        private Scope CompileAndGetScope(string source)
        {
            var lexer = new Lexer.Lexer(source);
            var syntaxAnalyzer = new SyntaxAnalyzer.SyntaxAnalyzer(lexer.Output);
            var semanticAnalyzer1 = new SemanticAnalyzer1.SemanticAnalyzer1(syntaxAnalyzer.OutputNode);
            syntaxAnalyzer.OutputNode.Accept(semanticAnalyzer1);
            var semanticAnalyzer2 = new SemanticAnalyzer2(semanticAnalyzer1.MainNode);
            semanticAnalyzer1.MainNode.Accept(semanticAnalyzer2);

            return semanticAnalyzer1.MainScope;
        }
    }
}
