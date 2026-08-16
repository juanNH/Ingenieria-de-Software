using Abstractions;

namespace Domain
{
    public class Traduccion_380_jh : IEntity_380_jh
    {
        public int Id_380_jh { get; set; }
        public int EtiquetaId_380_jh { get; set; }
        public int IdiomaId_380_jh { get; set; }
        public string Texto_380_jh { get; set; }
        public string EtiquetaKey_380_jh { get; set; }
        public string IdiomaCodigo_380_jh { get; set; }

        public AuditoriaMemento_380_jh SaveToMemento_380_jh()
        {
            return new AuditoriaMemento_380_jh("Traduccion", Id_380_jh, new System.Collections.Generic.Dictionary<string, object>
            {
                { "Id", Id_380_jh },
                { "EtiquetaId", EtiquetaId_380_jh },
                { "IdiomaId", IdiomaId_380_jh },
                { "Texto", Texto_380_jh },
                { "EtiquetaKey", EtiquetaKey_380_jh },
                { "IdiomaCodigo", IdiomaCodigo_380_jh }
            });
        }
    }
}
