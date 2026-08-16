using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Domain;

namespace DAL
{
    public class AuditoriaDataMapper_380_jh
    {
        private readonly DatabaseContext_380_jh _databaseContext_380_jh;

        public AuditoriaDataMapper_380_jh()
            : this(new DatabaseContext_380_jh())
        {
        }

        public AuditoriaDataMapper_380_jh(DatabaseContext_380_jh databaseContext)
        {
            _databaseContext_380_jh = databaseContext;
        }

        public int Insertar_380_jh(AuditoriaRegistro_380_jh auditoria)
        {
            if (auditoria == null)
            {
                return -1;
            }

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                CrearParametro_380_jh("@entidad", auditoria.Entidad_380_jh),
                CrearParametro_380_jh("@id_entidad", auditoria.IdEntidad_380_jh),
                CrearParametro_380_jh("@accion", auditoria.Accion_380_jh),
                CrearParametro_380_jh("@id_usuario_actor", auditoria.IdUsuarioActor_380_jh),
                CrearParametro_380_jh("@identificador_usuario_actor", auditoria.IdentificadorUsuarioActor_380_jh),
                CrearParametro_380_jh("@fecha_evento", auditoria.FechaEvento_380_jh),
                CrearParametro_380_jh("@estado_anterior_json", auditoria.EstadoAnteriorJson_380_jh),
                CrearParametro_380_jh("@estado_nuevo_json", auditoria.EstadoNuevoJson_380_jh),
                CrearParametro_380_jh("@cambios_json", auditoria.CambiosJson_380_jh)
            };

            const string sql = @"
                INSERT INTO dbo.Auditoria
                (
                    entidad,
                    id_entidad,
                    accion,
                    id_usuario_actor,
                    identificador_usuario_actor,
                    fecha_evento,
                    estado_anterior_json,
                    estado_nuevo_json,
                    cambios_json
                )
                OUTPUT INSERTED.id_auditoria
                VALUES
                (
                    @entidad,
                    @id_entidad,
                    @accion,
                    @id_usuario_actor,
                    @identificador_usuario_actor,
                    @fecha_evento,
                    @estado_anterior_json,
                    @estado_nuevo_json,
                    @cambios_json
                )";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);

                if (tabla.Rows.Count == 0)
                {
                    return -1;
                }

                auditoria.Id_380_jh = Convert.ToInt32(tabla.Rows[0]["id_auditoria"]);
                return auditoria.Id_380_jh;
            }
            catch
            {
                return -1;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public List<AuditoriaRegistro_380_jh> ListarPorEntidad_380_jh(string entidad, int idEntidad)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                CrearParametro_380_jh("@entidad", entidad),
                CrearParametro_380_jh("@id_entidad", idEntidad)
            };

            const string sql = @"
                SELECT
                    id_auditoria,
                    entidad,
                    id_entidad,
                    accion,
                    id_usuario_actor,
                    identificador_usuario_actor,
                    fecha_evento,
                    estado_anterior_json,
                    estado_nuevo_json,
                    cambios_json
                FROM dbo.Auditoria
                WHERE entidad = @entidad
                  AND id_entidad = @id_entidad
                ORDER BY fecha_evento DESC, id_auditoria DESC";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);
                List<AuditoriaRegistro_380_jh> registros = new List<AuditoriaRegistro_380_jh>();

                foreach (DataRow fila in tabla.Rows)
                {
                    registros.Add(MapearAuditoria_380_jh(fila));
                }

                return registros;
            }
            catch
            {
                return new List<AuditoriaRegistro_380_jh>();
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public List<AuditoriaRegistro_380_jh> ListarTodos_380_jh()
        {
            const string sql = @"
                SELECT
                    id_auditoria,
                    entidad,
                    id_entidad,
                    accion,
                    id_usuario_actor,
                    identificador_usuario_actor,
                    fecha_evento,
                    estado_anterior_json,
                    estado_nuevo_json,
                    cambios_json
                FROM dbo.Auditoria
                ORDER BY fecha_evento DESC, id_auditoria DESC";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql);
                List<AuditoriaRegistro_380_jh> registros = new List<AuditoriaRegistro_380_jh>();

                foreach (DataRow fila in tabla.Rows)
                {
                    registros.Add(MapearAuditoria_380_jh(fila));
                }

                return registros;
            }
            catch
            {
                return new List<AuditoriaRegistro_380_jh>();
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        private static SqlParameter CrearParametro_380_jh(string nombre, string valor)
        {
            return new SqlParameter
            {
                ParameterName = nombre,
                Value = string.IsNullOrWhiteSpace(valor) ? (object)DBNull.Value : valor,
                DbType = DbType.String
            };
        }

        private static SqlParameter CrearParametro_380_jh(string nombre, int valor)
        {
            return new SqlParameter
            {
                ParameterName = nombre,
                Value = valor,
                DbType = DbType.Int32
            };
        }

        private static SqlParameter CrearParametro_380_jh(string nombre, int? valor)
        {
            return new SqlParameter
            {
                ParameterName = nombre,
                Value = valor.HasValue ? (object)valor.Value : DBNull.Value,
                DbType = DbType.Int32
            };
        }

        private static SqlParameter CrearParametro_380_jh(string nombre, DateTime valor)
        {
            return new SqlParameter
            {
                ParameterName = nombre,
                Value = valor,
                DbType = DbType.DateTime
            };
        }

        private static AuditoriaRegistro_380_jh MapearAuditoria_380_jh(DataRow fila)
        {
            return new AuditoriaRegistro_380_jh
            {
                Id_380_jh = Convert.ToInt32(fila["id_auditoria"]),
                Entidad_380_jh = fila["entidad"].ToString(),
                IdEntidad_380_jh = Convert.ToInt32(fila["id_entidad"]),
                Accion_380_jh = fila["accion"].ToString(),
                IdUsuarioActor_380_jh = fila["id_usuario_actor"] == DBNull.Value ? (int?)null : Convert.ToInt32(fila["id_usuario_actor"]),
                IdentificadorUsuarioActor_380_jh = fila["identificador_usuario_actor"] == DBNull.Value ? null : fila["identificador_usuario_actor"].ToString(),
                FechaEvento_380_jh = Convert.ToDateTime(fila["fecha_evento"]),
                EstadoAnteriorJson_380_jh = fila["estado_anterior_json"] == DBNull.Value ? null : fila["estado_anterior_json"].ToString(),
                EstadoNuevoJson_380_jh = fila["estado_nuevo_json"] == DBNull.Value ? null : fila["estado_nuevo_json"].ToString(),
                CambiosJson_380_jh = fila["cambios_json"] == DBNull.Value ? null : fila["cambios_json"].ToString()
            };
        }
    }
}
