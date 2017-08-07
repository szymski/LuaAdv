using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.CodeGenerators
{
    public class SourceCodeBuilder
    {
        private Stack<StringBuilder> _stringBuilders = new Stack<StringBuilder>();
        private Stack<int> _stringBuildersTabs = new Stack<int>();
        private Stack<bool> _stringBuildersTabsInserted = new Stack<bool>();
        private StringBuilder _currentStringBuilder;

        public string Output => _currentStringBuilder.ToString();

        public int Tabs
        {
            get { return _stringBuildersTabs.Peek(); }
            set
            {
                _stringBuildersTabs.Pop();
                _stringBuildersTabs.Push(value);
            }
        }

        public bool TabsAlreadyInserted
        {
            get { return _stringBuildersTabsInserted.Peek(); }
            set
            {
                _stringBuildersTabsInserted.Pop();
                _stringBuildersTabsInserted.Push(value);
            }
        }

        public SourceCodeBuilder()
        {
            PushStringBuilder();
        }

        public void Append(string text)
        {
            if (text.Contains("\n"))
            {
                var split = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                for (int i = 0; i < split.Length; i++)
                {
                    if (i < split.Length - 1)
                        AppendLine(split[i]);
                    else
                        Append(split[i]);
                }

                return;
            }

            if (!TabsAlreadyInserted)
            {
                for (int i = 0; i < Tabs; i++)
                    _currentStringBuilder.Append('\t');
                TabsAlreadyInserted = true;
            }

            _currentStringBuilder.Append(text);
        }

        public void Append(string text, params object[] args)
        {
            if (text.Contains("\n"))
            {
                var split = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                for (int i = 0; i < split.Length; i++)
                {
                    if (i < split.Length - 1)
                        AppendLine(split[i], args);
                    else
                        Append(split[i], args);
                }

                return;
            }

            if (!TabsAlreadyInserted)
            {
                for (int i = 0; i < Tabs; i++)
                    _currentStringBuilder.Append('\t');
                TabsAlreadyInserted = true;
            }

            _currentStringBuilder.AppendFormat(text, args);
        }

        public void AppendLine(string text)
        {
            if (text.Contains("\n"))
            {
                foreach (var line in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
                    if (line.Length != 0)
                        AppendLine(line);

                return;
            }

            if (!TabsAlreadyInserted && text.Length != 0)
            {
                for (int i = 0; i < Tabs; i++)
                    _currentStringBuilder.Append('\t');
                TabsAlreadyInserted = true;
            }

            _currentStringBuilder.Append(text);
            _currentStringBuilder.AppendLine();
            TabsAlreadyInserted = false;
        }

        public void AppendLine(string text, params object[] args)
        {
            if (text.Contains("\n"))
            {
                foreach (var line in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
                    if (line.Length != 0)
                        AppendLine(line, args);

                return;
            }

            if (!TabsAlreadyInserted && text.Length != 0)
            {
                for (int i = 0; i < Tabs; i++)
                    _currentStringBuilder.Append('\t');
                TabsAlreadyInserted = true;
            }

            _currentStringBuilder.AppendFormat(text, args);
            _currentStringBuilder.AppendLine();
            TabsAlreadyInserted = false;
        }

        public void AppendLine()
        {
            _currentStringBuilder.AppendLine();
            TabsAlreadyInserted = false;
        }

        public void PushStringBuilder()
        {
            _stringBuilders.Push(new StringBuilder());
            _stringBuildersTabs.Push(0);
            _stringBuildersTabsInserted.Push(false);
            _currentStringBuilder = _stringBuilders.Peek();
        }

        public void PopStringBuilder()
        {
            _stringBuilders.Pop();
            _stringBuildersTabs.Pop();
            _stringBuildersTabsInserted.Pop();
            _currentStringBuilder = _stringBuilders.Peek();
        }

        public override string ToString() => Output;
    }
}
