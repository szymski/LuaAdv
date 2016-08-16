using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaAdv.Compiler.CodeGenerators
{
    public class SourceCodeBuilder
    {
        private Stack<StringBuilder> stringBuilders = new Stack<StringBuilder>();
        private StringBuilder currentStringBuilder;

        public string Output => currentStringBuilder.ToString();

        public int Tabs { get; set; } = 0;
        private bool tabsInserted = false;

        public SourceCodeBuilder()
        {
            PushStringBuilder();
        }

        public void Append(string text, params object[] args)
        {
            if (!tabsInserted)
            {
                for (int i = 0; i < Tabs; i++)
                    currentStringBuilder.Append('\t');
                tabsInserted = true;
            }

            currentStringBuilder.AppendFormat(text, args);
        }

        public void AppendLine(string text, params object[] args)
        {
            if (!tabsInserted)
            {
                for (int i = 0; i < Tabs; i++)
                    currentStringBuilder.Append('\t');
                tabsInserted = true;
            }

            currentStringBuilder.AppendFormat(text, args);
            currentStringBuilder.AppendLine();
            tabsInserted = false;
        }

        public void AppendLine()
        {
            currentStringBuilder.AppendLine();
            tabsInserted = false;
        }

        public void PushStringBuilder()
        {
            stringBuilders.Push(new StringBuilder());
            currentStringBuilder = stringBuilders.Peek();
        }

        public void PopStringBuilder()
        {
            stringBuilders.Pop();
            currentStringBuilder = stringBuilders.Peek();
        }

        public override string ToString() => Output;
    }
}
