using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace LuaAdvWatcher {
    [SupportedOSPlatform("windows")]
    class FileAssociation {
        public static void RegisterConfigFileAssociation()
        {
            string extension = ".lua_adv";
            string progID = "LuaAdv";
            string description = "LuaAdv Config File";
            string icon = "LuaAdvWatcher.exe";
            string application = System.Reflection.Assembly.GetExecutingAssembly().Location;

            // if (!IsAssociated(extension))
            Associate(extension, progID, description, icon, application);
        }

        // Associate file extension with progID, description, icon and application
        private static void Associate(
            string extension,
            string progID,
            string description,
            string icon,
            string application)
        {
            Registry.ClassesRoot.CreateSubKey(extension)?.SetValue("", progID);
            if (progID is not { Length: > 0 }) return;
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(progID))
            {
                if (description != null)
                    key.SetValue("", description);
                if (icon != null)
                    key.CreateSubKey("DefaultIcon").SetValue("", ToShortPathName(icon));
                if (application != null)
                    key.CreateSubKey(@"Shell\Open\Command").SetValue("", ToShortPathName(application) + " \"%1\"");
            }
        }

        // Return true if extension already associated in registry
        public static bool IsAssociated(string extension)
        {
            return (Registry.ClassesRoot.OpenSubKey(extension, false) != null);
        }

        [DllImport("Kernel32.dll")]
        private static extern uint GetShortPathName(
            string lpszLongPath,
            [Out] StringBuilder lpszShortPath,
            uint cchBuffer);

        // Return short path format of a file name
        private static string ToShortPathName(string longName)
        {
            StringBuilder s = new StringBuilder(1000);
            uint iSize = (uint)s.Capacity;
            uint iRet = GetShortPathName(longName, s, iSize);
            return s.ToString();
        }
    }
}