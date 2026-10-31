using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Domain;

namespace DAL
{
    public class PN2DataMapper_380_jh
    {
        private readonly DatabaseContext_380_jh _databaseContext_380_jh;

        public PN2DataMapper_380_jh() : this(new DatabaseContext_380_jh()) { }

        public PN2DataMapper_380_jh(DatabaseContext_380_jh databaseContext)
        {
            _databaseContext_380_jh = databaseContext;
        }

        public List<Unidad_380_jh> ListarInventario_380_jh(string buscar, string estado, int idUsuario)
        {
            return Leer_380_jh("sp_PN2_Inventario_Listar", ParametrosUsuario_380_jh(idUsuario,
                Texto_380_jh("@buscar", buscar, 150), Texto_380_jh("@estado", estado, 20, false)),
                UnidadDataMapper_380_jh.MapearPublico_380_jh);
        }

        public CodigoOperacionPN2_380_jh ActualizarUbicacion_380_jh(int idUnidad, string ubicacion, string observacion, int idUsuario)
        {
            int id;
            return Ejecutar_380_jh("sp_PN2_Ubicacion_Actualizar", ParametrosUsuario_380_jh(idUsuario,
                Entero_380_jh("@id_unidad", idUnidad), Texto_380_jh("@ubicacion", ubicacion, 150),
                Texto_380_jh("@observacion", observacion, 500)), out id);
        }

        public List<SolicitudTransfusional_380_jh> ListarSolicitudes_380_jh(string buscar, int idUsuario)
        {
            return Leer_380_jh("sp_SolicitudTransfusional_Listar", ParametrosUsuario_380_jh(idUsuario,
                Texto_380_jh("@buscar", buscar, 150)), MapearSolicitud_380_jh);
        }

        public CodigoOperacionPN2_380_jh RegistrarSolicitud_380_jh(SolicitudTransfusional_380_jh solicitud, int idUsuario)
        {
            int id;
            CodigoOperacionPN2_380_jh resultado = Ejecutar_380_jh("sp_SolicitudTransfusional_Registrar",
                ParametrosUsuario_380_jh(idUsuario, Texto_380_jh("@paciente_receptor", solicitud.PacienteReceptor_380_jh, 150),
                    Texto_380_jh("@tipo_componente", solicitud.TipoComponente_380_jh, 100),
                    Entero_380_jh("@cantidad_requerida", solicitud.CantidadRequerida_380_jh),
                    Texto_380_jh("@documento_receptor", solicitud.DocumentoReceptor_380_jh, 50),
                    Texto_380_jh("@grupo_receptor", solicitud.GrupoReceptor_380_jh, 5, false),
                    Texto_380_jh("@rh_receptor", solicitud.RhReceptor_380_jh, 5, false),
                    Texto_380_jh("@laboratorio_estudio", solicitud.LaboratorioEstudio_380_jh, 150),
                    Texto_380_jh("@referencia_estudio", solicitud.ReferenciaEstudio_380_jh, 150),
                    Fecha_380_jh("@fecha_estudio", solicitud.FechaEstudio_380_jh),
                    Texto_380_jh("@resultado_anticuerpos", solicitud.ResultadoAnticuerpos_380_jh, 500),
                    Texto_380_jh("@nombre_solicitante", solicitud.ProfesionalSolicitante_380_jh, 150)), out id);
            if (resultado == CodigoOperacionPN2_380_jh.Ok_380_jh)
            {
                solicitud.Id_380_jh = id;
                solicitud.IdUsuarioSolicitante_380_jh = idUsuario;
            }
            return resultado;
        }

        public CodigoOperacionPN2_380_jh ActualizarEstudio_380_jh(SolicitudTransfusional_380_jh solicitud, int idUsuario)
        {
            int id;
            return Ejecutar_380_jh("sp_PN2_ActualizarEstudio", ParametrosUsuario_380_jh(idUsuario,
                Entero_380_jh("@id_solicitud", solicitud.Id_380_jh),
                Texto_380_jh("@documento_receptor", solicitud.DocumentoReceptor_380_jh, 50),
                Texto_380_jh("@grupo_receptor", solicitud.GrupoReceptor_380_jh, 5, false),
                Texto_380_jh("@rh_receptor", solicitud.RhReceptor_380_jh, 5, false),
                Texto_380_jh("@laboratorio_estudio", solicitud.LaboratorioEstudio_380_jh, 150),
                Texto_380_jh("@referencia_estudio", solicitud.ReferenciaEstudio_380_jh, 150),
                Fecha_380_jh("@fecha_estudio", solicitud.FechaEstudio_380_jh),
                Texto_380_jh("@resultado_anticuerpos", solicitud.ResultadoAnticuerpos_380_jh, 500),
                Texto_380_jh("@nombre_solicitante", solicitud.ProfesionalSolicitante_380_jh, 150)), out id);
        }

