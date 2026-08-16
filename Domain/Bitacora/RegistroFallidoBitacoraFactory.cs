using Abstractions;

namespace Domain
{
    public class RegistroFallidoBitacoraFactory_380_jh : BitacoraFactory_380_jh
    {
        protected override IBitacoraEvento_380_jh CrearEvento_380_jh()
        {
            return new Bitacora_380_jh
            {
                Modulo_380_jh = BitacoraModulo_380_jh.Seguridad_380_jh,
                Accion_380_jh = BitacoraAccion_380_jh.RegistroFallido_380_jh,
                Nivel_380_jh = BitacoraNivel_380_jh.Advertencia_380_jh
            };
        }
    }
}
