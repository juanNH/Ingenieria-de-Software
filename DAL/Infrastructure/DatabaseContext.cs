using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Xml.Linq;

namespace DAL
{
    public class DatabaseContext_380_jh
    {
        private static readonly string ConnectionString_380_jh = ResolveConnectionString_380_jh();

        public SqlConnection Conexion_380_jh { get; private set; }
        public SqlTransaction Transaccion_380_jh { get; private set; }

        public void Abrir_380_jh()
        {
            Conexion_380_jh = new SqlConnection(ConnectionString_380_jh);
            Conexion_380_jh.Open();
        }

        public void Cerrar_380_jh()
        {
            if (Conexion_380_jh != null)
            {
                Conexion_380_jh.Close();
                Conexion_380_jh = null;
            }
        }

        #region Transaccion
        public void IniciarTx_380_jh()
        {
            if (Conexion_380_jh != null)
            {
                Transaccion_380_jh = Conexion_380_jh.BeginTransaction();
            }
        }

        public void Confirmar_380_jh()
        {
            if (Transaccion_380_jh != null)
            {
                Transaccion_380_jh.Commit();
                Transaccion_380_jh = null;
            }
        }

        public void Deshacer_380_jh()
        {
            if (Transaccion_380_jh != null)
            {
                Transaccion_380_jh.Rollback();
                Transaccion_380_jh = null;
            }
        }
        #endregion
        public SqlParameter CrearParametro_380_jh(string nombre, string valor)
        {
            return new SqlParameter
            {
                ParameterName = nombre,
                Value = string.IsNullOrWhiteSpace(valor) ? (object)DBNull.Value : valor,
                DbType = DbType.String
            };
        }

        public SqlParameter CrearParametro_380_jh(string nombre, int valor)
        {
            return new SqlParameter
            {
                ParameterName = nombre,
                Value = valor,
                DbType = DbType.Int32
            };
        }

        public int Escribir_380_jh(string sql, List<SqlParameter> parametros = null)
        {
            using (SqlCommand comando = CrearComando_380_jh(sql, parametros))
            {
                try
                {
                    return comando.ExecuteNonQuery();
                }
                catch
                {
                    return -1;
                }
            }
        }

        public int EscribirTexto_380_jh(string sql, List<SqlParameter> parametros = null)
        {
            using (SqlCommand comando = CrearComando_380_jh(sql, parametros, CommandType.Text))
            {
                try
                {
                    return comando.ExecuteNonQuery();
                }
                catch
                {
                    return -1;
                }
            }
        }

        public DataTable Leer_380_jh(string sql, List<SqlParameter> parametros = null)
        {
            using (SqlDataAdapter adaptador = new SqlDataAdapter())
            {
                DataTable tabla = new DataTable();
                adaptador.SelectCommand = CrearComando_380_jh(sql, parametros);
                adaptador.Fill(tabla);
                return tabla;
            }
        }

        public DataTable LeerTexto_380_jh(string sql, List<SqlParameter> parametros = null)
        {
            using (SqlDataAdapter adaptador = new SqlDataAdapter())
            {
                DataTable tabla = new DataTable();
                adaptador.SelectCommand = CrearComando_380_jh(sql, parametros, CommandType.Text);
                adaptador.Fill(tabla);
                return tabla;
            }
        }

        private SqlCommand CrearComando_380_jh(string sql, List<SqlParameter> parametros = null, CommandType commandType = CommandType.StoredProcedure)
        {
            SqlCommand comando = new SqlCommand(sql, Conexion_380_jh)
            {
                CommandType = commandType
            };

            if (Transaccion_380_jh != null)
            {
                comando.Transaction = Transaccion_380_jh;
            }

            if (parametros != null && parametros.Count > 0)
            {
                comando.Parameters.AddRange(parametros.ToArray());
            }

            return comando;
        }

        private static string ResolveConnectionString_380_jh()
        {
            string localConnectionString = ReadLocalConnectionString_380_jh();
            if (!string.IsNullOrWhiteSpace(localConnectionString))
            {
                return localConnectionString;
            }

            return ConfigurationManager.ConnectionStrings["TecniSalud"]?.ConnectionString;
        }

        private static string ReadLocalConnectionString_380_jh()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidatePaths =
            {
                Path.Combine(baseDirectory, "App.local.config"),
                Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\App.local.config")),
                Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\..\App.local.config"))
            };

            foreach (string localConfigPath in candidatePaths)
            {
                if (!File.Exists(localConfigPath))
                {
                    continue;
                }

                XDocument document = XDocument.Load(localConfigPath);
                XElement connectionElement = document.Root?
                    .Element("connectionStrings")?
                    .Element("add");

                string connectionString = connectionElement?.Attribute("connectionString")?.Value;
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    return connectionString;
                }
            }

            return null;
        }
    }
}
