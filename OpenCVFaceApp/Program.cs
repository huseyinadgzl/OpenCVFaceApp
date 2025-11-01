using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenCVFaceApp
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 mainForm = null;
            try
            {
                // Instantiate the form separately so constructor/InitializeComponent exceptions are caught
                mainForm = new Form1();
                
            }
            catch (Exception ex)
            {
                // Show full exception details (stack trace + inner exceptions)
                MessageBox.Show(ex.ToString(), "Startup error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(mainForm);
        }
    }
}
