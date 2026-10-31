using System;
using System.Collections.Generic;
using Abstractions;
using Domain;
using Repository;

namespace Application
{
    public class PN2ApplicationService_380_jh
    {
        private readonly PN2Repository_380_jh _repository_380_jh;
        private readonly AutorizacionApplicationService_380_jh _autorizacion_380_jh;
        private readonly BitacoraApplicationService_380_jh _bitacora_380_jh;
        private readonly IntegridadApplicationService_380_jh _integridad_380_jh;

        public PN2ApplicationService_380_jh()
            : this(new PN2Repository_380_jh(), new AutorizacionApplicationService_380_jh(),
                new BitacoraApplicationService_380_jh(), new IntegridadApplicationService_380_jh()) { }

        public PN2ApplicationService_380_jh(PN2Repository_380_jh repository,
            AutorizacionApplicationService_380_jh autorizacion, BitacoraApplicationService_380_jh bitacora,
            IntegridadApplicationService_380_jh integridad)
        {
            _repository_380_jh = repository;
            _autorizacion_380_jh = autorizacion;
            _bitacora_380_jh = bitacora;
            _integridad_380_jh = integridad;
        }

        public List<Unidad_380_jh> ListarInventario_380_jh(string buscar = null, string estado = null)
        {
            Usuario_380_jh usuario = UsuarioActual_380_jh();
            if (!TienePermiso_380_jh(usuario, PermisosSistema_380_jh.InventarioVer_380_jh))
                return new List<Unidad_380_jh>();
            ValidarConsulta_380_jh(TextoValido_380_jh(buscar, 150) && TextoValido_380_jh(estado, 20));
            return _repository_380_jh.ListarInventario_380_jh(Limpiar_380_jh(buscar), Limpiar_380_jh(estado), usuario.Id_380_jh);
        }

        public CodigoOperacionPN2_380_jh ActualizarUbicacion_380_jh(int idUnidad, string ubicacion, string observacion)
        {
            return Ejecutar_380_jh(PermisosSistema_380_jh.InventarioActualizar_380_jh,
                idUnidad > 0 && TextoValido_380_jh(ubicacion, 150, true) && TextoValido_380_jh(observacion, 500, true),
                BitacoraAccion_380_jh.UbicacionActualizada_380_jh, "Ubicación actualizada. Unidad: " + idUnidad + ".",
                idUsuario => _repository_380_jh.ActualizarUbicacion_380_jh(idUnidad, Limpiar_380_jh(ubicacion), Limpiar_380_jh(observacion), idUsuario));
        }

        public List<SolicitudTransfusional_380_jh> ListarSolicitudes_380_jh(string buscar = null)
        {
            Usuario_380_jh usuario = UsuarioActual_380_jh();
            if (!TienePermiso_380_jh(usuario, PermisosSistema_380_jh.SolicitudVer_380_jh))
                return new List<SolicitudTransfusional_380_jh>();
            ValidarConsulta_380_jh(TextoValido_380_jh(buscar, 150));
            // SQL restringe a las solicitudes propias del Profesional solicitante.
            return _repository_380_jh.ListarSolicitudes_380_jh(Limpiar_380_jh(buscar), usuario.Id_380_jh);
        }

        public CodigoOperacionPN2_380_jh RegistrarSolicitud_380_jh(SolicitudTransfusional_380_jh solicitud)
        {
            bool estudioAusente = solicitud != null && string.IsNullOrWhiteSpace(solicitud.GrupoReceptor_380_jh) &&
                string.IsNullOrWhiteSpace(solicitud.RhReceptor_380_jh) && string.IsNullOrWhiteSpace(solicitud.LaboratorioEstudio_380_jh) &&
                string.IsNullOrWhiteSpace(solicitud.ReferenciaEstudio_380_jh) && string.IsNullOrWhiteSpace(solicitud.ResultadoAnticuerpos_380_jh) &&
                !solicitud.FechaEstudio_380_jh.HasValue;
            bool estudioCompleto = solicitud != null &&
                (solicitud.GrupoReceptor_380_jh == "O" || solicitud.GrupoReceptor_380_jh == "A" ||
                 solicitud.GrupoReceptor_380_jh == "B" || solicitud.GrupoReceptor_380_jh == "AB") &&
                (solicitud.RhReceptor_380_jh == "+" || solicitud.RhReceptor_380_jh == "-") &&
                TextoValido_380_jh(solicitud.LaboratorioEstudio_380_jh, 150, true) &&
                TextoValido_380_jh(solicitud.ReferenciaEstudio_380_jh, 150, true) &&
                TextoValido_380_jh(solicitud.ResultadoAnticuerpos_380_jh, 500, true) &&
                solicitud.FechaEstudio_380_jh.HasValue && solicitud.FechaEstudio_380_jh.Value.Date <= DateTime.Today;
            bool valida = solicitud != null && TextoValido_380_jh(solicitud.PacienteReceptor_380_jh, 150, true) &&
                TextoValido_380_jh(solicitud.TipoComponente_380_jh, 100, true) && solicitud.CantidadRequerida_380_jh > 0 &&
                TextoValido_380_jh(solicitud.DocumentoReceptor_380_jh, 50, true) &&
                TextoValido_380_jh(solicitud.ProfesionalSolicitante_380_jh, 150, true) && (estudioAusente || estudioCompleto);
            return Ejecutar_380_jh(PermisosSistema_380_jh.SolicitudCrear_380_jh, valida,
                BitacoraAccion_380_jh.SolicitudTransfusionalRegistrada_380_jh, "Solicitud transfusional registrada.",
                idUsuario =>
                {
                    solicitud.PacienteReceptor_380_jh = Limpiar_380_jh(solicitud.PacienteReceptor_380_jh);
                    solicitud.TipoComponente_380_jh = Limpiar_380_jh(solicitud.TipoComponente_380_jh);
                    return _repository_380_jh.RegistrarSolicitud_380_jh(solicitud, idUsuario);
                });
        }

