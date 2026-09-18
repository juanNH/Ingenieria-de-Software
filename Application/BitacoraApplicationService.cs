using System.Collections.Generic;
using System;
using Abstractions;
using Domain;
using Repository;

namespace Application
{
    public class BitacoraApplicationService_380_jh
    {
        private readonly BitacoraRepository_380_jh _bitacoraRepository_380_jh;

        public BitacoraApplicationService_380_jh()
            : this(new BitacoraRepository_380_jh())
        {
        }

        public BitacoraApplicationService_380_jh(BitacoraRepository_380_jh bitacoraRepository)
        {
            _bitacoraRepository_380_jh = bitacoraRepository;
        }

        public List<BitacoraRegistro_380_jh> Listar_380_jh()
        {
            return Listar_380_jh(null);
        }

        public List<BitacoraRegistro_380_jh> Listar_380_jh(BitacoraFiltro_380_jh filtro)
        {
            return _bitacoraRepository_380_jh.Listar_380_jh(filtro);
        }

        public bool RegistrarEvento_380_jh(
            BitacoraModulo_380_jh modulo,
            BitacoraAccion_380_jh accion,
            BitacoraNivel_380_jh nivel,
            Usuario_380_jh usuario,
            string descripcion)
        {
            Bitacora_380_jh evento = new Bitacora_380_jh
            {
                IdUsuario_380_jh = usuario == null ? (int?)null : usuario.Id_380_jh,
                IdentificadorUsuario_380_jh = usuario == null ? null : usuario.Username_380_jh,
                Modulo_380_jh = modulo,
                Accion_380_jh = accion,
                Nivel_380_jh = nivel,
                Descripcion_380_jh = descripcion,
                Equipo_380_jh = Environment.MachineName,
                Fecha_380_jh = DateTime.Now
            };

            return _bitacoraRepository_380_jh.Registrar_380_jh(evento);
        }
    }
}
