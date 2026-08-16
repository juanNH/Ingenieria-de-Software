using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Domain;

namespace DAL
{
    public class UsuarioDataMapper_380_jh
    {
        private readonly DatabaseContext_380_jh _databaseContext_380_jh;

        public UsuarioDataMapper_380_jh()
            : this(new DatabaseContext_380_jh())
        {
        }

        public UsuarioDataMapper_380_jh(DatabaseContext_380_jh databaseContext)
        {
            _databaseContext_380_jh = databaseContext;
        }

        public CodigoRegistroUsuario_380_jh Insertar_380_jh(Usuario_380_jh usuario)
        {
            if (usuario == null)
            {
                return CodigoRegistroUsuario_380_jh.DatosInvalidos_380_jh;
            }

            SqlParameter idUsuarioNuevo = new SqlParameter("@id_usuario_nuevo", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@nombre_usuario", usuario.Username_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@email", usuario.Email_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@password_hash", usuario.Password_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@nombre", usuario.Nombre_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@apellido", usuario.Apellido_380_jh),
                idUsuarioNuevo
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.Leer_380_jh("sp_Usuario_Registrar", parametros);

                if (tabla.Rows.Count == 0)
                {
                    return CodigoRegistroUsuario_380_jh.ErrorBaseDatos_380_jh;
                }

                if (tabla.Columns.Contains("codigo_resultado"))
                {
                    return MapearResultadoRegistro_380_jh(tabla.Rows[0], usuario);
                }

                usuario.Id_380_jh = Convert.ToInt32(tabla.Rows[0]["id_usuario"]);
                usuario.IntentosLoginFallidos_380_jh = tabla.Columns.Contains("intentos_login_fallidos")
                    ? Convert.ToInt32(tabla.Rows[0]["intentos_login_fallidos"])
                    : 0;
                return CodigoRegistroUsuario_380_jh.Creado_380_jh;
            }
            catch (SqlException ex)
            {
                return MapearErrorSqlRegistro_380_jh(ex);
            }
            catch
            {
                return CodigoRegistroUsuario_380_jh.ErrorBaseDatos_380_jh;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public Usuario_380_jh ObtenerPorCredenciales_380_jh(string identificador, string passwordHash)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@identificador", identificador),
                _databaseContext_380_jh.CrearParametro_380_jh("@password_hash", passwordHash)
            };

            const string sql = @"
                SELECT TOP (1)
                    id_usuario,
                    id_idioma,
                    nombre_usuario,
                    email,
                    nombre,
                    apellido,
                    estado_usuario,
                    intentos_login_fallidos,
                    bloqueo_digitoverificador,
                    dvh
                FROM dbo.Usuario
                WHERE (nombre_usuario = @identificador OR email = @identificador)
                  AND password_hash = @password_hash
                  AND estado_usuario = 'ACTIVO'";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);

                if (tabla.Rows.Count == 0)
                {
                    return null;
                }

                return MapearUsuario_380_jh(tabla.Rows[0]);
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public bool ExistePorNombreUsuario_380_jh(string username)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@nombre_usuario", username)
            };

            const string sql = @"
                SELECT TOP (1) id_usuario
                FROM dbo.Usuario
                WHERE nombre_usuario = @nombre_usuario";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);
                return tabla.Rows.Count > 0;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public Usuario_380_jh ObtenerActivoPorIdentificador_380_jh(string identificador)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@identificador", identificador)
            };

            const string sql = @"
                SELECT TOP (1)
                    id_usuario,
                    id_idioma,
                    nombre_usuario,
                    email,
                    nombre,
                    apellido,
                    estado_usuario,
                    intentos_login_fallidos,
                    bloqueo_digitoverificador,
                    dvh
                FROM dbo.Usuario
                WHERE (nombre_usuario = @identificador OR email = @identificador)
                  AND estado_usuario = 'ACTIVO'";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);

                if (tabla.Rows.Count == 0)
                {
                    return null;
                }

