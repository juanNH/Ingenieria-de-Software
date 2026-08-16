using Abstractions;

namespace Domain
{
    public class Etiqueta_380_jh : IEntity_380_jh
    {
        public int Id_380_jh { get; set; }
        public string Key_380_jh { get; set; }
        public string Descripcion_380_jh { get; set; }

        public AuditoriaMemento_380_jh SaveToMemento_380_jh()
        {
            return new AuditoriaMemento_380_jh("Etiqueta", Id_380_jh, new System.Collections.Generic.Dictionary<string, object>
            {
                { "Id", Id_380_jh },
                { "Key", Key_380_jh },
                { "Descripcion", Descripcion_380_jh }
            });
        }

        public override string ToString()
        {
            return Key_380_jh ?? string.Empty;
        }
    }
}
