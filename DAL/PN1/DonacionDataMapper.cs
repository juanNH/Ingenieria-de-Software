using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Domain;

namespace DAL
{
    public class DonacionDataMapper_380_jh
    {
        private readonly DatabaseContext_380_jh _databaseContext_380_jh;

        public DonacionDataMapper_380_jh()
            : this(new DatabaseContext_380_jh())
        {
        }

        public DonacionDataMapper_380_jh(DatabaseContext_380_jh databaseContext)
        {
            _databaseContext_380_jh = databaseContext;
        }

        public CodigoOperacionPN1_380_jh Registrar_380_jh(Donacion_380_jh donacion, int idUsuario)
        {
            if (donacion == null)
            {
                return CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh;
            }

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_donante", donacion.IdDonante_380_jh),
                CrearParametroFechaHora_380_jh("@fecha_donacion", donacion.FechaDonacion_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@cantidad_unidades", donacion.CantidadUnidades_380_jh),
                CrearParametroTexto_380_jh("@tipo_componente", donacion.TipoComponente_380_jh, 100),
                CrearParametroFecha_380_jh("@fecha_vencimiento", donacion.FechaVencimiento_380_jh),
                CrearParametroTexto_380_jh("@observaciones", donacion.Observaciones_380_jh, 500),
                _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", idUsuario)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataSet conjunto = _databaseContext_380_jh.LeerConjunto_380_jh("sp_Donacion_Registrar", parametros);
                if (conjunto.Tables.Count == 0 || conjunto.Tables[0].Rows.Count == 0)
                {
                    return CodigoOperacionPN1_380_jh.Error_380_jh;
                }

                DataRow filaDonacion = conjunto.Tables[0].Rows[0];
                CodigoOperacionPN1_380_jh resultado = MapearResultado_380_jh(filaDonacion);
                if (resultado != CodigoOperacionPN1_380_jh.Ok_380_jh)
                {
                    return resultado;
                }

                donacion.Id_380_jh = Convert.ToInt32(filaDonacion["id_donacion"]);
                donacion.FechaAlta_380_jh = Convert.ToDateTime(filaDonacion["fecha_alta"]);
                donacion.Unidades_380_jh.Clear();

                if (conjunto.Tables.Count > 1)
                {
                    foreach (DataRow filaUnidad in conjunto.Tables[1].Rows)
                    {
                        donacion.Unidades_380_jh.Add(UnidadDataMapper_380_jh.MapearPublico_380_jh(filaUnidad));
                    }
                }

                return resultado;
            }
            catch (SqlException ex)
            {
                return ex.Number == 51001 || ex.Number == 51002 || ex.Number == 51014
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

        public CantidadDonacion_380_jh ConsultarCantidad_380_jh(int idDonacion, int idUsuario)
        {
            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataSet conjunto = _databaseContext_380_jh.LeerConjunto_380_jh("sp_Donacion_ConsultarCantidad", new List<SqlParameter>
                {
                    _databaseContext_380_jh.CrearParametro_380_jh("@id_donacion", idDonacion),
                    _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", idUsuario)
                });
                if (conjunto.Tables[0].Rows.Count == 0) return null;
                DataRow fila = conjunto.Tables[0].Rows[0];
                var resultado = new CantidadDonacion_380_jh
                {
                    Cantidad_380_jh = (int)fila["cantidad_unidades"],
                    Version_380_jh = fila.IsNull("version_actual") ? 0 : (int)fila["version_actual"],
                    Editable_380_jh = (bool)fila["editable"]
                };
                foreach (DataRow cambio in conjunto.Tables[1].Rows)
                    resultado.Historial_380_jh.Add(new CambioCantidadDonacion_380_jh
                    {
                        Version_380_jh = (int)cambio["id_auditoria"], Accion_380_jh = cambio["accion"].ToString(),
                        Fecha_380_jh = (DateTime)cambio["fecha_evento"], Responsable_380_jh = cambio["identificador_usuario_actor"].ToString(),
                        Anterior_380_jh = cambio.IsNull("cantidad_anterior") ? (int?)null : (int)cambio["cantidad_anterior"],
                        Nueva_380_jh = (int)cambio["cantidad_nueva"], Motivo_380_jh = cambio["motivo"].ToString(),
                        Codigos_380_jh = cambio["unidades_afectadas"].ToString()
                    });
                foreach (DataRow unidad in conjunto.Tables[2].Rows)
                    resultado.Anulables_380_jh.Add(new Unidad_380_jh
                    {
                        Id_380_jh = (int)unidad["id_unidad"], CodigoIdentificacion_380_jh = unidad["codigo_identificacion"].ToString()
                    });
                return resultado;
            }
            finally { _databaseContext_380_jh.Cerrar_380_jh(); }
        }

        public CodigoOperacionPN1_380_jh AjustarCantidad_380_jh(int idDonacion, int version, int cantidad, string motivo,
            IEnumerable<int> unidades, int? restaurar, int idUsuario)
        {
            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                _databaseContext_380_jh.LeerConjunto_380_jh("sp_Donacion_AjustarCantidad", new List<SqlParameter>
                {
                    _databaseContext_380_jh.CrearParametro_380_jh("@id_donacion", idDonacion),
                    _databaseContext_380_jh.CrearParametro_380_jh("@version_esperada", version),
                    _databaseContext_380_jh.CrearParametro_380_jh("@cantidad", cantidad),
                    new SqlParameter("@motivo", SqlDbType.NVarChar, 500) { Value = motivo },
                    new SqlParameter("@unidades_json", SqlDbType.NVarChar, -1) { Value = "[" + string.Join(",", unidades) + "]" },
                    _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", idUsuario),
                    new SqlParameter("@version_a_restaurar", SqlDbType.Int) { Value = (object)restaurar ?? DBNull.Value }
                });
                return CodigoOperacionPN1_380_jh.Ok_380_jh;
            }
            catch (SqlException ex)
            {
                switch (ex.Number)
                {
                    case 51001: case 51002: case 51014: return CodigoOperacionPN1_380_jh.IntegridadInvalida_380_jh;
                    case 51003: return CodigoOperacionPN1_380_jh.NoAutorizado_380_jh;
                    case 51120: return CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh;
                    case 51121: return CodigoOperacionPN1_380_jh.NoEncontrado_380_jh;
                    case 51122: return CodigoOperacionPN1_380_jh.ConflictoVersion_380_jh;
                    case 51123: return CodigoOperacionPN1_380_jh.DonacionCerrada_380_jh;
                    case 51124: return CodigoOperacionPN1_380_jh.UnidadesNoAnulables_380_jh;
                    default: return CodigoOperacionPN1_380_jh.Error_380_jh;
                }
            }
            finally { _databaseContext_380_jh.Cerrar_380_jh(); }
        }

        private static CodigoOperacionPN1_380_jh MapearResultado_380_jh(DataRow fila)
        {
            switch (fila["codigo_resultado"].ToString())
            {
                case "OK": return CodigoOperacionPN1_380_jh.Ok_380_jh;
                case "DATOS_INVALIDOS": return CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh;
                default: return CodigoOperacionPN1_380_jh.Error_380_jh;
            }
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

        private static SqlParameter CrearParametroFechaHora_380_jh(string nombre, DateTime valor)
        {
            return new SqlParameter(nombre, SqlDbType.DateTime) { Value = valor };
        }
    }
}