                return MapearUsuario_380_jh(tabla.Rows[0]);
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public Usuario_380_jh ObtenerPorId_380_jh(int id)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", id)
            };

            const string sql = @"
                SELECT TOP (1)
                    id_usuario,
                    id_idioma,
                    nombre_usuario,
                    email,
                    nombre,
                    apellido,
                    estado_usuario,
                    intentos_login_fallidos,
                    bloqueo_digitoverificador,
                    dvh
                FROM dbo.Usuario
                WHERE id_usuario = @id_usuario";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);

                if (tabla.Rows.Count == 0)
                {
                    return null;
                }

                return MapearUsuario_380_jh(tabla.Rows[0]);
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public bool EstaBloqueadoPorIdentificador_380_jh(string identificador)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@identificador", identificador)
            };

            const string sql = @"
                SELECT TOP (1) id_usuario
                FROM dbo.Usuario
                WHERE (nombre_usuario = @identificador OR email = @identificador)
                  AND estado_usuario <> 'ACTIVO'";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);
                return tabla.Rows.Count > 0;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public int RegistrarLoginFallidoPorIdentificador_380_jh(string identificador)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@identificador", identificador)
            };

            const string sql = @"
                UPDATE dbo.Usuario
                SET intentos_login_fallidos = ISNULL(intentos_login_fallidos, 0) + 1,
                    estado_usuario = CASE
                        WHEN ISNULL(intentos_login_fallidos, 0) + 1 >= 3 THEN 'INACTIVO'
                        ELSE estado_usuario
                    END
                OUTPUT INSERTED.intentos_login_fallidos
                WHERE (nombre_usuario = @identificador OR email = @identificador)
                  AND estado_usuario = 'ACTIVO'";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);

                if (tabla.Rows.Count == 0)
                {
                    return -1;
                }

                return Convert.ToInt32(tabla.Rows[0]["intentos_login_fallidos"]);
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public int ReiniciarIntentosLoginFallidos_380_jh(int idUsuario)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", idUsuario)
            };

            const string sql = @"
                UPDATE dbo.Usuario
                SET intentos_login_fallidos = 0
                WHERE id_usuario = @id_usuario";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                return _databaseContext_380_jh.EscribirTexto_380_jh(sql, parametros);
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public int Editar_380_jh(Usuario_380_jh usuario)
        {
            if (usuario == null || usuario.Id_380_jh == 0)
            {
                return -1;
            }

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", usuario.Id_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@nombre_usuario", usuario.Username_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@email", usuario.Email_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@nombre", usuario.Nombre_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@apellido", usuario.Apellido_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@estado_usuario", usuario.Estado_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@password_hash", string.IsNullOrWhiteSpace(usuario.Password_380_jh) ? null : usuario.Password_380_jh),
                new SqlParameter("@id_idioma", SqlDbType.Int)
                {
                    Value = usuario.IdiomaPreferidoId_380_jh.HasValue ? (object)usuario.IdiomaPreferidoId_380_jh.Value : DBNull.Value
                }
            };

            const string sql = @"
                UPDATE dbo.Usuario
                SET nombre_usuario = @nombre_usuario,
                    email = @email,
                    nombre = @nombre,
                    apellido = @apellido,
                    id_idioma = @id_idioma,
                    estado_usuario = @estado_usuario,
                    intentos_login_fallidos = CASE
                        WHEN @estado_usuario = 'ACTIVO' AND estado_usuario <> 'ACTIVO' THEN 0
                        ELSE intentos_login_fallidos
                    END,
                    password_hash = COALESCE(@password_hash, password_hash)
                WHERE id_usuario = @id_usuario";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                return _databaseContext_380_jh.EscribirTexto_380_jh(sql, parametros);
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public int Borrar_380_jh(Usuario_380_jh usuario)
        {
            if (usuario == null || usuario.Id_380_jh == 0)
            {
                return -1;
            }

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", usuario.Id_380_jh)
            };

            const string sql = @"
                UPDATE dbo.Usuario
                SET estado_usuario = 'INACTIVO'
                WHERE id_usuario = @id_usuario";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                return _databaseContext_380_jh.EscribirTexto_380_jh(sql, parametros);
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public List<Usuario_380_jh> Listar_380_jh()
        {
            const string sql = @"
                SELECT
                    u.id_usuario,
                    u.id_idioma,
                    u.nombre_usuario,
                    u.email,
                    u.nombre,
                    u.apellido,
                    u.estado_usuario,
                    u.intentos_login_fallidos,
                    u.bloqueo_digitoverificador,
                    u.dvh
                FROM dbo.Usuario u
                ORDER BY u.nombre_usuario";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql);
                List<Usuario_380_jh> usuarios = new List<Usuario_380_jh>();

                foreach (DataRow registro in tabla.Rows)
                {
                    usuarios.Add(MapearUsuario_380_jh(registro));
                }

                return usuarios;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public int ActualizarIdiomaPreferido_380_jh(int usuarioId, int idiomaId)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", usuarioId),
                _databaseContext_380_jh.CrearParametro_380_jh("@id_idioma", idiomaId)
            };

            const string sql = @"
                UPDATE dbo.Usuario
                SET id_idioma = @id_idioma
                WHERE id_usuario = @id_usuario";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                return _databaseContext_380_jh.EscribirTexto_380_jh(sql, parametros);
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public int RestaurarCampo_380_jh(int idUsuario, string campo, object valor)
        {
            if (idUsuario == 0 || string.IsNullOrWhiteSpace(campo))
            {
                return -1;
            }

            string columna = ObtenerColumnaRestaurable_380_jh(campo);
            if (string.IsNullOrWhiteSpace(columna))
            {
                return -1;
            }

            string sql = "UPDATE dbo.Usuario SET " + columna + " = @valor WHERE id_usuario = @id_usuario";
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", idUsuario),
                CrearParametroRestauracion_380_jh("@valor", campo, valor)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                return _databaseContext_380_jh.EscribirTexto_380_jh(sql, parametros);
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        private static Usuario_380_jh MapearUsuario_380_jh(DataRow registro)
        {
            int? idiomaPreferidoId = null;
            if (registro.Table.Columns.Contains("id_idioma") && registro["id_idioma"] != DBNull.Value)
            {
                idiomaPreferidoId = Convert.ToInt32(registro["id_idioma"]);
            }

            return new Usuario_380_jh
            {
                Id_380_jh = Convert.ToInt32(registro["id_usuario"]),
                Username_380_jh = registro["nombre_usuario"].ToString(),
                Email_380_jh = registro["email"].ToString(),
                Nombre_380_jh = registro.Table.Columns.Contains("nombre") && registro["nombre"] != DBNull.Value
                    ? registro["nombre"].ToString()
                    : null,
                Apellido_380_jh = registro.Table.Columns.Contains("apellido") && registro["apellido"] != DBNull.Value
                    ? registro["apellido"].ToString()
                    : null,
                Idioma_380_jh = idiomaPreferidoId.HasValue ? idiomaPreferidoId.Value.ToString() : null,
                IdiomaPreferidoId_380_jh = idiomaPreferidoId,
                Estado_380_jh = registro.Table.Columns.Contains("estado_usuario")
                    ? registro["estado_usuario"].ToString()
                    : null,
                IntentosLoginFallidos_380_jh = registro.Table.Columns.Contains("intentos_login_fallidos")
                    ? Convert.ToInt32(registro["intentos_login_fallidos"])
                    : 0,
                BloqueoDigitoVerificador_380_jh = registro.Table.Columns.Contains("bloqueo_digitoverificador") &&
                                           registro["bloqueo_digitoverificador"] != DBNull.Value &&
                                           Convert.ToBoolean(registro["bloqueo_digitoverificador"]),
                Dvh_380_jh = registro.Table.Columns.Contains("dvh") && registro["dvh"] != DBNull.Value
                    ? registro["dvh"].ToString()
                    : null
            };
        }

        private static CodigoRegistroUsuario_380_jh MapearResultadoRegistro_380_jh(DataRow registro, Usuario_380_jh usuario)
        {
            string codigoResultado = registro["codigo_resultado"].ToString();

            switch (codigoResultado)
            {
                case "OK":
                    usuario.Id_380_jh = Convert.ToInt32(registro["id_usuario"]);
                    usuario.IntentosLoginFallidos_380_jh = registro.Table.Columns.Contains("intentos_login_fallidos")
                        ? Convert.ToInt32(registro["intentos_login_fallidos"])
                        : 0;
                    return CodigoRegistroUsuario_380_jh.Creado_380_jh;

                case "USUARIO_EXISTENTE":
                    return CodigoRegistroUsuario_380_jh.UsuarioExistente_380_jh;

                case "EMAIL_EXISTENTE":
                    return CodigoRegistroUsuario_380_jh.EmailExistente_380_jh;

                case "IDIOMA_DEFAULT_INEXISTENTE":
                    return CodigoRegistroUsuario_380_jh.IdiomaDefaultInexistente_380_jh;

                default:
                    return CodigoRegistroUsuario_380_jh.ErrorBaseDatos_380_jh;
            }
        }

        private static CodigoRegistroUsuario_380_jh MapearErrorSqlRegistro_380_jh(SqlException ex)
        {
            if (ex.Number == 2601 || ex.Number == 2627)
            {
                string mensaje = ex.Message ?? string.Empty;

                if (mensaje.IndexOf("email", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return CodigoRegistroUsuario_380_jh.EmailExistente_380_jh;
                }

                return CodigoRegistroUsuario_380_jh.UsuarioExistente_380_jh;
            }

            return CodigoRegistroUsuario_380_jh.ErrorBaseDatos_380_jh;
        }

        private static string ObtenerColumnaRestaurable_380_jh(string campo)
        {
            switch (campo)
            {
                case "Username":
                    return "nombre_usuario";

                case "Email":
                    return "email";

                case "Nombre":
                    return "nombre";

                case "Apellido":
                    return "apellido";

                case "IdiomaPreferidoId":
                    return "id_idioma";

                case "Estado":
                    return "estado_usuario";

                case "IntentosLoginFallidos":
                    return "intentos_login_fallidos";

                case "BloqueoDigitoVerificador":
                    return "bloqueo_digitoverificador";

                default:
                    return null;
            }
        }

        private static SqlParameter CrearParametroRestauracion_380_jh(string nombre, string campo, object valor)
        {
            SqlParameter parametro = new SqlParameter
            {
                ParameterName = nombre,
                Value = valor ?? DBNull.Value
            };

            switch (campo)
            {
                case "IdiomaPreferidoId":
                case "IntentosLoginFallidos":
                    parametro.SqlDbType = SqlDbType.Int;
                    parametro.Value = valor == null ? (object)DBNull.Value : Convert.ToInt32(valor);
                    break;

                case "BloqueoDigitoVerificador":
                    parametro.SqlDbType = SqlDbType.Bit;
                    parametro.Value = valor == null ? (object)DBNull.Value : Convert.ToBoolean(valor);
                    break;

                default:
                    parametro.SqlDbType = SqlDbType.NVarChar;
                    parametro.Value = valor == null ? (object)DBNull.Value : valor.ToString();
                    break;
            }

            return parametro;
        }
    }
}
