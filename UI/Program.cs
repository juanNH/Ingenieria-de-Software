using System;
using System.Windows.Forms;

namespace UI
{
    internal static class Program_380_jh
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            bool mostrarLogin = true;

            while (mostrarLogin)
            {
                using (Login_380_jh login = new Login_380_jh())
                {
                    if (login.ShowDialog() != DialogResult.OK)
                    {
                        break;
                    }
                }

                using (MainForm_380_jh mainForm = new MainForm_380_jh())
                {
                    System.Windows.Forms.Application.Run(mainForm);
                    mostrarLogin = mainForm.DialogResult == DialogResult.Retry;
                }
            }
        }
    }
}
