using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Domain;

namespace DAL
{
    public class BitacoraDataMapper_380_jh
    {
        private readonly DatabaseContext_380_jh _databaseContext_380_jh;

        public BitacoraDataMapper_380_jh()
            : this(new DatabaseContext_380_jh())
        {
        }

        public BitacoraDataMapper_380_jh(DatabaseContext_380_jh databaseContext)
        {
            _databaseContext_380_jh = databaseContext;
        }

        public int Insertar_380_jh(BitacoraRegistro_380_jh bitacora)
        {
            if (bitacora == null)
            {
                return -1;
            }

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                CrearParametro_380_jh("@id_usuario", bitacora.IdUsuario_380_jh),
                CrearParametro_380_jh("@identificador_usuario", bitacora.IdentificadorUsuario_380_jh),
                CrearParametro_380_jh("@modulo", bitacora.Modulo_380_jh),
                CrearParametro_380_jh("@accion", bitacora.Accion_380_jh),
                CrearParametro_380_jh("@nivel", bitacora.Nivel_380_jh),
                CrearParametro_380_jh("@descripcion", bitacora.Descripcion_380_jh),
                CrearParametro_380_jh("@equipo", bitacora.Equipo_380_jh),
                CrearParametro_380_jh("@fecha_evento", bitacora.Fecha_380_jh)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.Leer_380_jh("sp_Bitacora_Registrar", parametros);

                if (tabla.Rows.Count == 0)
                {
                    return -1;
                }

                bitacora.Id_380_jh = Convert.ToInt32(tabla.Rows[0]["id_bitacora"]);
                return bitacora.Id_380_jh;
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

        public List<BitacoraRegistro_380_jh> Listar_380_jh()
        {
            return Listar_380_jh(null);
        }

        public List<BitacoraRegistro_380_jh> Listar_380_jh(BitacoraFiltro_380_jh filtro)
        {
            filtro = filtro ?? new BitacoraFiltro_380_jh();

            const string sql = @"
                SELECT
                    id_bitacora,
                    id_usuario,
                    identificador_usuario,
                    modulo,
                    accion,
                    nivel,
                    descripcion,
                    equipo,
                    fecha_evento
                FROM dbo.Bitacora
                WHERE (@fecha_desde IS NULL OR fecha_evento >= @fecha_desde)
                  AND (@fecha_hasta IS NULL OR fecha_evento < DATEADD(day, 1, @fecha_hasta))
                  AND (@usuario IS NULL OR identificador_usuario LIKE '%' + @usuario + '%')
                  AND (@modulo IS NULL OR modulo IN (@modulo, REPLACE(@modulo, '_380_jh', '')))
                  AND (@accion IS NULL OR accion IN (@accion, REPLACE(@accion, '_380_jh', '')))
                  AND (@nivel IS NULL OR nivel IN (@nivel, REPLACE(@nivel, '_380_jh', '')))
                  AND (@descripcion IS NULL OR descripcion LIKE '%' + @descripcion + '%')
                ORDER BY fecha_evento DESC, id_bitacora DESC";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                CrearParametro_380_jh("@fecha_desde", filtro.FechaDesde_380_jh),
                CrearParametro_380_jh("@fecha_hasta", filtro.FechaHasta_380_jh),
                CrearParametro_380_jh("@usuario", filtro.Usuario_380_jh),
                CrearParametro_380_jh("@modulo", filtro.Modulo_380_jh),
                CrearParametro_380_jh("@accion", filtro.Accion_380_jh),
                CrearParametro_380_jh("@nivel", filtro.Nivel_380_jh),
                CrearParametro_380_jh("@descripcion", filtro.Descripcion_380_jh)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);
                List<BitacoraRegistro_380_jh> registros = new List<BitacoraRegistro_380_jh>();

                foreach (DataRow fila in tabla.Rows)
                {
                    registros.Add(MapearBitacora_380_jh(fila));
                }

                return registros;
            }
            catch
            {
                return new List<BitacoraRegistro_380_jh>();
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        #region CreacParametros
        private static SqlParameter CrearParametro_380_jh(string nombre, string valor)
        {
            return new SqlParameter
            {
                ParameterName = nombre,
                Value = string.IsNullOrWhiteSpace(valor) ? (object)DBNull.Value : valor,
                DbType = DbType.String
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

        private static SqlParameter CrearParametro_380_jh(string nombre, DateTime? valor)
        {
            return new SqlParameter
            {
                ParameterName = nombre,
                Value = valor.HasValue ? (object)valor.Value : DBNull.Value,
                DbType = DbType.DateTime
            };
        }
        #endregion
        private static BitacoraRegistro_380_jh MapearBitacora_380_jh(DataRow fila)
        {
            return new BitacoraRegistro_380_jh
            {
                Id_380_jh = Convert.ToInt32(fila["id_bitacora"]),
                IdUsuario_380_jh = fila["id_usuario"] == DBNull.Value ? (int?)null : Convert.ToInt32(fila["id_usuario"]),
                IdentificadorUsuario_380_jh = fila["identificador_usuario"] == DBNull.Value ? null : fila["identificador_usuario"].ToString(),
                Modulo_380_jh = fila["modulo"].ToString(),
                Accion_380_jh = fila["accion"].ToString(),
                Nivel_380_jh = fila["nivel"].ToString(),
                Descripcion_380_jh = fila["descripcion"] == DBNull.Value ? null : fila["descripcion"].ToString(),
                Equipo_380_jh = fila["equipo"] == DBNull.Value ? null : fila["equipo"].ToString(),
                Fecha_380_jh = Convert.ToDateTime(fila["fecha_evento"])
            };
        }
    }
}
