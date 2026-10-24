using System.Collections.Generic;
using DAL;
using Domain;

namespace Repository
{
    public class PN2Repository_380_jh
    {
        private readonly PN2DataMapper_380_jh _mapper_380_jh;

        public PN2Repository_380_jh() : this(new PN2DataMapper_380_jh()) { }

        public PN2Repository_380_jh(PN2DataMapper_380_jh mapper)
        {
            _mapper_380_jh = mapper;
        }

        public List<Unidad_380_jh> ListarInventario_380_jh(string buscar, string estado, int idUsuario)
        {
            return _mapper_380_jh.ListarInventario_380_jh(buscar, estado, idUsuario);
        }

        public CodigoOperacionPN2_380_jh ActualizarUbicacion_380_jh(int idUnidad, string ubicacion, string observacion, int idUsuario)
        {
            return _mapper_380_jh.ActualizarUbicacion_380_jh(idUnidad, ubicacion, observacion, idUsuario);
        }

        public List<SolicitudTransfusional_380_jh> ListarSolicitudes_380_jh(string buscar, int idUsuario)
        {
            return _mapper_380_jh.ListarSolicitudes_380_jh(buscar, idUsuario);
        }

        public CodigoOperacionPN2_380_jh RegistrarSolicitud_380_jh(SolicitudTransfusional_380_jh solicitud, int idUsuario)
        {
            return _mapper_380_jh.RegistrarSolicitud_380_jh(solicitud, idUsuario);
        }

        public List<Reserva_380_jh> ListarReservas_380_jh(int? idSolicitudTransfusional, int idUsuario)
        {
            return _mapper_380_jh.ListarReservas_380_jh(idSolicitudTransfusional, idUsuario);
        }

        public CodigoOperacionPN2_380_jh Reservar_380_jh(Reserva_380_jh reserva, int idUsuario)
        {
            return _mapper_380_jh.Reservar_380_jh(reserva, idUsuario);
        }

        public CodigoOperacionPN2_380_jh CancelarReserva_380_jh(int idReserva, string motivo, int idUsuario)
        {
            return _mapper_380_jh.CancelarReserva_380_jh(idReserva, motivo, idUsuario);
        }

        public List<Entrega_380_jh> ListarEntregas_380_jh(int? idSolicitudTransfusional, int idUsuario)
        {
            return _mapper_380_jh.ListarEntregas_380_jh(idSolicitudTransfusional, idUsuario);
        }

        public CodigoOperacionPN2_380_jh Entregar_380_jh(int idReserva, string observacion, int idUsuario)
        {
            return _mapper_380_jh.Entregar_380_jh(idReserva, observacion, idUsuario);
        }

        public CodigoOperacionPN2_380_jh RegistrarDestino_380_jh(int idEntrega, string destino, string motivo, int idUsuario)
        {
            return _mapper_380_jh.RegistrarDestino_380_jh(idEntrega, destino, motivo, idUsuario);
        }

        public CodigoOperacionPN2_380_jh ReingresarUnidad_380_jh(int idUnidad, string observacion, int idUsuario)
        {
            return _mapper_380_jh.ReingresarUnidad_380_jh(idUnidad, observacion, idUsuario);
        }

        public List<MovimientoTrazabilidad_380_jh> ListarTrazabilidad_380_jh(TrazabilidadFiltro_380_jh filtro, int idUsuario)
        {
            return _mapper_380_jh.ListarTrazabilidad_380_jh(filtro, idUsuario);
        }

        public List<FilaReportePN2_380_jh> ListarReporte_380_jh(TrazabilidadFiltro_380_jh filtro, int idUsuario)
        {
            return _mapper_380_jh.ListarReporte_380_jh(filtro, idUsuario);
        }
    }
}