        public CodigoOperacionPN2_380_jh ValidarEstudio_380_jh(int idSolicitud, int idUsuario)
        {
            int id;
            return Ejecutar_380_jh("sp_PN2_ValidarEstudio", ParametrosUsuario_380_jh(idUsuario,
                Entero_380_jh("@id_solicitud", idSolicitud)), out id);
        }

        public List<Unidad_380_jh> ListarCandidatas_380_jh(int idSolicitud, int idUsuario)
        {
            return Leer_380_jh("sp_PN2_UnidadesCandidatas", ParametrosUsuario_380_jh(idUsuario,
                Entero_380_jh("@id_solicitud", idSolicitud)), UnidadDataMapper_380_jh.MapearPublico_380_jh);
        }

        public List<Reserva_380_jh> ListarReservas_380_jh(int? idSolicitudTransfusional, int idUsuario)
        {
            return Leer_380_jh("sp_Reserva_Listar", ParametrosUsuario_380_jh(idUsuario,
                Entero_380_jh("@id_solicitud_transfusional", idSolicitudTransfusional)), MapearReserva_380_jh);
        }

        public CodigoOperacionPN2_380_jh Reservar_380_jh(Reserva_380_jh reserva, int idUsuario)
        {
            int id;
            CodigoOperacionPN2_380_jh resultado = Ejecutar_380_jh("sp_Reserva_Registrar", ParametrosUsuario_380_jh(idUsuario,
                Entero_380_jh("@id_solicitud_transfusional", reserva.IdSolicitudTransfusional_380_jh),
                Entero_380_jh("@id_unidad", reserva.IdUnidad_380_jh),
                Texto_380_jh("@observacion", null, 500),
                Texto_380_jh("@referencia_compatibilidad", reserva.Observacion_380_jh, 450)), out id);
            if (resultado == CodigoOperacionPN2_380_jh.Ok_380_jh)
            {
                reserva.Id_380_jh = id;
                reserva.IdUsuarioResponsable_380_jh = idUsuario;
            }
            return resultado;
        }

        public CodigoOperacionPN2_380_jh CancelarReserva_380_jh(int idReserva, string motivo, int idUsuario)
        {
            int id;
            return Ejecutar_380_jh("sp_Reserva_Cancelar", ParametrosUsuario_380_jh(idUsuario,
                Entero_380_jh("@id_reserva", idReserva), Texto_380_jh("@motivo", motivo, 500)), out id);
        }

        public List<Entrega_380_jh> ListarEntregas_380_jh(int? idSolicitudTransfusional, int idUsuario)
        {
            return Leer_380_jh("sp_Entrega_Listar", ParametrosUsuario_380_jh(idUsuario,
                Entero_380_jh("@id_solicitud_transfusional", idSolicitudTransfusional)), MapearEntrega_380_jh);
        }

        public CodigoOperacionPN2_380_jh Entregar_380_jh(int idReserva, string observacion, int idUsuario)
        {
            int id;
            return Ejecutar_380_jh("sp_Entrega_Registrar", ParametrosUsuario_380_jh(idUsuario,
                Entero_380_jh("@id_reserva", idReserva), Texto_380_jh("@observacion", observacion, 500)), out id);
        }

        public CodigoOperacionPN2_380_jh RegistrarDestino_380_jh(int idEntrega, string destino, string motivo, int idUsuario)
        {
            int id;
            return Ejecutar_380_jh("sp_Entrega_RegistrarDestino", ParametrosUsuario_380_jh(idUsuario,
                Entero_380_jh("@id_entrega", idEntrega), Texto_380_jh("@destino", destino, 20, false),
                Texto_380_jh("@motivo", motivo, 500)), out id);
        }

        public CodigoOperacionPN2_380_jh ReingresarUnidad_380_jh(int idUnidad, string observacion, int idUsuario)
        {
            int id;
            return Ejecutar_380_jh("sp_PN2_Unidad_Reingresar", ParametrosUsuario_380_jh(idUsuario,
                Entero_380_jh("@id_unidad", idUnidad), Texto_380_jh("@observacion", observacion, 500)), out id);
        }

