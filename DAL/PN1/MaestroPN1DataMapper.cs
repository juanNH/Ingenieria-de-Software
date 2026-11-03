using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Domain;

namespace DAL
{
    public class MaestroPN1DataMapper_380_jh
    {
        public List<MaestroPN1_380_jh> Listar_380_jh(string tipo, bool soloActivos)
        {
            var db = new DatabaseContext_380_jh();
            try
            {
                db.Abrir_380_jh();
                var parametros = new List<SqlParameter> {
                    new SqlParameter("@tipo", tipo), new SqlParameter("@solo_activos", soloActivos)
                };
                var resultado = new List<MaestroPN1_380_jh>();
                foreach (DataRow fila in db.Leer_380_jh("sp_MaestroPN1_Listar", parametros).Rows)
                    resultado.Add(new MaestroPN1_380_jh {
                        Id_380_jh = Convert.ToInt32(fila["id"]), Codigo_380_jh = Convert.ToString(fila["codigo"]),
                        Descripcion_380_jh = Convert.ToString(fila["descripcion"]), Activo_380_jh = Convert.ToBoolean(fila["activo"]),
                        MotivoObligatorio_380_jh = fila.Table.Columns.Contains("motivo_obligatorio") && Convert.ToBoolean(fila["motivo_obligatorio"])
                    });
                return resultado;
            }
            finally { db.Cerrar_380_jh(); }
        }

        public string Guardar_380_jh(string tipo, MaestroPN1_380_jh item, int idUsuario)
        {
            var db = new DatabaseContext_380_jh();
            try
            {
                db.Abrir_380_jh();
                var parametros = new List<SqlParameter> {
                    new SqlParameter("@tipo", tipo), new SqlParameter("@id", item.Id_380_jh),
                    new SqlParameter("@codigo", item.Codigo_380_jh), new SqlParameter("@descripcion", item.Descripcion_380_jh),
                    new SqlParameter("@activo", item.Activo_380_jh), new SqlParameter("@id_usuario", idUsuario),
                    new SqlParameter("@motivo_obligatorio", item.MotivoObligatorio_380_jh)
                };
                DataTable resultado = db.Leer_380_jh("sp_MaestroPN1_Guardar", parametros);
                return resultado.Rows.Count == 0 ? "ERROR" : Convert.ToString(resultado.Rows[0]["codigo_resultado"]);
            }
            finally { db.Cerrar_380_jh(); }
        }
    }
}
