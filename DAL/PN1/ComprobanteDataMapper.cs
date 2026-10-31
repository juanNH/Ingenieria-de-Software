using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Domain;

namespace DAL
{
    public class ComprobanteDataMapper_380_jh
    {
        public bool Disponible_380_jh()
        {
            DatabaseContext_380_jh db = new DatabaseContext_380_jh();
            try
            {
                db.Abrir_380_jh();
                DataTable filas = db.Leer_380_jh("sp_Comprobante_Estado");
                return filas.Rows.Count == 1 && Convert.ToInt32(filas.Rows[0]["disponible"]) == 1;
            }
            finally { db.Cerrar_380_jh(); }
        }

        public List<ComprobantePendiente_380_jh> ListarPendientes_380_jh(int idUsuario)
        {
            DatabaseContext_380_jh db = new DatabaseContext_380_jh();
            try
            {
                db.Abrir_380_jh();
                DataTable filas = db.Leer_380_jh("sp_Comprobante_Pendientes", new List<SqlParameter> { db.CrearParametro_380_jh("@id_usuario", idUsuario) });
                List<ComprobantePendiente_380_jh> resultado = new List<ComprobantePendiente_380_jh>();
                foreach (DataRow fila in filas.Rows)
                    resultado.Add(new ComprobantePendiente_380_jh { Id_380_jh = Convert.ToInt64(fila["id_comprobante"]), Identificador_380_jh = (Guid)fila["identificador"],
                        Proceso_380_jh = Convert.ToString(fila["proceso"]), IdReferencia_380_jh = Convert.ToInt32(fila["id_referencia"]),
                        DatosXml_380_jh = Convert.ToString(fila["datos"]) });
                return resultado;
            }
            finally { db.Cerrar_380_jh(); }
        }

        public void MarcarGenerado_380_jh(long id, int idUsuario, string ruta)
        {
            DatabaseContext_380_jh db = new DatabaseContext_380_jh();
            try
            {
                db.Abrir_380_jh();
                // Leer permite propagar cualquier error SQL; Escribir devuelve -1 al fallar.
                db.Leer_380_jh("sp_Comprobante_MarcarGenerado", new List<SqlParameter> {
                    new SqlParameter("@id_comprobante", SqlDbType.BigInt) { Value = id },
                    db.CrearParametro_380_jh("@id_usuario", idUsuario),
                    new SqlParameter("@ruta", SqlDbType.NVarChar, 2048) { Value = ruta } });
            }
            finally { db.Cerrar_380_jh(); }
        }

        public string ObtenerOriginal_380_jh(string proceso, int idReferencia, int idUsuario)
        {
            DatabaseContext_380_jh db = new DatabaseContext_380_jh();
            try
            {
                db.Abrir_380_jh();
                DataTable filas = db.Leer_380_jh("sp_Comprobante_Original", new List<SqlParameter> {
                    new SqlParameter("@proceso", SqlDbType.VarChar, 3) { Value = proceso },
                    db.CrearParametro_380_jh("@id_referencia", idReferencia), db.CrearParametro_380_jh("@id_usuario", idUsuario) });
                return filas.Rows.Count == 0 || filas.Rows[0].IsNull("ruta") ? null : Convert.ToString(filas.Rows[0]["ruta"]);
            }
            finally { db.Cerrar_380_jh(); }
        }
    }
}
