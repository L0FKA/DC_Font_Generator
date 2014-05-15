using System;
using System.Windows.Forms;

namespace DC_Font_Generator
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Environment.Version.Major < 2 && Environment.Version.Build < 50727)
            {
                MessageBox.Show(".NET Framework need 2.0 or later.", ".NET Framework Version", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Application.Run(new MainForm());
        }
    }
}