        public CodigoOperacionPN2_380_jh ActualizarEstudio_380_jh(SolicitudTransfusional_380_jh solicitud)
        {
            bool valida = solicitud != null && solicitud.Id_380_jh > 0 &&
                TextoValido_380_jh(solicitud.DocumentoReceptor_380_jh, 50, true) &&
                TextoValido_380_jh(solicitud.ProfesionalSolicitante_380_jh, 150, true) &&
                (solicitud.GrupoReceptor_380_jh == "O" || solicitud.GrupoReceptor_380_jh == "A" || solicitud.GrupoReceptor_380_jh == "B" || solicitud.GrupoReceptor_380_jh == "AB") &&
                (solicitud.RhReceptor_380_jh == "+" || solicitud.RhReceptor_380_jh == "-") &&
                TextoValido_380_jh(solicitud.LaboratorioEstudio_380_jh, 150, true) &&
                TextoValido_380_jh(solicitud.ReferenciaEstudio_380_jh, 150, true) &&
                TextoValido_380_jh(solicitud.ResultadoAnticuerpos_380_jh, 500, true) &&
                solicitud.FechaEstudio_380_jh.HasValue && solicitud.FechaEstudio_380_jh.Value.Date <= DateTime.Today;
            return Ejecutar_380_jh(PermisosSistema_380_jh.SolicitudCrear_380_jh, valida,
                BitacoraAccion_380_jh.EstudioReceptorActualizado_380_jh, "Estudio del receptor actualizado.",
                idUsuario => _repository_380_jh.ActualizarEstudio_380_jh(solicitud, idUsuario));
        }

        public CodigoOperacionPN2_380_jh ValidarEstudio_380_jh(int idSolicitud)
        {
            return Ejecutar_380_jh(PermisosSistema_380_jh.ReservaCrear_380_jh, idSolicitud > 0,
                BitacoraAccion_380_jh.EstudioReceptorValidado_380_jh, "Estudio del receptor validado. Solicitud: " + idSolicitud + ".",
                idUsuario => _repository_380_jh.ValidarEstudio_380_jh(idSolicitud, idUsuario));
        }

        public List<Unidad_380_jh> ListarCandidatas_380_jh(int idSolicitud)
        {
            Usuario_380_jh usuario = UsuarioActual_380_jh();
            if (!TienePermiso_380_jh(usuario, PermisosSistema_380_jh.InventarioVer_380_jh) ||
                !TienePermiso_380_jh(usuario, PermisosSistema_380_jh.ReservaVer_380_jh)) return new List<Unidad_380_jh>();
            ValidarConsulta_380_jh(idSolicitud > 0);
            return _repository_380_jh.ListarCandidatas_380_jh(idSolicitud, usuario.Id_380_jh);
        }

        public List<Reserva_380_jh> ListarReservas_380_jh(int? idSolicitudTransfusional = null)
        {
            Usuario_380_jh usuario = UsuarioActual_380_jh();
            if (!TienePermiso_380_jh(usuario, PermisosSistema_380_jh.ReservaVer_380_jh)) return new List<Reserva_380_jh>();
            ValidarConsulta_380_jh(!idSolicitudTransfusional.HasValue || idSolicitudTransfusional.Value > 0);
            return _repository_380_jh.ListarReservas_380_jh(idSolicitudTransfusional, usuario.Id_380_jh);
        }