        public List<MovimientoTrazabilidad_380_jh> ListarTrazabilidad_380_jh(TrazabilidadFiltro_380_jh filtro, int idUsuario)
        {
            return Leer_380_jh("sp_PN2_Trazabilidad_Listar", ParametrosFiltro_380_jh(filtro, idUsuario), MapearMovimiento_380_jh);
        }

        public List<FilaReportePN2_380_jh> ListarReporte_380_jh(TrazabilidadFiltro_380_jh filtro, int idUsuario)
        {
            return Leer_380_jh("sp_PN2_Reporte_Listar", ParametrosFiltro_380_jh(filtro, idUsuario), fila => new FilaReportePN2_380_jh
            {
                Seccion_380_jh = Cadena_380_jh(fila, "seccion"), Concepto_380_jh = Cadena_380_jh(fila, "concepto"),
                Cantidad_380_jh = Convert.ToInt32(fila["cantidad"]), Detalle_380_jh = Cadena_380_jh(fila, "detalle")
            });
        }

        private List<T> Leer_380_jh<T>(string procedimiento, List<SqlParameter> parametros, Func<DataRow, T> mapear)
        {
            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.Leer_380_jh(procedimiento, parametros);
                List<T> resultados = new List<T>();
                foreach (DataRow fila in tabla.Rows) resultados.Add(mapear(fila));
                return resultados;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("PN2_READ_FAILED", ex);
            }
            finally { _databaseContext_380_jh.Cerrar_380_jh(); }
        }

