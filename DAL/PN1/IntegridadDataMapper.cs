using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Domain;

namespace DAL
{
    public class IntegridadDataMapper_380_jh
    {
        private readonly DatabaseContext_380_jh _contexto_380_jh = new DatabaseContext_380_jh();

        // No se ocultan errores como un informe valido o una lista vacia.
        public InformeIntegridad_380_jh Verificar_380_jh(int idUsuario)
        {
            try
            {
                _contexto_380_jh.Abrir_380_jh();
                DataSet datos = _contexto_380_jh.LeerConjunto_380_jh("sp_IntegridadPN1_Verificar", Parametros_380_jh(idUsuario));
                if (datos.Tables.Count != 2 || datos.Tables[0].Rows.Count != 1)
                    throw new InvalidOperationException("Invalid integrity report.");
                DataRow cabecera = datos.Tables[0].Rows[0];
                var informe = new InformeIntegridad_380_jh
                {
                    Disponible_380_jh = true,
                    EsValida_380_jh = (bool)cabecera["es_valida"],
                    PuedeInicializar_380_jh = (bool)cabecera["puede_inicializar"],
                    Revision_380_jh = cabecera["revision"].ToString()
                };
                foreach (DataRow fila in datos.Tables[1].Rows)
                    informe.Incidencias_380_jh.Add(new IncidenciaIntegridad_380_jh
                    {
                        Entidad_380_jh = fila["entidad"].ToString(),
                        IdEntidad_380_jh = (int)fila["id_entidad"],
                        Tipo_380_jh = fila["tipo"].ToString(),
                        Fecha_380_jh = (DateTime)fila["fecha_deteccion"],
                        EstadoActual_380_jh = fila["estado_actual"].ToString(),
                        EstadoConfiable_380_jh = fila["estado_confiable"].ToString()
                    });
                return informe;
            }
            finally { _contexto_380_jh.Cerrar_380_jh(); }
        }

        public string Recuperar_380_jh(int idUsuario, string revision, bool inicializar)
        {
            try
            {
                var parametros = Parametros_380_jh(idUsuario);
                parametros.Add(new SqlParameter("@revision", SqlDbType.VarChar, 64) { Value = (object)revision ?? DBNull.Value });
                _contexto_380_jh.Abrir_380_jh();
                DataTable resultado = _contexto_380_jh.Leer_380_jh(inicializar ? "sp_IntegridadPN1_Inicializar" : "sp_IntegridadPN1_Restaurar", parametros);
                return resultado.Rows.Count == 1 && resultado.Rows[0]["codigo_resultado"].ToString() == "OK"
                    ? "INTEGRITY_RECOVERED" : "INTEGRITY_RECOVERY_FAILED";
            }
            catch (SqlException ex)
            {
                switch (ex.Number)
                {
                    case 51003: return "OPERATION_NOT_AUTHORIZED";
                    case 51005: return "INTEGRITY_STALE_PREVIEW";
                    case 51006: return "INTEGRITY_BACKUP_REQUIRED";
                    default: return "INTEGRITY_RECOVERY_FAILED";
                }
            }
            finally { _contexto_380_jh.Cerrar_380_jh(); }
        }

        private static List<SqlParameter> Parametros_380_jh(int idUsuario)
        {
            return new List<SqlParameter> { new SqlParameter("@id_usuario", SqlDbType.Int) { Value = idUsuario > 0 ? (object)idUsuario : DBNull.Value } };
        }
    }
}
