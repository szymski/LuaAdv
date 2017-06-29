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
        private bool _tabsInserted = false;

        public SourceCodeBuilder()
        {
            PushStringBuilder();
        }

        public void Append(string text)
        {
            if (!_tabsInserted)
            {
                for (int i = 0; i < Tabs; i++)
                    _currentStringBuilder.Append('\t');
                _tabsInserted = true;
            }

            _currentStringBuilder.Append(text);
        }

        public void Append(string text, params object[] args)
        {
            if (!_tabsInserted)
            {
                for (int i = 0; i < Tabs; i++)
                    _currentStringBuilder.Append('\t');
                _tabsInserted = true;
            }

            _currentStringBuilder.AppendFormat(text, args);
        }

        public void AppendLine(string text)
        {
            if (!_tabsInserted)
            {
                for (int i = 0; i < Tabs; i++)
                    _currentStringBuilder.Append('\t');
                _tabsInserted = true;
            }

            _currentStringBuilder.Append(text);
            _currentStringBuilder.AppendLine();
            _tabsInserted = false;
        }

        public void AppendLine(string text, params object[] args)
        {
            if (!_tabsInserted)
            {
                for (int i = 0; i < Tabs; i++)
                    _currentStringBuilder.Append('\t');
                _tabsInserted = true;
            }

            _currentStringBuilder.AppendFormat(text, args);
            _currentStringBuilder.AppendLine();
            _tabsInserted = false;
        }

        public void AppendLine()
        {
            _currentStringBuilder.AppendLine();
            _tabsInserted = false;
        }

        public void PushStringBuilder()
        {
            _stringBuilders.Push(new StringBuilder());
            _stringBuildersTabs.Push(0);
            _currentStringBuilder = _stringBuilders.Peek();
        }

        public void PopStringBuilder()
        {
            _stringBuilders.Pop();
            _stringBuildersTabs.Pop();
            _currentStringBuilder = _stringBuilders.Peek();
        }

        public override string ToString() => Output;
    }
}
