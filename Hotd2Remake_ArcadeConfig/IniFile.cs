using System.Runtime.InteropServices;
using System.Text;

namespace Hotd2Remake_ArcadeConfig
{
    /// <summary>
    /// Windows INI functions : the comments of the file are kept when a value is written
    /// </summary>
    public class IniFile
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public string FilePath { get; private set; }

        public IniFile(string Path)
        {
            FilePath = Path;
        }

        public string Read(string Section, string Key)
        {
            StringBuilder Value = new StringBuilder(255);
            GetPrivateProfileString(Section, Key, string.Empty, Value, 255, FilePath);
            return Value.ToString().Trim();
        }

        public void Write(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, FilePath);
        }
    }
}
