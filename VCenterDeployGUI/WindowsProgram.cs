using System;
using System.Windows.Forms;

namespace VCenterDeployGUI
{
    internal static class WindowsProgram
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}