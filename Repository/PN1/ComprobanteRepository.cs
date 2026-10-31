using System.Collections.Generic;
using DAL;
using Domain;

namespace Repository
{
    public class ComprobanteRepository_380_jh
    {
        private readonly ComprobanteDataMapper_380_jh _mapper = new ComprobanteDataMapper_380_jh();
        public bool Disponible_380_jh() { return _mapper.Disponible_380_jh(); }
        public string ObtenerOriginal_380_jh(string proceso, int idReferencia, int idUsuario) { return _mapper.ObtenerOriginal_380_jh(proceso, idReferencia, idUsuario); }
        public List<ComprobantePendiente_380_jh> ListarPendientes_380_jh(int idUsuario) { return _mapper.ListarPendientes_380_jh(idUsuario); }
        public void MarcarGenerado_380_jh(long id, int idUsuario, string ruta) { _mapper.MarcarGenerado_380_jh(id, idUsuario, ruta); }
    }
}
