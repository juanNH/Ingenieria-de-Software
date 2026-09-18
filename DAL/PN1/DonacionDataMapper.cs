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
