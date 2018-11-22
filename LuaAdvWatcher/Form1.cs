using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LuaAdv.Compiler;
using LuaAdv.Compiler.CodeGenerators;
using Newtonsoft.Json.Linq;

namespace LuaAdvWatcher
{
    public partial class Form1 : Form
    {
        private string _configFilename = "";

        private string _comment = "";
        private string _inputDirectory = "";
        private string _outputDirectory = "";
        private string _docsDirectory = "";
        private string[] _toIncludeFiles = new string[0];
        private bool _obfuscate = false;

        private Compiler _compiler = new Compiler();

        public Form1(string configFilename)
        {
            _configFilename = configFilename;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitTrayMenu();
            ParseConfig();
            CreateWatcher();
        }

        private void InitTrayMenu()
        {
            trayIcon.Icon = Icon;
            trayIcon.Visible = true;

            var context = new ContextMenu();

            context.MenuItems.Add("Compile all", (o, args) => CompileAll());
            context.MenuItems.Add("Generate documentation", (o, args) => GenerateDocumentation());
            context.MenuItems.Add("Exit", (o, args) => Close());

            trayIcon.ContextMenu = context;

            trayIcon.ShowBalloonTip(8000, "LuaAdv is running", "Now you can code in LuaAdv and every change will be compiled and put into the output folder.", ToolTipIcon.Info);
        }

        private void CreateWatcher()
        {
            FileSystemWatcher watcher = new FileSystemWatcher(_inputDirectory);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.lua*"; // .luaa and .lua files
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            DateTime lastCompile = DateTime.Now;

            watcher.Changed += (sender, args) =>
            {
                // For some reason, files changes are reported twice, so we have to wait a while
                if (DateTime.Now.Subtract(lastCompile).Seconds < 1)
                    return;

                lastCompile = DateTime.Now;

                ProcessFile(new FileInfo(args.FullPath));
            };
        }

        private void ParseConfig()
        {
            var contents = File.ReadAllText(_configFilename);
            var obj = JObject.Parse(contents);

            string configFileDirectory = Path.GetDirectoryName(_configFilename);

            Directory.SetCurrentDirectory(configFileDirectory);

            _inputDirectory = obj["input_dir"]?.Value<string>() ?? configFileDirectory;
            _outputDirectory = obj["output_dir"]?.Value<string>() ?? configFileDirectory;
            _docsDirectory = obj["docs_dir"]?.Value<string>() ?? configFileDirectory + "/docs/";
            _obfuscate = obj["obfuscate"]?.Value<bool>() ?? false;
            _comment = "";
            if (obj["comment"] != null)
                foreach (var line in obj["comment"].Values())
                    _comment += line.Value<string>() + "\n";
            _comment += "\n";
            if (obj["includes"] != null)
                _toIncludeFiles = obj["includes"].Values().Select(v => v.Value<string>()).ToArray();

            _inputDirectory = Path.GetFullPath(_inputDirectory);
            _outputDirectory = Path.GetFullPath(_outputDirectory);
            _docsDirectory = Path.GetFullPath(_docsDirectory);
        }

        private bool CompileAll(string dir = null, int level = 0)
        {
            if (dir == null)
                dir = _inputDirectory;

            foreach (var file in new DirectoryInfo(dir).EnumerateFiles()
                .Where(f => f.Extension == ".luaa" || f.Extension == ".lua"))
                if (!ProcessFile(file))
                    return false;

            // Do the same for all folders recursively
            foreach (var d in new DirectoryInfo(dir).EnumerateDirectories())
                if (!CompileAll(d.FullName, level++))
                    return false;

            if (level == 0)
            {
                trayIcon.ShowBalloonTip(8000, "LuaAdv", "Successfully compiled all files!", ToolTipIcon.Info);
            }

            return true;
        }

