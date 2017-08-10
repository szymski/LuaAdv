using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.Nodes;
using LuaAdv.Compiler.Nodes.Expressions;
using LuaAdv.Compiler.Nodes.Expressions.BasicTypes;
using LuaAdv.Compiler.Nodes.Statements;
using LuaAdv.Compiler.SemanticAnalyzer1;

namespace LuaAdv.Compiler.CodeGenerators
{
    public struct DocumentationFunction
    {
        /// <summary>
        /// Full name of the function, with table index.
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// Only the name of the function, without table index.
        /// </summary>
        public string TopLevelName { get; set; }
        public bool Local { get; set; }
        public Token Token { get; set; }
        public string Comment { get; set; }
        public DocumentationFunctionParameter[] Parameters { get; set; }
    }

    public struct DocumentationFunctionParameter
    {
        public string Name { get; set; }
        public Node DefaultValue { get; set; }
    }

    public struct DocumentationClassField
    {
        public string Name { get; set; }
        public Token Token { get; set; }
        public string Comment { get; set; }
        public Node DefaultValue { get; set; }
    }

    public struct DocumentationClassMethod
    {
        public string Name { get; set; }
        public Token Token { get; set; }
        public string Comment { get; set; }
        public DocumentationFunctionParameter[] Parameters { get; set; }
    }

    public struct DocumentationClass
    {
        public string Name { get; set; }
        /// <summary>
        /// Can be null
        /// </summary>
        public string BaseClass { get; set; }
        public string Comment { get; set; }
        public DocumentationClassField[] Fields { get; set; }
        public DocumentationClassMethod[] Methods { get; set; }
    }

    public class DocumentationVisitor : TransparentVisitor
    {
        private DocumentationCommentNode _lastCommentNode;

        public List<DocumentationFunction> Functions { get; } = new List<DocumentationFunction>();
        public List<DocumentationClass> Classes { get; } = new List<DocumentationClass>();

        public DocumentationVisitor(Node mainNode) : base(mainNode)
        {

        }

        public override Node Visit(DocumentationCommentNode node)
        {
            _lastCommentNode = node;
            return base.Visit(node);
        }

        public override Node Visit(StatementFunctionDeclaration node)
        {
            Functions.Add(new DocumentationFunction()
            {
                TopLevelName = GetFunctionTopLevelname(node.name),
                FullName = GetFunctionFullName(node.name),
                Token = node.Token,
                Parameters = node.parameterList.Select(p => new DocumentationFunctionParameter()
                {
                    Name = p.Item2,
                    DefaultValue = p.Item3,
                }).ToArray(),
                Comment = _lastCommentNode != null ? _lastCommentNode.Token.Value : "No description",
                Local = node.local,
            });

            _lastCommentNode = null;

            return base.Visit(node);
        }

        public override Node Visit(Class node)
        {
            DocumentationClass docClass = new DocumentationClass()
            {
                Name = node.name,
                BaseClass = node.baseClass,
                Comment = _lastCommentNode != null ? _lastCommentNode.Token.Value : "No description",
                Methods = node.methods.Select(m => new DocumentationClassMethod()
                {
                    Name = m.Item1,
                    Token = new TokenSymbol(), // TODO: Tokens for class methods
                    Comment = m.Item4 != null ? m.Item4.Value : "No description",
                    Parameters = m.Item2.Select(p => new DocumentationFunctionParameter()
                    {
                        Name = p.Item2,
                        DefaultValue = p.Item3,
                    }).ToArray(),
                }).ToArray(),
                Fields = node.fields.Select(f => new DocumentationClassField()
                {
                    Name = f.Item1,
                    Token = new TokenSymbol(), // TODO: Tokens for class fields
                    DefaultValue = f.Item2,
                    Comment = f.Item3 != null ? f.Item3.Value : "No description",
                }).ToArray(),
            };

            Classes.Add(docClass);

            _lastCommentNode = null;

            return base.Visit(node);
        }

        private string GetFunctionFullName(Node node)
        {
            if (node is Variable)
                return (node as Variable).name;
            else if (node is TableDotIndex)
                return $"{GetFunctionFullName((node as TableDotIndex).table)}.{(node as TableDotIndex).index}";
            else
                return "INVALID";
        }

        private string GetFunctionTopLevelname(Node node)
        {
            if (node is Variable)
                return (node as Variable).name;
            else
                return "INVALID";
        }
    }
}
