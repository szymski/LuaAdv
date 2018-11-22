using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuaAdvWatcher
{
    static class Program
    {
        public const string ConfigFilename = ".lua_adv";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var configFilename = "";

#if DEBUG
            //configFilename = "Test/.lua_adv";
            configFilename =
                @"F:\Projects\LuaAdv\LuaAdvWatcher\bin\Debug\Test\.lua_adv";
#else
            if (args.Length == 0)
            {
                MessageBox.Show($"No file specified. Please open {ConfigFilename}.", "LuaAdv", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            configFilename = args[0];
#endif

            if (!File.Exists(configFilename))
            {
                MessageBox.Show("No such file!", "LuaAdv", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            configFilename = Path.GetFullPath(configFilename);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(configFilename));
        }
    }
}
