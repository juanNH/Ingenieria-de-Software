using System;
using Abstractions;
using Domain;
using Repository;

namespace Application
{
    public class IntegridadApplicationService_380_jh
    {
        private readonly IntegridadRepository_380_jh _repository_380_jh = new IntegridadRepository_380_jh();
        public static event EventHandler EstadoCambiado_380_jh;
        private static bool _falloNotificado_380_jh;
        public static InformeIntegridad_380_jh Estado_380_jh { get; private set; } = new InformeIntegridad_380_jh();

        // Antes de iniciar sesion, solo se habilita el acceso si se pudo verificar la integridad.
        public bool VerificarParaAcceso_380_jh()
        {
            try
            {
                InformeIntegridad_380_jh informe = _repository_380_jh.Verificar_380_jh(0);
                return informe.Disponible_380_jh && informe.EsValida_380_jh;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Integrity access check: {0}", ex.GetType().Name);
                return false;
            }
        }

        public InformeIntegridad_380_jh Verificar_380_jh()
        {
            var usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            InformeIntegridad_380_jh informe;
            try
            {
                informe = _repository_380_jh.Verificar_380_jh(usuario == null ? 0 : usuario.Id_380_jh);
                _falloNotificado_380_jh = false;
            }
            catch (Exception ex)
            {
                informe = new InformeIntegridad_380_jh(); // Error de conexion/esquema => bloqueo, nunca OK.
                System.Diagnostics.Trace.TraceError("Integrity check: {0}", ex.GetType().Name);
                if (!_falloNotificado_380_jh)
                {
                    _falloNotificado_380_jh = true;
                    new BitacoraApplicationService_380_jh().RegistrarEvento_380_jh(
                        BitacoraModulo_380_jh.Integridad_380_jh, BitacoraAccion_380_jh.OperacionFallida_380_jh,
                        BitacoraNivel_380_jh.Error_380_jh, usuario, "INTEGRITY_UNAVAILABLE");
                }
            }
            Estado_380_jh = informe;
            EstadoCambiado_380_jh?.Invoke(this, EventArgs.Empty);
            return informe;
        }

        public string Recuperar_380_jh(string revision, bool inicializar)
        {
            var usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            string resultado;
            if (usuario == null)
                resultado = "OPERATION_NOT_AUTHORIZED";
            else
            {
                try { resultado = _repository_380_jh.Recuperar_380_jh(usuario.Id_380_jh, revision, inicializar); }
                catch { resultado = "INTEGRITY_RECOVERY_FAILED"; }
            }
            // Los exitos se registran atomicamente en SQL junto con las versiones y los digitos.
            if (resultado != "INTEGRITY_RECOVERED")
                new BitacoraApplicationService_380_jh().RegistrarEvento_380_jh(
                    BitacoraModulo_380_jh.Integridad_380_jh, BitacoraAccion_380_jh.RestauracionFallida_380_jh,
                    BitacoraNivel_380_jh.Error_380_jh, usuario, resultado);
            Verificar_380_jh();
            return resultado;
        }
    }
}
