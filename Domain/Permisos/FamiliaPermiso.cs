using System;
using System.Collections.Generic;

namespace Domain
{
    public class FamiliaPermiso_380_jh : ComponentePermiso_380_jh
    {
        private readonly List<ComponentePermiso_380_jh> _hijos_380_jh = new List<ComponentePermiso_380_jh>();

        public override TipoComponentePermiso_380_jh Tipo_380_jh
        {
            get { return TipoComponentePermiso_380_jh.Familia_380_jh; }
        }

        public override void Agregar_380_jh(ComponentePermiso_380_jh componente)
        {
            if (componente == null)
            {
                return;
            }

            foreach (ComponentePermiso_380_jh hijo in _hijos_380_jh)
            {
                if (hijo.Id_380_jh == componente.Id_380_jh && componente.Id_380_jh != 0)
                {
                    return;
                }
            }

            if (componente.Id_380_jh == 0)
            {
                _hijos_380_jh.Add(componente);
                return;
            }

            _hijos_380_jh.Add(componente);
        }

        public override void Quitar_380_jh(ComponentePermiso_380_jh componente)
        {
            if (componente == null)
            {
                return;
            }

            _hijos_380_jh.RemoveAll(h => h.Id_380_jh == componente.Id_380_jh);
        }

        public override IReadOnlyList<ComponentePermiso_380_jh> ObtenerHijos_380_jh()
        {
            return _hijos_380_jh.AsReadOnly();
        }

        public override bool TienePermiso_380_jh(string codigoPermiso)
        {
            if (string.IsNullOrWhiteSpace(codigoPermiso))
            {
                return false;
            }

            foreach (ComponentePermiso_380_jh hijo in _hijos_380_jh)
            {
                if (hijo.TienePermiso_380_jh(codigoPermiso))
                {
                    return true;
                }
            }

            return false;
        }

        public bool PuedeAgregar_380_jh(ComponentePermiso_380_jh candidato)
        {
            if (candidato == null)
            {
                return false;
            }

            if (Id_380_jh != 0 && candidato.Id_380_jh == Id_380_jh)
            {
                return false;
            }

            return !Contiene_380_jh(candidato, Id_380_jh);
        }

        private static bool Contiene_380_jh(ComponentePermiso_380_jh componente, int idBuscado)
        {
            if (componente == null || idBuscado == 0)
            {
                return false;
            }

            if (componente.Id_380_jh == idBuscado)
            {
                return true;
            }

            foreach (ComponentePermiso_380_jh hijo in componente.ObtenerHijos_380_jh())
            {
                if (Contiene_380_jh(hijo, idBuscado))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
