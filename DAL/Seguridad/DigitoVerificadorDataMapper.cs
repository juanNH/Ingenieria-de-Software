using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace DAL
{
    public class DigitoVerificadorDataMapper_380_jh
    {
        private const string EntidadUsuario_380_jh = "Usuario";
        private readonly DatabaseContext_380_jh _databaseContext_380_jh;

        public DigitoVerificadorDataMapper_380_jh()
            : this(new DatabaseContext_380_jh())
        {
        }

        public DigitoVerificadorDataMapper_380_jh(DatabaseContext_380_jh databaseContext)
        {
            _databaseContext_380_jh = databaseContext;
        }

        public bool VerificarUsuarios_380_jh()
        {
            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                List<UsuarioDigitoRegistro_380_jh> usuarios = LeerUsuariosEnConexion_380_jh();

                if (usuarios.Count == 0)
                {
                    GuardarDvvEnConexion_380_jh(CalcularHash_380_jh(string.Empty));
                    return true;
                }

                string dvvRegistrado = ObtenerDvvEnConexion_380_jh();
                if (string.IsNullOrWhiteSpace(dvvRegistrado) || TodosSinDvh_380_jh(usuarios))
                {
                    RecalcularUsuariosEnConexion_380_jh(usuarios);
                    return true;
                }

                bool integridadValida = true;
                List<string> dvhCalculados = new List<string>();

                foreach (UsuarioDigitoRegistro_380_jh usuario in usuarios)
                {
                    string dvhCalculado = CalcularDvh_380_jh(usuario);
                    dvhCalculados.Add(dvhCalculado);

                    if (!string.Equals(usuario.Dvh_380_jh, dvhCalculado, StringComparison.OrdinalIgnoreCase))
                    {
                        MarcarBloqueoEnConexion_380_jh(usuario.IdUsuario_380_jh, true);
                        integridadValida = false;
                    }
                }

                string dvvCalculado = CalcularDvv_380_jh(dvhCalculados);
                if (!string.Equals(dvvRegistrado, dvvCalculado, StringComparison.OrdinalIgnoreCase))
                {
                    integridadValida = false;
                }

                return integridadValida;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public bool RecalcularUsuarios_380_jh()
        {
            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                RecalcularUsuariosEnConexion_380_jh(LeerUsuariosEnConexion_380_jh());
                return true;
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

        public bool RecalcularUsuarioYDvv_380_jh(int idUsuario)
        {
            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                UsuarioDigitoRegistro_380_jh usuario = LeerUsuarioEnConexion_380_jh(idUsuario);
                if (usuario == null)
                {
                    return false;
                }

                ActualizarDvhEnConexion_380_jh(usuario.IdUsuario_380_jh, CalcularDvh_380_jh(usuario), usuario.BloqueoDigitoVerificador_380_jh);
                List<UsuarioDigitoRegistro_380_jh> usuarios = LeerUsuariosEnConexion_380_jh();
                GuardarDvvEnConexion_380_jh(CalcularDvvDesdeUsuarios_380_jh(usuarios));
                return true;
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

        public bool HayBloqueoUsuarios_380_jh()
        {
            const string sql = @"
                SELECT TOP (1) id_usuario
                FROM dbo.Usuario
                WHERE bloqueo_digitoverificador = 1";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                return _databaseContext_380_jh.LeerTexto_380_jh(sql).Rows.Count > 0;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        private void RecalcularUsuariosEnConexion_380_jh(List<UsuarioDigitoRegistro_380_jh> usuarios)
        {
            List<string> dvhs = new List<string>();

            foreach (UsuarioDigitoRegistro_380_jh usuario in usuarios)
            {
                string dvh = CalcularDvh_380_jh(usuario);
                dvhs.Add(dvh);
                ActualizarDvhEnConexion_380_jh(usuario.IdUsuario_380_jh, dvh, false);
            }

            GuardarDvvEnConexion_380_jh(CalcularDvv_380_jh(dvhs));
        }

        private List<UsuarioDigitoRegistro_380_jh> LeerUsuariosEnConexion_380_jh()
        {
            const string sql = @"
                SELECT
                    id_usuario,
                    id_idioma,
                    nombre_usuario,
                    email,
                    password_hash,
                    nombre,
                    apellido,
                    estado_usuario,
                    intentos_login_fallidos,
                    bloqueo_digitoverificador,
                    dvh
                FROM dbo.Usuario
                ORDER BY id_usuario";

            DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql);
            List<UsuarioDigitoRegistro_380_jh> usuarios = new List<UsuarioDigitoRegistro_380_jh>();

            foreach (DataRow fila in tabla.Rows)
            {
                usuarios.Add(Mapear_380_jh(fila));
            }

            return usuarios;
        }

        private UsuarioDigitoRegistro_380_jh LeerUsuarioEnConexion_380_jh(int idUsuario)
        {
            const string sql = @"
                SELECT TOP (1)
                    id_usuario,
                    id_idioma,
                    nombre_usuario,
                    email,
                    password_hash,
                    nombre,
                    apellido,
                    estado_usuario,
                    intentos_login_fallidos,
                    bloqueo_digitoverificador,
                    dvh
                FROM dbo.Usuario
                WHERE id_usuario = @id_usuario";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", idUsuario)
            };

            DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);
            return tabla.Rows.Count == 0 ? null : Mapear_380_jh(tabla.Rows[0]);
        }

        private void ActualizarDvhEnConexion_380_jh(int idUsuario, string dvh, bool bloqueoDigitoVerificador)
        {
            const string sql = @"
                UPDATE dbo.Usuario
                SET dvh = @dvh,
                    bloqueo_digitoverificador = @bloqueo_digitoverificador
                WHERE id_usuario = @id_usuario";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", idUsuario),
                _databaseContext_380_jh.CrearParametro_380_jh("@dvh", dvh),
                new SqlParameter("@bloqueo_digitoverificador", SqlDbType.Bit)
                {
                    Value = bloqueoDigitoVerificador
                }
            };

            _databaseContext_380_jh.EscribirTexto_380_jh(sql, parametros);
        }

        private void MarcarBloqueoEnConexion_380_jh(int idUsuario, bool bloqueo)
        {
            const string sql = @"
                UPDATE dbo.Usuario
                SET bloqueo_digitoverificador = @bloqueo_digitoverificador
                WHERE id_usuario = @id_usuario";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", idUsuario),
                new SqlParameter("@bloqueo_digitoverificador", SqlDbType.Bit)
                {
                    Value = bloqueo
                }
            };

            _databaseContext_380_jh.EscribirTexto_380_jh(sql, parametros);
        }

        private string ObtenerDvvEnConexion_380_jh()
        {
            const string sql = @"
                SELECT TOP (1) dvv
                FROM dbo.DigitoVerificadorVertical
                WHERE entidad = @entidad";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@entidad", EntidadUsuario_380_jh)
            };

            DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);
            return tabla.Rows.Count == 0 ? null : tabla.Rows[0]["dvv"].ToString();
        }

        private void GuardarDvvEnConexion_380_jh(string dvv)
        {
            const string sql = @"
                MERGE dbo.DigitoVerificadorVertical AS destino
                USING (SELECT @entidad AS entidad) AS origen
                    ON destino.entidad = origen.entidad
                WHEN MATCHED THEN
                    UPDATE SET dvv = @dvv,
                               fecha_calculo = GETDATE()
                WHEN NOT MATCHED THEN
                    INSERT (entidad, dvv, fecha_calculo)
                    VALUES (@entidad, @dvv, GETDATE());";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@entidad", EntidadUsuario_380_jh),
                _databaseContext_380_jh.CrearParametro_380_jh("@dvv", dvv)
            };

            _databaseContext_380_jh.EscribirTexto_380_jh(sql, parametros);
        }

        private static bool TodosSinDvh_380_jh(List<UsuarioDigitoRegistro_380_jh> usuarios)
        {
            foreach (UsuarioDigitoRegistro_380_jh usuario in usuarios)
            {
                if (!string.IsNullOrWhiteSpace(usuario.Dvh_380_jh))
                {
                    return false;
                }
            }

            return true;
        }

        private static string CalcularDvvDesdeUsuarios_380_jh(List<UsuarioDigitoRegistro_380_jh> usuarios)
        {
            List<string> dvhs = new List<string>();

            foreach (UsuarioDigitoRegistro_380_jh usuario in usuarios)
            {
                dvhs.Add(string.IsNullOrWhiteSpace(usuario.Dvh_380_jh) ? CalcularDvh_380_jh(usuario) : usuario.Dvh_380_jh);
            }

            return CalcularDvv_380_jh(dvhs);
        }

        private static string CalcularDvv_380_jh(List<string> dvhs)
        {
            StringBuilder builder = new StringBuilder();

            foreach (string dvh in dvhs)
            {
                builder.Append(dvh ?? string.Empty);
                builder.Append("|");
            }

            return CalcularHash_380_jh(builder.ToString());
        }

        private static string CalcularDvh_380_jh(UsuarioDigitoRegistro_380_jh usuario)
        {
            string datos = string.Join("|", new[]
            {
                usuario.IdUsuario_380_jh.ToString(CultureInfo.InvariantCulture),
                usuario.IdIdioma_380_jh.HasValue ? usuario.IdIdioma_380_jh.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                usuario.NombreUsuario_380_jh ?? string.Empty,
                usuario.Email_380_jh ?? string.Empty,
                usuario.PasswordHash_380_jh ?? string.Empty,
                usuario.Nombre_380_jh ?? string.Empty,
                usuario.Apellido_380_jh ?? string.Empty,
                usuario.EstadoUsuario_380_jh ?? string.Empty,
                usuario.IntentosLoginFallidos_380_jh.ToString(CultureInfo.InvariantCulture)
            });

            return CalcularHash_380_jh(datos);
        }

        private static string CalcularHash_380_jh(string texto)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto ?? string.Empty));
                StringBuilder builder = new StringBuilder(bytes.Length * 2);

                foreach (byte valor in bytes)
                {
                    builder.Append(valor.ToString("x2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }

        private static UsuarioDigitoRegistro_380_jh Mapear_380_jh(DataRow fila)
        {
            return new UsuarioDigitoRegistro_380_jh
            {
                IdUsuario_380_jh = Convert.ToInt32(fila["id_usuario"]),
                IdIdioma_380_jh = fila["id_idioma"] == DBNull.Value ? (int?)null : Convert.ToInt32(fila["id_idioma"]),
                NombreUsuario_380_jh = fila["nombre_usuario"].ToString(),
                Email_380_jh = fila["email"].ToString(),
                PasswordHash_380_jh = fila["password_hash"].ToString(),
                Nombre_380_jh = fila["nombre"] == DBNull.Value ? null : fila["nombre"].ToString(),
                Apellido_380_jh = fila["apellido"] == DBNull.Value ? null : fila["apellido"].ToString(),
                EstadoUsuario_380_jh = fila["estado_usuario"].ToString(),
                IntentosLoginFallidos_380_jh = Convert.ToInt32(fila["intentos_login_fallidos"]),
                BloqueoDigitoVerificador_380_jh = fila["bloqueo_digitoverificador"] != DBNull.Value &&
                                           Convert.ToBoolean(fila["bloqueo_digitoverificador"]),
                Dvh_380_jh = fila["dvh"] == DBNull.Value ? null : fila["dvh"].ToString()
            };
        }

        private class UsuarioDigitoRegistro_380_jh
        {
            public int IdUsuario_380_jh { get; set; }
            public int? IdIdioma_380_jh { get; set; }
            public string NombreUsuario_380_jh { get; set; }
            public string Email_380_jh { get; set; }
            public string PasswordHash_380_jh { get; set; }
            public string Nombre_380_jh { get; set; }
            public string Apellido_380_jh { get; set; }
            public string EstadoUsuario_380_jh { get; set; }
            public int IntentosLoginFallidos_380_jh { get; set; }
            public bool BloqueoDigitoVerificador_380_jh { get; set; }
            public string Dvh_380_jh { get; set; }
        }
    }
}
