using System;
using System.Collections.Generic;

namespace Domain
{
    public class Permiso_380_jh : ComponentePermiso_380_jh
    {
        public override TipoComponentePermiso_380_jh Tipo_380_jh
        {
            get { return TipoComponentePermiso_380_jh.Permiso_380_jh; }
        }

        public override void Agregar_380_jh(ComponentePermiso_380_jh componente)
        {
            throw new InvalidOperationException("Un permiso no puede contener otros componentes.");
        }

        public override void Quitar_380_jh(ComponentePermiso_380_jh componente)
        {
            throw new InvalidOperationException("Un permiso no puede contener otros componentes.");
        }

        public override IReadOnlyList<ComponentePermiso_380_jh> ObtenerHijos_380_jh()
        {
            return new List<ComponentePermiso_380_jh>().AsReadOnly();
        }

        public override bool TienePermiso_380_jh(string codigoPermiso)
        {
            return !string.IsNullOrWhiteSpace(codigoPermiso) &&
                   string.Equals(Codigo_380_jh, codigoPermiso, StringComparison.OrdinalIgnoreCase);
        }
    }
}
