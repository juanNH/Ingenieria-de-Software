using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Domain;

namespace DAL
{
    public class UnidadDataMapper_380_jh
    {
        private readonly DatabaseContext_380_jh _databaseContext_380_jh;

        public UnidadDataMapper_380_jh()
            : this(new DatabaseContext_380_jh())
        {
        }

        public UnidadDataMapper_380_jh(DatabaseContext_380_jh databaseContext)
        {
            _databaseContext_380_jh = databaseContext;
        }

        public List<Unidad_380_jh> Listar_380_jh(string buscar)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                CrearParametroTexto_380_jh("@buscar", buscar, 150)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.Leer_380_jh("sp_Unidad_Listar", parametros);
                List<Unidad_380_jh> unidades = new List<Unidad_380_jh>();

                foreach (DataRow fila in tabla.Rows)
                {
                    unidades.Add(MapearPublico_380_jh(fila));
                }

                return unidades;
            }
            catch
            {
                return new List<Unidad_380_jh>();
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public CodigoOperacionPN1_380_jh Clasificar_380_jh(Unidad_380_jh unidad, int idUsuario)
        {
            if (unidad == null)
            {
                return CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh;
            }

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_unidad", unidad.Id_380_jh),
                CrearParametroTexto_380_jh("@tipo_componente", unidad.TipoComponente_380_jh, 100),
                CrearParametroTexto_380_jh("@grupo_sanguineo", unidad.GrupoSanguineo_380_jh, 5),
                CrearParametroTexto_380_jh("@factor_rh", unidad.FactorRh_380_jh, 5),
                CrearParametroFecha_380_jh("@fecha_vencimiento", unidad.FechaVencimiento_380_jh),
                CrearParametroTexto_380_jh("@observaciones", unidad.Observaciones_380_jh, 500),
                _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", idUsuario)
            };

            return EjecutarActualizacion_380_jh("sp_Unidad_Clasificar", parametros, unidad);
        }

        public CodigoOperacionPN1_380_jh CambiarEstado_380_jh(Unidad_380_jh unidad, string estado, string observacion, int idUsuario)
        {
            if (unidad == null)
            {
                return CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh;
            }

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_unidad", unidad.Id_380_jh),
                CrearParametroTexto_380_jh("@estado_nuevo", estado, 20),
                CrearParametroTexto_380_jh("@observacion", observacion, 500),
                _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", idUsuario)
            };

