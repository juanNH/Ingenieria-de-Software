using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Domain;

namespace DAL
{
    public class IdiomaDataMapper_380_jh
    {
        private readonly DatabaseContext_380_jh _databaseContext_380_jh;

        public IdiomaDataMapper_380_jh()
            : this(new DatabaseContext_380_jh())
        {
        }

        public IdiomaDataMapper_380_jh(DatabaseContext_380_jh databaseContext)
        {
            _databaseContext_380_jh = databaseContext;
        }

        public int Crear_380_jh(Idioma_380_jh idioma, int? idUsuarioResponsable)
        {
            if (idioma == null)
            {
                return -1;
            }

            const string sql = @"
                INSERT INTO dbo.Idioma (codigo, nombre, estado_idioma)
                OUTPUT INSERTED.id_idioma
                VALUES (@codigo, @nombre, @estado_idioma)";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@codigo", idioma.Codigo_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@nombre", idioma.Nombre_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@estado_idioma", EstadoDesdeActivo_380_jh(idioma.Activo_380_jh))
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);
                if (tabla.Rows.Count == 0)
                {
                    return -1;
                }

                idioma.Id_380_jh = Convert.ToInt32(tabla.Rows[0]["id_idioma"]);
                RegistrarHistorialEnConexion_380_jh(idioma.Id_380_jh, null, EstadoDesdeActivo_380_jh(idioma.Activo_380_jh), "Alta de idioma", idUsuarioResponsable);
                return idioma.Id_380_jh;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public bool Actualizar_380_jh(Idioma_380_jh idioma, int? idUsuarioResponsable, string motivo)
        {
            if (idioma == null || idioma.Id_380_jh == 0)
            {
                return false;
            }

            Idioma_380_jh idiomaActual = ObtenerPorId_380_jh(idioma.Id_380_jh);
            string estadoAnterior = idiomaActual == null ? null : EstadoDesdeActivo_380_jh(idiomaActual.Activo_380_jh);
            string estadoNuevo = EstadoDesdeActivo_380_jh(idioma.Activo_380_jh);

            const string sql = @"
                UPDATE dbo.Idioma
                SET codigo = @codigo,
                    nombre = @nombre,
                    estado_idioma = @estado_idioma
                WHERE id_idioma = @id_idioma";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_idioma", idioma.Id_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@codigo", idioma.Codigo_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@nombre", idioma.Nombre_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@estado_idioma", estadoNuevo)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                int afectados = _databaseContext_380_jh.EscribirTexto_380_jh(sql, parametros);
                if (afectados > 0 && estadoAnterior != estadoNuevo)
                {
                    RegistrarHistorialEnConexion_380_jh(idioma.Id_380_jh, estadoAnterior, estadoNuevo, motivo, idUsuarioResponsable);
                }

                return afectados > 0;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public Idioma_380_jh ObtenerPorId_380_jh(int id)
        {
            const string sql = @"
                SELECT id_idioma, codigo, nombre, estado_idioma
                FROM dbo.Idioma
                WHERE id_idioma = @id_idioma";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_idioma", id)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);
                return tabla.Rows.Count == 0 ? null : Mapear_380_jh(tabla.Rows[0]);
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public Idioma_380_jh ObtenerDefault_380_jh()
        {
            const string sql = @"
                SELECT TOP (1) id_idioma, codigo, nombre, estado_idioma
                FROM dbo.Idioma
                WHERE estado_idioma IN ('Activo', 'ACTIVO')
                ORDER BY CASE WHEN codigo IN ('es-AR', 'es') THEN 0 ELSE 1 END, id_idioma";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql);
                return tabla.Rows.Count == 0 ? null : Mapear_380_jh(tabla.Rows[0]);
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public List<Idioma_380_jh> Listar_380_jh(bool soloActivos)
        {
            string filtro = soloActivos ? "WHERE estado_idioma IN ('Activo', 'ACTIVO')" : string.Empty;
            string sql = @"
                SELECT id_idioma, codigo, nombre, estado_idioma
                FROM dbo.Idioma
                " + filtro + @"
                ORDER BY nombre";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql);
                List<Idioma_380_jh> idiomas = new List<Idioma_380_jh>();

                foreach (DataRow registro in tabla.Rows)
                {
                    idiomas.Add(Mapear_380_jh(registro));
                }

                return idiomas;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        private void RegistrarHistorialEnConexion_380_jh(int idiomaId, string estadoAnterior, string estadoNuevo, string motivo, int? idUsuarioResponsable)
        {
            const string sql = @"
                IF OBJECT_ID('dbo.IdiomaEstadoHistorial', 'U') IS NOT NULL
                BEGIN
                    INSERT INTO dbo.IdiomaEstadoHistorial
                        (id_idioma, estado_anterior, estado_nuevo, motivo, id_usuario_responsable)
                    VALUES
                        (@id_idioma, @estado_anterior, @estado_nuevo, @motivo, @id_usuario_responsable)
                END";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_idioma", idiomaId),
                _databaseContext_380_jh.CrearParametro_380_jh("@estado_anterior", estadoAnterior),
                _databaseContext_380_jh.CrearParametro_380_jh("@estado_nuevo", estadoNuevo),
                _databaseContext_380_jh.CrearParametro_380_jh("@motivo", motivo),
                new SqlParameter("@id_usuario_responsable", SqlDbType.Int)
                {
                    Value = idUsuarioResponsable.HasValue ? (object)idUsuarioResponsable.Value : DBNull.Value
                }
            };

            _databaseContext_380_jh.EscribirTexto_380_jh(sql, parametros);
        }

        private static Idioma_380_jh Mapear_380_jh(DataRow registro)
        {
            return new Idioma_380_jh
            {
                Id_380_jh = Convert.ToInt32(registro["id_idioma"]),
                Codigo_380_jh = registro["codigo"].ToString(),
                Nombre_380_jh = registro["nombre"].ToString(),
                Activo_380_jh = EsActivo_380_jh(registro["estado_idioma"].ToString())
            };
        }

        private static string EstadoDesdeActivo_380_jh(bool activo)
        {
            return activo ? "Activo" : "Inactivo";
        }

        private static bool EsActivo_380_jh(string estado)
        {
            return string.Equals(estado, "Activo", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(estado, "ACTIVO", StringComparison.OrdinalIgnoreCase);
        }
    }
}
