using Abstractions;

namespace Domain
{
    public class Idioma_380_jh : IEntity_380_jh
    {
        public int Id_380_jh { get; set; }
        public string Codigo_380_jh { get; set; }
        public string Nombre_380_jh { get; set; }
        public bool Activo_380_jh { get; set; }

        public AuditoriaMemento_380_jh SaveToMemento_380_jh()
        {
            return new AuditoriaMemento_380_jh("Idioma", Id_380_jh, new System.Collections.Generic.Dictionary<string, object>
            {
                { "Id", Id_380_jh },
                { "Codigo", Codigo_380_jh },
                { "Nombre", Nombre_380_jh },
                { "Activo", Activo_380_jh }
            });
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Nombre_380_jh) ? Codigo_380_jh : Nombre_380_jh;
        }
    }
}
