using System;
using System.Runtime.InteropServices;
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

                DesconectarProveedoresAccesibilidad_380_jh();
            }
        }

        private static void DesconectarProveedoresAccesibilidad_380_jh()
        {
            // La API existe desde Windows 8 / Server 2012; se conserva el soporte anterior.
            if (Environment.OSVersion.Version < new Version(6, 2))
            {
                return;
            }

            // Run y Dispose ya terminaron. El siguiente Login no debe atender proveedores
            // UIA retenidos de celdas destruidas. Los controles nuevos publican los suyos.
            Marshal.ThrowExceptionForHR(UiaDisconnectAllProviders_380_jh());
        }

        [DllImport("UIAutomationCore.dll", EntryPoint = "UiaDisconnectAllProviders", ExactSpelling = true)]
        private static extern int UiaDisconnectAllProviders_380_jh();
    }
}
