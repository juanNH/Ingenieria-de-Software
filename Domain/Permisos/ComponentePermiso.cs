using System.Collections.Generic;
using Abstractions;

namespace Domain
{
    public abstract class ComponentePermiso_380_jh : IEntity_380_jh
    {
        public int Id_380_jh { get; set; }
        public string Codigo_380_jh { get; set; }
        public string Nombre_380_jh { get; set; }
        public string Descripcion_380_jh { get; set; }
        public string Estado_380_jh { get; set; }

        public abstract TipoComponentePermiso_380_jh Tipo_380_jh { get; }

        public abstract void Agregar_380_jh(ComponentePermiso_380_jh componente);
        public abstract void Quitar_380_jh(ComponentePermiso_380_jh componente);
        public abstract IReadOnlyList<ComponentePermiso_380_jh> ObtenerHijos_380_jh();
        public abstract bool TienePermiso_380_jh(string codigoPermiso);

        public AuditoriaMemento_380_jh SaveToMemento_380_jh()
        {
            return new AuditoriaMemento_380_jh("ComponentePermiso", Id_380_jh, new Dictionary<string, object>
            {
                { "Id", Id_380_jh },
                { "Codigo", Codigo_380_jh },
                { "Nombre", Nombre_380_jh },
                { "Descripcion", Descripcion_380_jh },
                { "Estado", Estado_380_jh },
                { "Tipo", Tipo_380_jh.ToString() }
            });
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Nombre_380_jh) ? Codigo_380_jh : Nombre_380_jh;
        }
    }
}
