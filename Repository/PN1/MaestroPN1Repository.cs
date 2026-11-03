using System.Collections.Generic;
using DAL;
using Domain;

namespace Repository
{
    public class MaestroPN1Repository_380_jh
    {
        private readonly MaestroPN1DataMapper_380_jh _mapper = new MaestroPN1DataMapper_380_jh();
        public List<MaestroPN1_380_jh> Listar_380_jh(string tipo, bool soloActivos) => _mapper.Listar_380_jh(tipo, soloActivos);
        public string Guardar_380_jh(string tipo, MaestroPN1_380_jh item, int idUsuario) => _mapper.Guardar_380_jh(tipo, item, idUsuario);
    }
}
