namespace Domain
{
    public static class BitacoraRegistroMapper_380_jh
    {
        public static BitacoraRegistro_380_jh Mapear_380_jh(IBitacoraEvento_380_jh bitacora)
        {
            if (bitacora == null)
            {
                return null;
            }

            return new BitacoraRegistro_380_jh
            {
                Id_380_jh = bitacora.Id_380_jh,
                IdUsuario_380_jh = bitacora.IdUsuario_380_jh,
                IdentificadorUsuario_380_jh = bitacora.IdentificadorUsuario_380_jh,
                Modulo_380_jh = bitacora.Modulo_380_jh.ToString(),
                Accion_380_jh = bitacora.Accion_380_jh.ToString(),
                Nivel_380_jh = bitacora.Nivel_380_jh.ToString(),
                Descripcion_380_jh = bitacora.Descripcion_380_jh,
                Equipo_380_jh = bitacora.Equipo_380_jh,
                Fecha_380_jh = bitacora.Fecha_380_jh
            };
        }
    }
}
