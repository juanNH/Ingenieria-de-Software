using System.Collections.Generic;
using DAL;
using Domain;

namespace Repository
{
    public class LanguageRepository_380_jh
    {
        private readonly IdiomaDataMapper_380_jh _idiomaDataMapper_380_jh;

        public LanguageRepository_380_jh()
            : this(new IdiomaDataMapper_380_jh())
        {
        }

        public LanguageRepository_380_jh(IdiomaDataMapper_380_jh idiomaDataMapper)
        {
            _idiomaDataMapper_380_jh = idiomaDataMapper;
        }

        public int Crear_380_jh(Idioma_380_jh idioma, int? idUsuarioResponsable)
        {
            return _idiomaDataMapper_380_jh.Crear_380_jh(idioma, idUsuarioResponsable);
        }

        public bool Actualizar_380_jh(Idioma_380_jh idioma, int? idUsuarioResponsable, string motivo)
        {
            return _idiomaDataMapper_380_jh.Actualizar_380_jh(idioma, idUsuarioResponsable, motivo);
        }

        public Idioma_380_jh ObtenerPorId_380_jh(int id)
        {
            return _idiomaDataMapper_380_jh.ObtenerPorId_380_jh(id);
        }

        public Idioma_380_jh ObtenerDefault_380_jh()
        {
            return _idiomaDataMapper_380_jh.ObtenerDefault_380_jh();
        }

        public List<Idioma_380_jh> Listar_380_jh(bool soloActivos)
        {
            return _idiomaDataMapper_380_jh.Listar_380_jh(soloActivos);
        }
    }
}