        private CodigoOperacionPN2_380_jh Ejecutar_380_jh(string procedimiento, List<SqlParameter> parametros, out int idResultado)
        {
            idResultado = 0;
            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.Leer_380_jh(procedimiento, parametros);
                if (tabla.Rows.Count == 0) return CodigoOperacionPN2_380_jh.Error_380_jh;
                DataRow fila = tabla.Rows[0];
                CodigoOperacionPN2_380_jh resultado = MapearResultado_380_jh(fila["codigo_resultado"].ToString());
                if (resultado == CodigoOperacionPN2_380_jh.Ok_380_jh && fila["id_resultado"] != DBNull.Value)
                    idResultado = Convert.ToInt32(fila["id_resultado"]);
                return resultado;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 51001 || ex.Number == 51002) return CodigoOperacionPN2_380_jh.IntegridadInvalida_380_jh;
                if (ex.Number == 51003) return CodigoOperacionPN2_380_jh.NoAutorizado_380_jh;
                return CodigoOperacionPN2_380_jh.Error_380_jh;
            }
            catch { return CodigoOperacionPN2_380_jh.Error_380_jh; }
            finally { _databaseContext_380_jh.Cerrar_380_jh(); }
        }

        private static CodigoOperacionPN2_380_jh MapearResultado_380_jh(string codigo)
        {
            switch (codigo)
            {
                case "OK": return CodigoOperacionPN2_380_jh.Ok_380_jh;
                case "DATOS_INVALIDOS": return CodigoOperacionPN2_380_jh.DatosInvalidos_380_jh;
                case "NO_AUTORIZADO": return CodigoOperacionPN2_380_jh.NoAutorizado_380_jh;
                case "NO_ENCONTRADO": return CodigoOperacionPN2_380_jh.NoEncontrado_380_jh;
                case "CONFLICTO_ESTADO": return CodigoOperacionPN2_380_jh.ConflictoEstado_380_jh;
                case "SIN_DISPONIBILIDAD": return CodigoOperacionPN2_380_jh.SinDisponibilidad_380_jh;
                case "CANTIDAD_COMPLETA": return CodigoOperacionPN2_380_jh.CantidadCompleta_380_jh;
                case "INTEGRIDAD_INVALIDA": return CodigoOperacionPN2_380_jh.IntegridadInvalida_380_jh;
                default: return CodigoOperacionPN2_380_jh.Error_380_jh;
            }
        }

        private static SolicitudTransfusional_380_jh MapearSolicitud_380_jh(DataRow fila)
        {
            return new SolicitudTransfusional_380_jh
            {
                Id_380_jh = Convert.ToInt32(fila["id_solicitud_transfusional"]),
                PacienteReceptor_380_jh = Cadena_380_jh(fila, "paciente_receptor"),
                DocumentoReceptor_380_jh = Cadena_380_jh(fila, "documento_receptor"),
                GrupoReceptor_380_jh = Cadena_380_jh(fila, "grupo_receptor"),
                RhReceptor_380_jh = Cadena_380_jh(fila, "rh_receptor"),
                LaboratorioEstudio_380_jh = Cadena_380_jh(fila, "laboratorio_estudio"),
                ReferenciaEstudio_380_jh = Cadena_380_jh(fila, "referencia_estudio"),
                FechaEstudio_380_jh = FechaNullable_380_jh(fila, "fecha_estudio"),
                ResultadoAnticuerpos_380_jh = Cadena_380_jh(fila, "resultado_anticuerpos"),
                IdUsuarioValidacion_380_jh = EnteroNullable_380_jh(fila, "id_usuario_validacion"),
                FechaValidacion_380_jh = FechaNullable_380_jh(fila, "fecha_validacion"),
                FechaSolicitud_380_jh = Convert.ToDateTime(fila["fecha_solicitud"]),
                TipoComponente_380_jh = Cadena_380_jh(fila, "tipo_componente"),
                CantidadRequerida_380_jh = Convert.ToInt32(fila["cantidad_requerida"]),
                IdUsuarioSolicitante_380_jh = Convert.ToInt32(fila["id_usuario_solicitante"]),
                ProfesionalSolicitante_380_jh = Cadena_380_jh(fila, "profesional_solicitante"),
                EstadoSolicitud_380_jh = Cadena_380_jh(fila, "estado_solicitud"),
                CantidadReservada_380_jh = Convert.ToInt32(fila["cantidad_reservada"]),
                CantidadEntregada_380_jh = Convert.ToInt32(fila["cantidad_entregada"]),
                SaldoPendiente_380_jh = Convert.ToInt32(fila["saldo_pendiente"])
            };
        }

        private static Reserva_380_jh MapearReserva_380_jh(DataRow fila)
        {
            return new Reserva_380_jh
            {
                Id_380_jh = Convert.ToInt32(fila["id_reserva"]),
                IdSolicitudTransfusional_380_jh = Convert.ToInt32(fila["id_solicitud_transfusional"]),
                IdUnidad_380_jh = Convert.ToInt32(fila["id_unidad"]),
                CodigoIdentificacion_380_jh = Cadena_380_jh(fila, "codigo_identificacion"),
                PacienteReceptor_380_jh = Cadena_380_jh(fila, "paciente_receptor"),
                TipoComponente_380_jh = Cadena_380_jh(fila, "tipo_componente"),
                EstadoReserva_380_jh = Cadena_380_jh(fila, "estado_reserva"),
                FechaReserva_380_jh = Convert.ToDateTime(fila["fecha_reserva"]),
                IdUsuarioResponsable_380_jh = Convert.ToInt32(fila["id_usuario_responsable"]),
                Responsable_380_jh = Cadena_380_jh(fila, "responsable"),
                Observacion_380_jh = Cadena_380_jh(fila, "observacion"),
                FechaCierre_380_jh = FechaNullable_380_jh(fila, "fecha_cierre"),
                MotivoCancelacion_380_jh = Cadena_380_jh(fila, "motivo_cancelacion")
            };
        }

        private static Entrega_380_jh MapearEntrega_380_jh(DataRow fila)
        {
            return new Entrega_380_jh
            {
                Id_380_jh = Convert.ToInt32(fila["id_entrega"]),
                IdReserva_380_jh = Convert.ToInt32(fila["id_reserva"]),
                IdSolicitudTransfusional_380_jh = Convert.ToInt32(fila["id_solicitud_transfusional"]),
                IdUnidad_380_jh = Convert.ToInt32(fila["id_unidad"]),
                CodigoIdentificacion_380_jh = Cadena_380_jh(fila, "codigo_identificacion"),
                PacienteReceptor_380_jh = Cadena_380_jh(fila, "paciente_receptor"),
                TipoComponente_380_jh = Cadena_380_jh(fila, "tipo_componente"),
                FechaEntrega_380_jh = Convert.ToDateTime(fila["fecha_entrega"]),
                IdUsuarioResponsable_380_jh = Convert.ToInt32(fila["id_usuario_responsable"]),
                Responsable_380_jh = Cadena_380_jh(fila, "responsable"),
                Observacion_380_jh = Cadena_380_jh(fila, "observacion"),
                Destino_380_jh = Cadena_380_jh(fila, "destino"),
                FechaDestino_380_jh = FechaNullable_380_jh(fila, "fecha_destino"),
                MotivoDestino_380_jh = Cadena_380_jh(fila, "motivo_destino"),
                IdUsuarioDestino_380_jh = EnteroNullable_380_jh(fila, "id_usuario_destino")
            };
        }

        private static MovimientoTrazabilidad_380_jh MapearMovimiento_380_jh(DataRow fila)
        {
            return new MovimientoTrazabilidad_380_jh
            {
                Id_380_jh = Convert.ToInt32(fila["id_movimiento_unidad"]),
                IdUnidad_380_jh = Convert.ToInt32(fila["id_unidad"]),
                CodigoIdentificacion_380_jh = Cadena_380_jh(fila, "codigo_identificacion"),
                TipoMovimiento_380_jh = Cadena_380_jh(fila, "tipo_movimiento"),
                EstadoAnterior_380_jh = Cadena_380_jh(fila, "estado_anterior"),
                EstadoNuevo_380_jh = Cadena_380_jh(fila, "estado_nuevo"),
                Observacion_380_jh = Cadena_380_jh(fila, "observacion"),
                FechaMovimiento_380_jh = Convert.ToDateTime(fila["fecha_movimiento"]),
                IdUsuario_380_jh = Convert.ToInt32(fila["id_usuario"]),
                Usuario_380_jh = Cadena_380_jh(fila, "usuario"),
                IdSolicitudTransfusional_380_jh = EnteroNullable_380_jh(fila, "id_solicitud_transfusional"),
                IdReserva_380_jh = EnteroNullable_380_jh(fila, "id_reserva"),
                IdEntrega_380_jh = EnteroNullable_380_jh(fila, "id_entrega"),
                TipoComponente_380_jh = Cadena_380_jh(fila, "tipo_componente"),
                GrupoSanguineo_380_jh = Cadena_380_jh(fila, "grupo_sanguineo"),
                FactorRh_380_jh = Cadena_380_jh(fila, "factor_rh")
            };
        }

        private static List<SqlParameter> ParametrosFiltro_380_jh(TrazabilidadFiltro_380_jh filtro, int idUsuario)
        {
            filtro = filtro ?? new TrazabilidadFiltro_380_jh();
            return ParametrosUsuario_380_jh(idUsuario, Entero_380_jh("@id_unidad", filtro.IdUnidad_380_jh),
                Fecha_380_jh("@desde", filtro.Desde_380_jh), Fecha_380_jh("@hasta", filtro.Hasta_380_jh),
                Texto_380_jh("@estado", filtro.Estado_380_jh, 20, false),
                Texto_380_jh("@grupo_sanguineo", filtro.GrupoSanguineo_380_jh, 5, false),
                Texto_380_jh("@tipo_componente", filtro.TipoComponente_380_jh, 100));
        }

        private static List<SqlParameter> ParametrosUsuario_380_jh(int idUsuario, params SqlParameter[] adicionales)
        {
            List<SqlParameter> parametros = new List<SqlParameter>(adicionales);
            parametros.Add(Entero_380_jh("@id_usuario", idUsuario));
            return parametros;
        }

        private static SqlParameter Texto_380_jh(string nombre, string valor, int tamano, bool unicode = true)
        {
            return new SqlParameter(nombre, unicode ? SqlDbType.NVarChar : SqlDbType.VarChar, tamano)
            { Value = string.IsNullOrWhiteSpace(valor) ? (object)DBNull.Value : valor.Trim() };
        }

        private static SqlParameter Entero_380_jh(string nombre, int? valor)
        {
            return new SqlParameter(nombre, SqlDbType.Int) { Value = valor.HasValue ? (object)valor.Value : DBNull.Value };
        }

        private static SqlParameter Fecha_380_jh(string nombre, DateTime? valor)
        {
            return new SqlParameter(nombre, SqlDbType.DateTime) { Value = valor.HasValue ? (object)valor.Value : DBNull.Value };
        }

        private static string Cadena_380_jh(DataRow fila, string columna)
        {
            return fila[columna] == DBNull.Value ? null : fila[columna].ToString();
        }

        private static int? EnteroNullable_380_jh(DataRow fila, string columna)
        {
            return fila[columna] == DBNull.Value ? (int?)null : Convert.ToInt32(fila[columna]);
        }

        private static DateTime? FechaNullable_380_jh(DataRow fila, string columna)
        {
            return fila[columna] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(fila[columna]);
        }
    }
}
