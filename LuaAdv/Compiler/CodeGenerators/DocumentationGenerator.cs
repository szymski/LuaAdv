using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaAdv.Compiler.SemanticAnalyzer;

namespace LuaAdv.Compiler.CodeGenerators
{
    public struct DocumentationFile
    {
        public string Path { get; set; }
        public DocumentationFunction[] Functions { get; set; }
        public DocumentationClass[] Classes { get; set; }
    }

    public class DocumentationGenerator
    {
        private Dictionary<string, string> _files = new Dictionary<string, string>();
        private Dictionary<string, DocumentationFile> _documentationFiles = new Dictionary<string, DocumentationFile>();

        private string _outputDir;

        public void AddFile(string filename, string contents)
        {
            if (_files.ContainsKey(filename))
                throw new ArgumentException("This file has already been added.", nameof(filename));

            _files.Add(filename, contents);
        }

        /// <summary>
        /// Generates documentation for all files.
        /// </summary>
        /// <returns>A dictionairy where first item is the name of the HTML file 
        /// and the second is the HTML contents of the file.</returns>
        public Dictionary<string, string> Generate(string outputDir)
        {
            _outputDir = outputDir;

            foreach (var file in _files)
                ProcessFile(file.Key, file.Value);

            var result = new Dictionary<string, string>();

            foreach (var docFile in _documentationFiles)
                result.Add($"{docFile.Key}.html", GenerateHtmlForFile(docFile.Value));

            return result;
        }

        private void ProcessFile(string filename, string contents)
        {
            var lexer = new Lexer.Lexer(contents);
            var syntaxAnalyzer = new SyntaxAnalyzer.SyntaxAnalyzer(lexer.Output);
            var semanticAnalyzer1 = new SemanticAnalyzer1.SemanticAnalyzer1(syntaxAnalyzer.OutputNode);
            syntaxAnalyzer.OutputNode.Accept(semanticAnalyzer1);
            var semanticAnalyzer2 = new SemanticAnalyzer2(semanticAnalyzer1.MainNode);
            semanticAnalyzer1.MainNode.Accept(semanticAnalyzer2);
            var docsVisitor = new DocumentationVisitor(semanticAnalyzer2.MainNode);
            semanticAnalyzer2.MainNode.Accept(docsVisitor);

            DocumentationFile file = new DocumentationFile();
            file.Path = filename;
            file.Functions = docsVisitor.Functions.ToArray();
            file.Classes = docsVisitor.Classes.ToArray();

            _documentationFiles.Add(filename, file);
        }

        private string GenerateHtmlForFile(DocumentationFile file)
        {
            StringBuilder b = new StringBuilder();

            b.AppendLine("<!DOCTYPE html>");
            b.AppendLine("<html lang=\"en\">");
            b.AppendLine("<head>");
            b.AppendFormat("<link rel='stylesheet' href='{0}'>\n", GetRelativePath("style.css", file.Path));
            b.AppendLine("</head>");
            b.AppendLine("<body>");

            b.AppendLine("<div class='menu'>");
            GenerateMenu(b, file.Path);
            b.AppendLine("</div>");

            b.AppendLine("<div class='content'>");

            b.AppendFormat("<h1>{0}</h1>\n", file.Path);

            #region Classes

            b.AppendFormat("<h2 class='section'>Classes</h2>");

            foreach (var docClass in file.Classes)
            {
                b.AppendLine("<div class='class'>");
                b.AppendFormat("<h3 class='class_name'><span class='function_keyword'>class</span> ");
                b.AppendFormat("{0}", docClass.Name);
                if(docClass.BaseClass != null)
                    b.AppendFormat(" : {0}", docClass.BaseClass);
                b.AppendLine("</h3>");

                b.AppendLine("<div class='function_description_box'>");
                b.Append("<p>");
                b.Append(docClass.Comment);
                b.Append("</p>");
                b.AppendLine("</div>");

                b.AppendFormat("<h3 class='class_section'>Fields</h3>");
                b.AppendLine("<div class='class_fields'>");
                foreach (var field in docClass.Fields)
                    GenerateClassField(b, field);
                b.AppendLine("</div>");

                b.AppendFormat("<h3 class='class_section'>Methods</h3>");
                b.AppendLine("<div class='class_methods'>");
                foreach (var method in docClass.Methods)
                    GenerateClassMethod(b, method);
                b.AppendLine("</div>");

                b.AppendLine("</div>");
            }

            #endregion

            #region Global variables

            b.AppendFormat("<h2 class='section'>Fields</h2>");

            #endregion

            #region Global Functions

            b.AppendFormat("<h2 class='section'>Functions</h2>");
            foreach (var func in file.Functions.Where(f => !f.Local))
                GenerateFunction(b, func);

            #endregion

            #region Local Functions

            b.AppendFormat("<h2 class='section'>Local functions</h2>");
            foreach (var func in file.Functions.Where(f => f.Local))
                GenerateFunction(b, func);

            #endregion

            b.AppendLine("</div>");

            b.AppendLine("</body>");
            b.AppendLine("</html>");

            return b.ToString();
        }

