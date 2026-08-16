using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Domain;
using Repository;

namespace Application
{
    public class AuditoriaApplicationService_380_jh
    {
        private readonly AuditoriaRepository_380_jh _auditoriaRepository_380_jh;
        private readonly JavaScriptSerializer _serializer_380_jh;

        public AuditoriaApplicationService_380_jh()
            : this(new AuditoriaRepository_380_jh())
        {
        }

        public AuditoriaApplicationService_380_jh(AuditoriaRepository_380_jh auditoriaRepository)
        {
            _auditoriaRepository_380_jh = auditoriaRepository;
            _serializer_380_jh = new JavaScriptSerializer();
        }

        public bool RegistrarModificacion_380_jh(AuditoriaMemento_380_jh estadoAnterior, AuditoriaMemento_380_jh estadoNuevo)
        {
            return RegistrarCambio_380_jh(estadoAnterior, estadoNuevo, "UPDATE");
        }

        public bool RegistrarAlta_380_jh(AuditoriaMemento_380_jh estadoNuevo)
        {
            return RegistrarCambio_380_jh(null, estadoNuevo, "CREATE");
        }

        public bool RegistrarCambio_380_jh(AuditoriaMemento_380_jh estadoAnterior, AuditoriaMemento_380_jh estadoNuevo, string accion)
        {
            if (estadoNuevo == null || estadoNuevo.Entidad_380_jh != "Usuario")
            {
                return true;
            }

            if (estadoAnterior == null || estadoNuevo == null)
            {
                estadoAnterior = new AuditoriaMemento_380_jh(estadoNuevo.Entidad_380_jh, estadoNuevo.IdEntidad_380_jh, new Dictionary<string, object>());
            }

            if (estadoAnterior.Entidad_380_jh != estadoNuevo.Entidad_380_jh || estadoAnterior.IdEntidad_380_jh != estadoNuevo.IdEntidad_380_jh)
            {
                return false;
            }

            List<AuditoriaCambio_380_jh> cambios = CalcularCambios_380_jh(estadoAnterior, estadoNuevo);

            if (cambios.Count == 0)
            {
                return true;
            }

            Usuario_380_jh usuarioActor = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();

            AuditoriaRegistro_380_jh auditoria = new AuditoriaRegistro_380_jh
            {
                Entidad_380_jh = estadoNuevo.Entidad_380_jh,
                IdEntidad_380_jh = estadoNuevo.IdEntidad_380_jh,
                Accion_380_jh = string.IsNullOrWhiteSpace(accion) ? "UPDATE" : accion.Trim(),
                IdUsuarioActor_380_jh = usuarioActor?.Id_380_jh,
                IdentificadorUsuarioActor_380_jh = usuarioActor?.Username_380_jh,
                FechaEvento_380_jh = DateTime.Now,
                EstadoAnteriorJson_380_jh = _serializer_380_jh.Serialize(estadoAnterior.Estado_380_jh),
                EstadoNuevoJson_380_jh = _serializer_380_jh.Serialize(estadoNuevo.Estado_380_jh),
                CambiosJson_380_jh = _serializer_380_jh.Serialize(cambios)
            };

            return _auditoriaRepository_380_jh.Registrar_380_jh(auditoria);
        }

        public bool RegistrarSnapshot_380_jh(string entidad, int idEntidad, string accion, IDictionary<string, object> estado)
        {
            return RegistrarCambio_380_jh(
                new AuditoriaMemento_380_jh(entidad, idEntidad, new Dictionary<string, object>()),
                new AuditoriaMemento_380_jh(entidad, idEntidad, estado),
                accion);
        }

        public List<AuditoriaRegistro_380_jh> ListarTodos_380_jh()
        {
            List<AuditoriaRegistro_380_jh> registrosUsuario = new List<AuditoriaRegistro_380_jh>();

            foreach (AuditoriaRegistro_380_jh registro in _auditoriaRepository_380_jh.ListarTodos_380_jh())
            {
                if (registro.Entidad_380_jh == "Usuario")
                {
                    registrosUsuario.Add(registro);
                }
            }

            return registrosUsuario;
        }

        public List<AuditoriaRegistro_380_jh> ListarHistorial_380_jh(string entidad, int idEntidad)
        {
            return _auditoriaRepository_380_jh.ListarPorEntidad_380_jh(entidad, idEntidad);
        }

        private static List<AuditoriaCambio_380_jh> CalcularCambios_380_jh(AuditoriaMemento_380_jh estadoAnterior, AuditoriaMemento_380_jh estadoNuevo)
        {
            List<AuditoriaCambio_380_jh> cambios = new List<AuditoriaCambio_380_jh>();
            SortedSet<string> campos = new SortedSet<string>(StringComparer.Ordinal);

            foreach (string campo in estadoAnterior.Estado_380_jh.Keys)
            {
                campos.Add(campo);
            }

            foreach (string campo in estadoNuevo.Estado_380_jh.Keys)
            {
                campos.Add(campo);
            }

            foreach (string campo in campos)
            {
                object valorAnterior = estadoAnterior.Estado_380_jh.ContainsKey(campo) ? estadoAnterior.Estado_380_jh[campo] : null;
                object valorNuevo = estadoNuevo.Estado_380_jh.ContainsKey(campo) ? estadoNuevo.Estado_380_jh[campo] : null;

                if (object.Equals(valorAnterior, valorNuevo))
                {
                    continue;
                }

                cambios.Add(new AuditoriaCambio_380_jh
                {
                    Campo_380_jh = campo,
                    ValorAnterior_380_jh = valorAnterior,
                    ValorNuevo_380_jh = valorNuevo
                });
            }

            return cambios;
        }
    }
}