        public CodigoOperacionPN2_380_jh Reservar_380_jh(Reserva_380_jh reserva)
        {
            return Ejecutar_380_jh(PermisosSistema_380_jh.ReservaCrear_380_jh,
                reserva != null && reserva.IdSolicitudTransfusional_380_jh > 0 && reserva.IdUnidad_380_jh > 0 &&
                TextoValido_380_jh(reserva.Observacion_380_jh, 450, true), BitacoraAccion_380_jh.UnidadReservada_380_jh,
                "Reserva registrada. Solicitud: " + (reserva == null ? 0 : reserva.IdSolicitudTransfusional_380_jh) +
                ". Unidad: " + (reserva == null ? 0 : reserva.IdUnidad_380_jh) + ".",
                idUsuario => _repository_380_jh.Reservar_380_jh(reserva, idUsuario));
        }

        public CodigoOperacionPN2_380_jh CancelarReserva_380_jh(int idReserva, string motivo)
        {
            return Ejecutar_380_jh(PermisosSistema_380_jh.ReservaCancelar_380_jh,
                idReserva > 0 && TextoValido_380_jh(motivo, 500, true), BitacoraAccion_380_jh.ReservaCancelada_380_jh,
                "Reserva cancelada: " + idReserva + ".",
                idUsuario => _repository_380_jh.CancelarReserva_380_jh(idReserva, Limpiar_380_jh(motivo), idUsuario));
        }

        public List<Entrega_380_jh> ListarEntregas_380_jh(int? idSolicitudTransfusional = null)
        {
            Usuario_380_jh usuario = UsuarioActual_380_jh();
            if (!TienePermiso_380_jh(usuario, PermisosSistema_380_jh.ReservaVer_380_jh)) return new List<Entrega_380_jh>();
            ValidarConsulta_380_jh(!idSolicitudTransfusional.HasValue || idSolicitudTransfusional.Value > 0);
            return _repository_380_jh.ListarEntregas_380_jh(idSolicitudTransfusional, usuario.Id_380_jh);
        }

        public CodigoOperacionPN2_380_jh Entregar_380_jh(int idReserva, string observacion)
        {
            return Ejecutar_380_jh(PermisosSistema_380_jh.EntregaRegistrar_380_jh,
                idReserva > 0 && TextoValido_380_jh(observacion, 500), BitacoraAccion_380_jh.UnidadEntregada_380_jh,
                "Entrega registrada. Reserva: " + idReserva + ".",
                idUsuario => _repository_380_jh.Entregar_380_jh(idReserva, Limpiar_380_jh(observacion), idUsuario));
        }

        public CodigoOperacionPN2_380_jh RegistrarDestino_380_jh(int idEntrega, string destino, string motivo)
        {
            destino = Limpiar_380_jh(destino);
            if (destino != null) destino = destino.ToUpperInvariant();
            bool transfundida = destino == EstadoUnidadPN2_380_jh.Transfundida_380_jh;
            bool devuelta = destino == EstadoUnidadPN2_380_jh.Devuelta_380_jh;
            bool descartada = destino == EstadoUnidadPN1_380_jh.Descartada_380_jh;
            BitacoraAccion_380_jh accion = transfundida ? BitacoraAccion_380_jh.UnidadTransfundida_380_jh :
                devuelta ? BitacoraAccion_380_jh.UnidadDevuelta_380_jh : BitacoraAccion_380_jh.UnidadDescartada_380_jh;
            return Ejecutar_380_jh(PermisosSistema_380_jh.DestinoRegistrar_380_jh,
                idEntrega > 0 && (transfundida || devuelta || descartada) && TextoValido_380_jh(motivo, 500, !transfundida), accion,
                "Destino registrado. Entrega: " + idEntrega + ". Estado: " + destino + ".",
                idUsuario => _repository_380_jh.RegistrarDestino_380_jh(idEntrega, destino, Limpiar_380_jh(motivo), idUsuario));
        }

        public CodigoOperacionPN2_380_jh ReingresarUnidad_380_jh(int idUnidad, string observacion)
        {
            return Ejecutar_380_jh(PermisosSistema_380_jh.UnidadReingresar_380_jh,
                idUnidad > 0 && TextoValido_380_jh(observacion, 500, true), BitacoraAccion_380_jh.UnidadReingresada_380_jh,
                "Unidad reingresada a revisión: " + idUnidad + ".",
                idUsuario => _repository_380_jh.ReingresarUnidad_380_jh(idUnidad, Limpiar_380_jh(observacion), idUsuario));
        }

        public List<MovimientoTrazabilidad_380_jh> ListarTrazabilidad_380_jh(TrazabilidadFiltro_380_jh filtro)
        {
            Usuario_380_jh usuario = UsuarioActual_380_jh();
            if (!TienePermiso_380_jh(usuario, PermisosSistema_380_jh.TrazabilidadVer_380_jh))
                return new List<MovimientoTrazabilidad_380_jh>();
            ValidarConsulta_380_jh(FiltroValido_380_jh(filtro));
            return _repository_380_jh.ListarTrazabilidad_380_jh(filtro, usuario.Id_380_jh);
        }

