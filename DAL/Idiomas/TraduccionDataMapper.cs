using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Domain;

namespace DAL
{
    public class TraduccionDataMapper_380_jh
    {
        private readonly DatabaseContext_380_jh _databaseContext_380_jh;

        public TraduccionDataMapper_380_jh()
            : this(new DatabaseContext_380_jh())
        {
        }

        public TraduccionDataMapper_380_jh(DatabaseContext_380_jh databaseContext)
        {
            _databaseContext_380_jh = databaseContext;
        }

        public int CrearEtiqueta_380_jh(Etiqueta_380_jh etiqueta)
        {
            if (etiqueta == null)
            {
                return -1;
            }

            const string sql = @"
                INSERT INTO dbo.Etiqueta (clave, descripcion)
                OUTPUT INSERTED.id_etiqueta
                VALUES (@clave, @descripcion)";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@clave", etiqueta.Key_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@descripcion", etiqueta.Descripcion_380_jh)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);
                if (tabla.Rows.Count == 0)
                {
                    return -1;
                }

                etiqueta.Id_380_jh = Convert.ToInt32(tabla.Rows[0]["id_etiqueta"]);
                return etiqueta.Id_380_jh;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public bool GuardarTraduccion_380_jh(Traduccion_380_jh traduccion)
        {
            if (traduccion == null || traduccion.EtiquetaId_380_jh == 0 || traduccion.IdiomaId_380_jh == 0)
            {
                return false;
            }

            const string sql = @"
                MERGE dbo.Traduccion AS target
                USING (SELECT @id_etiqueta AS id_etiqueta, @id_idioma AS id_idioma) AS source
                    ON target.id_etiqueta = source.id_etiqueta
                   AND target.id_idioma = source.id_idioma
                WHEN MATCHED THEN
                    UPDATE SET texto = @texto
                WHEN NOT MATCHED THEN
                    INSERT (id_etiqueta, id_idioma, texto)
                    VALUES (@id_etiqueta, @id_idioma, @texto);";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_etiqueta", traduccion.EtiquetaId_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@id_idioma", traduccion.IdiomaId_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@texto", traduccion.Texto_380_jh)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                return _databaseContext_380_jh.EscribirTexto_380_jh(sql, parametros) > 0;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public string ObtenerTexto_380_jh(string key, int idiomaId)
        {
            const string sql = @"
                SELECT TOP (1) t.texto
                FROM dbo.Traduccion t
                INNER JOIN dbo.Etiqueta e ON e.id_etiqueta = t.id_etiqueta
                WHERE e.clave = @clave
                  AND t.id_idioma = @id_idioma";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@clave", key),
                _databaseContext_380_jh.CrearParametro_380_jh("@id_idioma", idiomaId)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);
                return tabla.Rows.Count == 0 ? null : tabla.Rows[0]["texto"].ToString();
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public Traduccion_380_jh ObtenerTraduccion_380_jh(int etiquetaId, int idiomaId)
        {
            const string sql = @"
                SELECT TOP (1)
                    t.id_traduccion,
                    t.id_etiqueta,
                    t.id_idioma,
                    t.texto,
                    e.clave,
                    i.codigo
                FROM dbo.Traduccion t
                INNER JOIN dbo.Etiqueta e ON e.id_etiqueta = t.id_etiqueta
                INNER JOIN dbo.Idioma i ON i.id_idioma = t.id_idioma
                WHERE t.id_etiqueta = @id_etiqueta
                  AND t.id_idioma = @id_idioma";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_etiqueta", etiquetaId),
                _databaseContext_380_jh.CrearParametro_380_jh("@id_idioma", idiomaId)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);
                return tabla.Rows.Count == 0 ? null : MapearTraduccion_380_jh(tabla.Rows[0]);
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public Dictionary<string, string> ListarPorIdioma_380_jh(int idiomaId)
        {
            const string sql = @"
                SELECT e.clave, t.texto
                FROM dbo.Traduccion t
                INNER JOIN dbo.Etiqueta e ON e.id_etiqueta = t.id_etiqueta
                WHERE t.id_idioma = @id_idioma";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_idioma", idiomaId)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);
                Dictionary<string, string> traducciones = new Dictionary<string, string>();

                foreach (DataRow registro in tabla.Rows)
                {
                    traducciones[registro["clave"].ToString()] = registro["texto"].ToString();
                }

                return traducciones;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public List<Etiqueta_380_jh> ListarEtiquetas_380_jh()
        {
            const string sql = @"
                SELECT id_etiqueta, clave, descripcion
                FROM dbo.Etiqueta
                ORDER BY clave";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql);
                List<Etiqueta_380_jh> etiquetas = new List<Etiqueta_380_jh>();

                foreach (DataRow registro in tabla.Rows)
                {
                    etiquetas.Add(new Etiqueta_380_jh
                    {
                        Id_380_jh = Convert.ToInt32(registro["id_etiqueta"]),
                        Key_380_jh = registro["clave"].ToString(),
                        Descripcion_380_jh = registro["descripcion"] == DBNull.Value ? null : registro["descripcion"].ToString()
                    });
                }

                return etiquetas;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public List<Traduccion_380_jh> ListarTraducciones_380_jh()
        {
            const string sql = @"
                SELECT
                    t.id_traduccion,
                    t.id_etiqueta,
                    t.id_idioma,
                    t.texto,
                    e.clave,
                    i.codigo
                FROM dbo.Traduccion t
                INNER JOIN dbo.Etiqueta e ON e.id_etiqueta = t.id_etiqueta
                INNER JOIN dbo.Idioma i ON i.id_idioma = t.id_idioma
                ORDER BY e.clave, i.codigo";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql);
                List<Traduccion_380_jh> traducciones = new List<Traduccion_380_jh>();

                foreach (DataRow registro in tabla.Rows)
                {
                    traducciones.Add(MapearTraduccion_380_jh(registro));
                }

                return traducciones;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        private static Traduccion_380_jh MapearTraduccion_380_jh(DataRow registro)
        {
            return new Traduccion_380_jh
            {
                Id_380_jh = Convert.ToInt32(registro["id_traduccion"]),
                EtiquetaId_380_jh = Convert.ToInt32(registro["id_etiqueta"]),
                IdiomaId_380_jh = Convert.ToInt32(registro["id_idioma"]),
                Texto_380_jh = registro["texto"].ToString(),
                EtiquetaKey_380_jh = registro["clave"].ToString(),
                IdiomaCodigo_380_jh = registro["codigo"].ToString()
            };
        }
    }
}
