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

        private bool TabsAlreadyInserted
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
            if (!TabsAlreadyInserted)
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
            if (!TabsAlreadyInserted)
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
