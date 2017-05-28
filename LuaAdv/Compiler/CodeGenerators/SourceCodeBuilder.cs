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
        private StringBuilder _currentStringBuilder;

        public string Output => _currentStringBuilder.ToString();

        public int Tabs { get; set; } = 0;
        private bool _tabsInserted = false;

        public SourceCodeBuilder()
        {
            PushStringBuilder();
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
            _currentStringBuilder = _stringBuilders.Peek();
        }

        public void PopStringBuilder()
        {
            _stringBuilders.Pop();
            _currentStringBuilder = _stringBuilders.Peek();
        }

        public override string ToString() => Output;
    }
}
