using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Domain;

namespace DAL
{
    public class DonanteDataMapper_380_jh
    {
        private readonly DatabaseContext_380_jh _databaseContext_380_jh;

        public DonanteDataMapper_380_jh()
            : this(new DatabaseContext_380_jh())
        {
        }

        public DonanteDataMapper_380_jh(DatabaseContext_380_jh databaseContext)
        {
            _databaseContext_380_jh = databaseContext;
        }

        public List<Donante_380_jh> Listar_380_jh(string buscar)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                CrearParametroTexto_380_jh("@buscar", buscar)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.Leer_380_jh("sp_Donante_Listar", parametros);
                List<Donante_380_jh> donantes = new List<Donante_380_jh>();

                foreach (DataRow fila in tabla.Rows)
                {
                    donantes.Add(Mapear_380_jh(fila));
                }

                return donantes;
            }
            catch
            {
                return new List<Donante_380_jh>();
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public Dictionary<int, string> ListarEmailsLegados_380_jh()
        {
            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.Leer_380_jh("sp_Donante_Correos_Legados_Listar");
                Dictionary<int, string> emails = new Dictionary<int, string>();
                foreach (DataRow fila in tabla.Rows)
                {
                    if (fila["email"] != DBNull.Value)
                    {
                        emails[Convert.ToInt32(fila["id_donante"])] = fila["email"].ToString();
                    }
                }

                return emails;
            }
            catch
            {
                return new Dictionary<int, string>();
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public bool MigrarEmailsLegados_380_jh(string emailsJson, int idUsuario)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("@emails_json", SqlDbType.NVarChar, -1) { Value = emailsJson },
                _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", idUsuario)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.Leer_380_jh("sp_Donante_Correos_Migrar", parametros);
                return tabla.Rows.Count > 0 && Convert.ToBoolean(tabla.Rows[0]["completado"]);
            }
            catch
            {
                return false;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public string ObtenerEmailCifrado_380_jh(int idDonante)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_donante", idDonante)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.Leer_380_jh("sp_Donante_Email_Obtener", parametros);
                return tabla.Rows.Count == 0 ? null : Texto_380_jh(tabla.Rows[0], "email");
            }
            catch
            {
                return null;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public CodigoOperacionPN1_380_jh Registrar_380_jh(Donante_380_jh donante, int idUsuario)
        {
            if (donante == null)
            {
                return CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh;
            }

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                CrearParametroTexto_380_jh("@documento", donante.Documento_380_jh),
                CrearParametroTexto_380_jh("@nombre", donante.Nombre_380_jh),
                CrearParametroTexto_380_jh("@apellido", donante.Apellido_380_jh),
                CrearParametroFecha_380_jh("@fecha_nacimiento", donante.FechaNacimiento_380_jh),
                CrearParametroTexto_380_jh("@telefono", donante.Telefono_380_jh),
                CrearParametroEmail_380_jh("@email", donante.Email_380_jh),
                CrearParametroTexto_380_jh("@domicilio", donante.Domicilio_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", idUsuario)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.Leer_380_jh("sp_Donante_Registrar", parametros);
                if (tabla.Rows.Count == 0)
                {
                    return CodigoOperacionPN1_380_jh.Error_380_jh;
                }

                CodigoOperacionPN1_380_jh resultado = MapearResultado_380_jh(tabla.Rows[0]);
                if (resultado == CodigoOperacionPN1_380_jh.Ok_380_jh)
                {
                    donante.Id_380_jh = Convert.ToInt32(tabla.Rows[0]["id_donante"]);
                }

                return resultado;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 51001 || ex.Number == 51002) return CodigoOperacionPN1_380_jh.IntegridadInvalida_380_jh;
                return ex.Number == 2601 || ex.Number == 2627
                    ? CodigoOperacionPN1_380_jh.Duplicado_380_jh
                    : CodigoOperacionPN1_380_jh.Error_380_jh;
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

        public CodigoOperacionPN1_380_jh Actualizar_380_jh(Donante_380_jh donante, string emailCifrado, bool actualizarEmail, int idUsuario)
        {
            if (donante == null)
            {
                return CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh;
            }

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_donante", donante.Id_380_jh),
                CrearParametroTexto_380_jh("@documento", donante.Documento_380_jh),
                CrearParametroTexto_380_jh("@nombre", donante.Nombre_380_jh),
                CrearParametroTexto_380_jh("@apellido", donante.Apellido_380_jh),
                CrearParametroFecha_380_jh("@fecha_nacimiento", donante.FechaNacimiento_380_jh),
                CrearParametroTexto_380_jh("@telefono", donante.Telefono_380_jh),
                CrearParametroEmail_380_jh("@email", emailCifrado),
                new SqlParameter("@actualizar_email", SqlDbType.Bit) { Value = actualizarEmail },
                CrearParametroTexto_380_jh("@domicilio", donante.Domicilio_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", idUsuario)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.Leer_380_jh("sp_Donante_Actualizar", parametros);
                return tabla.Rows.Count == 0
                    ? CodigoOperacionPN1_380_jh.Error_380_jh
                    : MapearResultado_380_jh(tabla.Rows[0]);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 51001 || ex.Number == 51002) return CodigoOperacionPN1_380_jh.IntegridadInvalida_380_jh;
                return ex.Number == 2601 || ex.Number == 2627
                    ? CodigoOperacionPN1_380_jh.Duplicado_380_jh
                    : CodigoOperacionPN1_380_jh.Error_380_jh;
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
                case "DUPLICADO": return CodigoOperacionPN1_380_jh.Duplicado_380_jh;
                case "DATOS_INVALIDOS": return CodigoOperacionPN1_380_jh.DatosInvalidos_380_jh;
                case "NO_ENCONTRADO": return CodigoOperacionPN1_380_jh.NoEncontrado_380_jh;
                default: return CodigoOperacionPN1_380_jh.Error_380_jh;
            }
        }

        private static Donante_380_jh Mapear_380_jh(DataRow fila)
        {
            return new Donante_380_jh
            {
                Id_380_jh = Convert.ToInt32(fila["id_donante"]),
                Documento_380_jh = Texto_380_jh(fila, "documento"),
                Nombre_380_jh = Texto_380_jh(fila, "nombre"),
                Apellido_380_jh = Texto_380_jh(fila, "apellido"),
                FechaNacimiento_380_jh = Fecha_380_jh(fila, "fecha_nacimiento"),
                Telefono_380_jh = Texto_380_jh(fila, "telefono"),
                Email_380_jh = Texto_380_jh(fila, "email"),
                TieneEmail_380_jh = fila.Table.Columns.Contains("tiene_email") && fila["tiene_email"] != DBNull.Value && Convert.ToBoolean(fila["tiene_email"]),
                Domicilio_380_jh = Texto_380_jh(fila, "domicilio"),
                Estado_380_jh = Texto_380_jh(fila, "estado_donante"),
                FechaAlta_380_jh = Convert.ToDateTime(fila["fecha_alta"]),
                IdUsuarioAlta_380_jh = Convert.ToInt32(fila["id_usuario_alta"])
            };
        }

        private static string Texto_380_jh(DataRow fila, string columna)
        {
            return fila.Table.Columns.Contains(columna) && fila[columna] != DBNull.Value
                ? fila[columna].ToString()
                : null;
        }

        private static DateTime? Fecha_380_jh(DataRow fila, string columna)
        {
            return fila.Table.Columns.Contains(columna) && fila[columna] != DBNull.Value
                ? (DateTime?)Convert.ToDateTime(fila[columna])
                : null;
        }

        private static SqlParameter CrearParametroTexto_380_jh(string nombre, string valor)
        {
            return new SqlParameter(nombre, SqlDbType.VarChar, 255)
            {
                Value = string.IsNullOrWhiteSpace(valor) ? (object)DBNull.Value : valor.Trim()
            };
        }

        private static SqlParameter CrearParametroEmail_380_jh(string nombre, string valor)
        {
            return new SqlParameter(nombre, SqlDbType.VarChar, 512)
            {
                Value = string.IsNullOrWhiteSpace(valor) ? (object)DBNull.Value : valor.Trim()
            };
        }

        private static SqlParameter CrearParametroFecha_380_jh(string nombre, DateTime? valor)
        {
            return new SqlParameter(nombre, SqlDbType.Date)
            {
                Value = valor.HasValue ? (object)valor.Value.Date : DBNull.Value
            };
        }
    }
}