        private string GetRelativePath(string targetPath, string filePath)
        {
            Uri currentDir = new Uri(_outputDir + "/" + Path.GetDirectoryName(filePath));
            Uri targetFileUri = new Uri(_outputDir + "/" + targetPath);
            return currentDir.MakeRelativeUri(targetFileUri).ToString();
        }

        private void GenerateMenu(StringBuilder b, string currentFilePath)
        {
            
        }

        private void GenerateFunction(StringBuilder b, DocumentationFunction func)
        {
            b.AppendLine("<div class='function'>");

            b.AppendFormat("<h3 class='function_name'>");
            if (func.Local)
                b.Append("local ");
            b.AppendFormat("<span class='function_keyword'>function</span> {0}", func.FullName);
            b.Append("(");
            for (int i = 0; i < func.Parameters.Length; i++)
            {
                var param = func.Parameters[i];
                b.AppendFormat("<span class='parameter'>{0}</span>", param.Name);

                if (param.DefaultValue != null)
                    b.AppendFormat(" = <span class='parameter_default'>{0}</span>", param.DefaultValue.Token.Value);

                if (i != func.Parameters.Length - 1)
                    b.Append(", ");
            }
            b.Append(")");
            b.AppendLine("</h3>");
            b.AppendLine("<div class='function_description_box'>");
            b.Append("<p>");
            b.Append(func.Comment);
            b.Append("</p>");
            b.AppendLine("</div>");

            b.AppendLine("</div>");
        }

        private void GenerateClassMethod(StringBuilder b, DocumentationClassMethod method)
        {
            b.AppendLine("<div class='function'>");

            b.AppendFormat("<h3 class='function_name'>");
            b.AppendFormat("<span class='function_keyword'>method</span> {0}", method.Name);
            b.Append("(");
            for (int i = 0; i < method.Parameters.Length; i++)
            {
                var param = method.Parameters[i];
                b.AppendFormat("<span class='parameter'>{0}</span>", param.Name);

                if (param.DefaultValue != null)
                    b.AppendFormat(" = <span class='parameter_default'>{0}</span>", param.DefaultValue.Token.Value);

                if (i != method.Parameters.Length - 1)
                    b.Append(", ");
            }
            b.Append(")");
            b.AppendLine("</h3>");
            b.AppendLine("<div class='function_description_box'>");
            b.Append("<p>");
            b.Append(method.Comment);
            b.Append("</p>");
            b.AppendLine("</div>");

            b.AppendLine("</div>");
        }

        private void GenerateClassField(StringBuilder b, DocumentationClassField field)
        {
            b.AppendLine("<p class='class_field'>");
            b.AppendFormat("<span class = 'class_field_declaration'><span class='function_keyword'>var</span> {0}", field.Name);
            if (field.DefaultValue != null)
                b.AppendFormat(" = {0}", field.DefaultValue.Token.Value);
            b.Append("</span>");
            b.AppendFormat(" - {0}", field.Comment);
            b.AppendLine();
            b.AppendLine("</p>");
        }
    }
}