        public List<FilaReportePN2_380_jh> ListarReporte_380_jh(TrazabilidadFiltro_380_jh filtro)
        {
            Usuario_380_jh usuario = UsuarioActual_380_jh();
            if (!TienePermiso_380_jh(usuario, PermisosSistema_380_jh.ReporteVer_380_jh)) return new List<FilaReportePN2_380_jh>();
            ValidarConsulta_380_jh(FiltroValido_380_jh(filtro));
            return _repository_380_jh.ListarReporte_380_jh(filtro, usuario.Id_380_jh);
        }

        private CodigoOperacionPN2_380_jh Ejecutar_380_jh(string permiso, bool datosValidos,
            BitacoraAccion_380_jh accion, string descripcion, Func<int, CodigoOperacionPN2_380_jh> operacion)
        {
            Usuario_380_jh usuario = UsuarioActual_380_jh();
            if (!TienePermiso_380_jh(usuario, permiso))
            {
                RegistrarFalla_380_jh(usuario, "Operación PN2 no autorizada: " + permiso + ".");
                return CodigoOperacionPN2_380_jh.NoAutorizado_380_jh;
            }
            if (!datosValidos)
            {
                RegistrarFalla_380_jh(usuario, "Operación PN2 rechazada por datos inválidos: " + permiso + ".");
                return CodigoOperacionPN2_380_jh.DatosInvalidos_380_jh;
            }
            InformeIntegridad_380_jh informe = _integridad_380_jh.Verificar_380_jh();
            if (!informe.Disponible_380_jh || !informe.EsValida_380_jh)
            {
                RegistrarFalla_380_jh(usuario, "Operación PN2 bloqueada por integridad.");
                return CodigoOperacionPN2_380_jh.IntegridadInvalida_380_jh;
            }
            CodigoOperacionPN2_380_jh resultado = operacion(usuario.Id_380_jh);
            if (resultado == CodigoOperacionPN2_380_jh.Ok_380_jh)
                _bitacora_380_jh.RegistrarEvento_380_jh(BitacoraModulo_380_jh.BancoSangre_380_jh, accion,
                    BitacoraNivel_380_jh.Informacion_380_jh, usuario, descripcion);
            else
                RegistrarFalla_380_jh(usuario, "Operación PN2 rechazada: " + permiso + ". Resultado: " + resultado + ".");
            if (resultado == CodigoOperacionPN2_380_jh.IntegridadInvalida_380_jh) _integridad_380_jh.Verificar_380_jh();
            return resultado;
        }

        private void RegistrarFalla_380_jh(Usuario_380_jh usuario, string descripcion)
        {
            _bitacora_380_jh.RegistrarEvento_380_jh(BitacoraModulo_380_jh.BancoSangre_380_jh,
                BitacoraAccion_380_jh.OperacionFallida_380_jh, BitacoraNivel_380_jh.Advertencia_380_jh, usuario, descripcion);
        }

        private bool TienePermiso_380_jh(Usuario_380_jh usuario, string permiso)
        {
            return _autorizacion_380_jh.TienePermiso_380_jh(usuario, permiso);
        }

        private static Usuario_380_jh UsuarioActual_380_jh()
        {
            return Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
        }

        private static bool TextoValido_380_jh(string valor, int maximo, bool requerido = false)
        {
            return string.IsNullOrWhiteSpace(valor) ? !requerido : valor.Trim().Length <= maximo;
        }

        private static string Limpiar_380_jh(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static bool FiltroValido_380_jh(TrazabilidadFiltro_380_jh filtro)
        {
            if (filtro == null) return true;
            DateTime fechaMinima = new DateTime(1753, 1, 1);
            return (!filtro.IdUnidad_380_jh.HasValue || filtro.IdUnidad_380_jh.Value > 0) &&
                (!filtro.Desde_380_jh.HasValue || filtro.Desde_380_jh.Value >= fechaMinima) &&
                (!filtro.Hasta_380_jh.HasValue || filtro.Hasta_380_jh.Value >= fechaMinima) &&
                (!filtro.Desde_380_jh.HasValue || !filtro.Hasta_380_jh.HasValue || filtro.Desde_380_jh.Value <= filtro.Hasta_380_jh.Value) &&
                TextoValido_380_jh(filtro.Estado_380_jh, 20) && TextoValido_380_jh(filtro.GrupoSanguineo_380_jh, 5) &&
                TextoValido_380_jh(filtro.TipoComponente_380_jh, 100);
        }

        private static void ValidarConsulta_380_jh(bool valida)
        {
            if (!valida) throw new ArgumentException("PN2_INVALID_FILTER");
        }
    }
}