        private void GenerateDocumentation()
        {
            try
            {
                DocumentationGenerator docGen = new DocumentationGenerator();
                AddDocumentationFiles(docGen);
                var result = docGen.Generate(_docsDirectory);

                if (!Directory.Exists(_docsDirectory))
                    Directory.CreateDirectory(_docsDirectory);

                foreach (var file in result)
                {
                    string outputDir = _docsDirectory + "/" + file.Key;

                    WriteToFile(outputDir, file.Value);
                }

                trayIcon.ShowBalloonTip(8000, "LuaAdv", "Documentation successfully generated!", ToolTipIcon.Info);
            }
            catch (Exception e)
            {
                trayIcon.ShowBalloonTip(8000, "LuaAdv", "An error occured while generating the documentation!", ToolTipIcon.Error);
                throw e;
            }
        }

        private bool AddDocumentationFiles(DocumentationGenerator docGen, string dir = null, int level = 0)
        {
            if (dir == null)
                dir = _inputDirectory;

            foreach (var file in new DirectoryInfo(dir).EnumerateFiles()
                .Where(f => f.Extension == ".luaa"))
            {
                string relativeFilename = file.FullName.Replace(_inputDirectory, "");
                docGen.AddFile(relativeFilename, File.ReadAllText(file.FullName));
            }

            // Do the same for all folders recursively
            foreach (var d in new DirectoryInfo(dir).EnumerateDirectories())
                if (!AddDocumentationFiles(docGen, d.FullName, level++))
                    return false;

            return true;
        }

        private bool ProcessFile(FileInfo file)
        {
            var outputFilename = _outputDirectory + "\\" + file.FullName.Replace(_inputDirectory, "")
                .Replace(".luaa", ".lua");

            string contents = null;

            for (int i = 10; i != 0; i--)
            {
                // Just after the file has been modified, we cannot access it, so let's wait a while
                Thread.Sleep(20);

                try
                {
                    contents = File.ReadAllText(file.FullName);
                    break;
                }
                catch
                {
                    if (i == 0)
                        return false;
                }
            }

            // Lua files are just copied to the output dir
            if (file.Extension == ".lua")
                WriteToFile(outputFilename, contents); // TODO: Should it also include the comment here?
            else
            {
                try
                {
                    WriteToFile(outputFilename, CompileLuaAdvFile(file.Name, contents));
                }
                catch (CompilerException e)
                {
                    trayIcon.ShowBalloonTip(12000, $"{file.Name} {e.Line}:{e.Character}", e.Message, ToolTipIcon.Error);
                    return false;
                }
                catch (Exception e)
                {
#if DEBUG
                    throw e;
#endif
                    trayIcon.ShowBalloonTip(12000, $"Unknown exception in {file.Name}", e.Message, ToolTipIcon.Error);
                    return false;
                }
            }

            return true;
        }

        private string CompileLuaAdvFile(string filename, string source)
        {
            AddIncludesToCompiler();
            string preparedComment = PrepareComment(filename);
            return preparedComment + _compiler.Compile(filename, source, _obfuscate, filename);
        }

        private void AddIncludesToCompiler()
        {
            foreach (var filename in _toIncludeFiles)
            {
                try
                {
                    _compiler.AddInclude(filename, File.ReadAllText(Path.Combine(_inputDirectory, filename)), true, File.GetLastWriteTime(filename));
                }
                catch (CompilerException e)
                {
                    trayIcon.ShowBalloonTip(12000, $"{filename} {e.Line}:{e.Character}", e.Message, ToolTipIcon.Error);
                }
                catch (Exception e)
                {
#if DEBUG
                    throw e;
#endif
                    trayIcon.ShowBalloonTip(12000, $"Unknown exception in {filename}", e.Message, ToolTipIcon.Error);
                }
            }
        }

        private string PrepareComment(string filename)
        {
            string comment = _comment;

            comment = comment.Replace("%filename%", filename);
            comment = comment.Replace("%date%", DateTime.Now.ToString("d"));
            comment = comment.Replace("%time%", DateTime.Now.ToString("T"));
            comment = comment.Replace("%year%", DateTime.Now.Year.ToString());

            return comment;
        }

        private void WriteToFile(string path, string contents)
        {
            // Create the directory if doesn't exist
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            File.WriteAllText(path, contents);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
