using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Hotd2Remake_ArcadeConfig
{
    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main()
        {
            //Sharp texts on high DPI screens (WndMain is scaling its sizes)
            SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WndMain());
        }
    }
}
