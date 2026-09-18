using System;
using System.Windows.Forms;
using Application;

namespace UI
{
    internal sealed class ProteccionIntegridad_380_jh : IDisposable
    {
        private readonly Control _vista_380_jh;
        private readonly Button[] _acciones_380_jh;
        public ProteccionIntegridad_380_jh(Control vista, params Button[] acciones)
        {
            _vista_380_jh = vista;
            _acciones_380_jh = acciones;
            IntegridadApplicationService_380_jh.EstadoCambiado_380_jh += Actualizar_380_jh;
            vista.Disposed += delegate { Dispose(); };
            Actualizar_380_jh(this, EventArgs.Empty);
        }
        private void Actualizar_380_jh(object sender, EventArgs e)
        {
            if (_vista_380_jh.IsDisposed) return;
            foreach (Button boton in _acciones_380_jh)
                boton.Enabled = IntegridadApplicationService_380_jh.Estado_380_jh.EsValida_380_jh;
        }
        public void Dispose() => IntegridadApplicationService_380_jh.EstadoCambiado_380_jh -= Actualizar_380_jh;
    }
}