            return EjecutarActualizacion_380_jh("sp_Unidad_CambiarEstado", parametros, unidad);
        }

        private CodigoOperacionPN1_380_jh EjecutarActualizacion_380_jh(string procedimiento, List<SqlParameter> parametros, Unidad_380_jh unidad)
        {
            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.Leer_380_jh(procedimiento, parametros);
                if (tabla.Rows.Count == 0)
                {
                    return CodigoOperacionPN1_380_jh.Error_380_jh;
                }

                CodigoOperacionPN1_380_jh resultado = MapearResultado_380_jh(tabla.Rows[0]);
                if (resultado == CodigoOperacionPN1_380_jh.Ok_380_jh)
                {
                    Copiar_380_jh(unidad, MapearPublico_380_jh(tabla.Rows[0]));
                }

                return resultado;
            }
            catch (SqlException ex)
            {
                return ex.Number == 51001 || ex.Number == 51002
                    ? CodigoOperacionPN1_380_jh.IntegridadInvalida_380_jh : CodigoOperacionPN1_380_jh.Error_380_jh;
            }
            catch
            {
                return CodigoOperacionPN1_380_jh.Error_380_jh;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        internal static Unidad_380_jh MapearPublico_380_jh(DataRow fila)
        {
            return new Unidad_380_jh
            {
                Id_380_jh = Convert.ToInt32(fila["id_unidad"]),
                CodigoIdentificacion_380_jh = Texto_380_jh(fila, "codigo_identificacion"),
                IdDonacion_380_jh = Convert.ToInt32(fila["id_donacion"]),
                IdDonante_380_jh = EnteroNullable_380_jh(fila, "id_donante"),
                DocumentoDonante_380_jh = Texto_380_jh(fila, "documento_donante"),
                Donante_380_jh = Texto_380_jh(fila, "donante"),
                TipoComponente_380_jh = Texto_380_jh(fila, "tipo_componente"),
                FechaVencimiento_380_jh = Convert.ToDateTime(fila["fecha_vencimiento"]),
                GrupoSanguineo_380_jh = Texto_380_jh(fila, "grupo_sanguineo"),
                FactorRh_380_jh = Texto_380_jh(fila, "factor_rh"),
                Observaciones_380_jh = Texto_380_jh(fila, "observaciones"),
                EstadoOperativo_380_jh = Texto_380_jh(fila, "estado_operativo"),
                FechaAlta_380_jh = Convert.ToDateTime(fila["fecha_alta"]),
                FechaUltimaModificacion_380_jh = Convert.ToDateTime(fila["fecha_ultima_modificacion"]),
                IdUsuarioUltimaModificacion_380_jh = Convert.ToInt32(fila["id_usuario_ultima_modificacion"])
            };
        }

        private static void Copiar_380_jh(Unidad_380_jh destino, Unidad_380_jh origen)
        {
            destino.CodigoIdentificacion_380_jh = origen.CodigoIdentificacion_380_jh;
            destino.IdDonacion_380_jh = origen.IdDonacion_380_jh;
            destino.IdDonante_380_jh = origen.IdDonante_380_jh;
            destino.DocumentoDonante_380_jh = origen.DocumentoDonante_380_jh;
            destino.Donante_380_jh = origen.Donante_380_jh;
            destino.TipoComponente_380_jh = origen.TipoComponente_380_jh;
            destino.FechaVencimiento_380_jh = origen.FechaVencimiento_380_jh;
            destino.GrupoSanguineo_380_jh = origen.GrupoSanguineo_380_jh;
            destino.FactorRh_380_jh = origen.FactorRh_380_jh;
            destino.Observaciones_380_jh = origen.Observaciones_380_jh;
            destino.EstadoOperativo_380_jh = origen.EstadoOperativo_380_jh;
            destino.FechaAlta_380_jh = origen.FechaAlta_380_jh;
            destino.FechaUltimaModificacion_380_jh = origen.FechaUltimaModificacion_380_jh;
            destino.IdUsuarioUltimaModificacion_380_jh = origen.IdUsuarioUltimaModificacion_380_jh;
        }

        private static CodigoOperacionPN1_380_jh MapearResultado_380_jh(DataRow fila)
        {
            switch (fila["codigo_resultado"].ToString())
            {
                case "OK": return CodigoOperacionPN1_380_jh.Ok_380_jh;
                case "DATOS_INVALIDOS": return CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh;
                case "NO_ENCONTRADO": return CodigoOperacionPN1_380_jh.NoEncontrado_380_jh;
                case "CONFLICTO_ESTADO": return CodigoOperacionPN1_380_jh.ConflictoEstado_380_jh;
                default: return CodigoOperacionPN1_380_jh.Error_380_jh;
            }
        }

        private static string Texto_380_jh(DataRow fila, string columna)
        {
            return fila.Table.Columns.Contains(columna) && fila[columna] != DBNull.Value
                ? fila[columna].ToString()
                : null;
        }

        private static int? EnteroNullable_380_jh(DataRow fila, string columna)
        {
            return fila.Table.Columns.Contains(columna) && fila[columna] != DBNull.Value
                ? (int?)Convert.ToInt32(fila[columna])
                : null;
        }

        private static SqlParameter CrearParametroTexto_380_jh(string nombre, string valor, int tamano)
        {
            return new SqlParameter(nombre, SqlDbType.VarChar, tamano)
            {
                Value = string.IsNullOrWhiteSpace(valor) ? (object)DBNull.Value : valor.Trim()
            };
        }

        private static SqlParameter CrearParametroFecha_380_jh(string nombre, DateTime valor)
        {
            return new SqlParameter(nombre, SqlDbType.Date) { Value = valor.Date };
        }
    }
}
