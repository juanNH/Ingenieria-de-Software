using Abstractions;
using System.Collections.Generic;

namespace Domain
{
    public class Usuario_380_jh : IEntity_380_jh
    {
        public int Id_380_jh { get; set; }
        public string Username_380_jh { get; set; }
        public string Email_380_jh { get; set; }
        public string Password_380_jh { get; set; }
        public string Nombre_380_jh { get; set; }
        public string Apellido_380_jh { get; set; }
        public string Idioma_380_jh { get; set; }
        public int? IdiomaPreferidoId_380_jh { get; set; }
        public string Estado_380_jh { get; set; }
        public int IntentosLoginFallidos_380_jh { get; set; }
        public bool BloqueoDigitoVerificador_380_jh { get; set; }
        public string Dvh_380_jh { get; set; }
        public List<ComponentePermiso_380_jh> ComponentesPermiso_380_jh { get; set; } = new List<ComponentePermiso_380_jh>();

        public AuditoriaMemento_380_jh CrearMemento_380_jh()
        {
            return SaveToMemento_380_jh();
        }

        public AuditoriaMemento_380_jh SaveToMemento_380_jh()
        {
            return new AuditoriaMemento_380_jh("Usuario", Id_380_jh, new Dictionary<string, object>
            {
                { "Id", Id_380_jh },
                { "Username", Username_380_jh },
                { "Email", Email_380_jh },
                { "Nombre", Nombre_380_jh },
                { "Apellido", Apellido_380_jh },
                { "Idioma", Idioma_380_jh },
                { "IdiomaPreferidoId", IdiomaPreferidoId_380_jh },
                { "Estado", Estado_380_jh },
                { "IntentosLoginFallidos", IntentosLoginFallidos_380_jh },
                { "BloqueoDigitoVerificador", BloqueoDigitoVerificador_380_jh }
            });
        }

        public void RestoreFromMemento_380_jh(AuditoriaMemento_380_jh memento)
        {
            if (memento == null || memento.Entidad_380_jh != "Usuario")
            {
                return;
            }

            IReadOnlyDictionary<string, object> estado = memento.GetSavedMemento_380_jh();
            Id_380_jh = ObtenerValor_380_jh<int>(estado, "Id", Id_380_jh);
            Username_380_jh = ObtenerValor_380_jh<string>(estado, "Username", Username_380_jh);
            Email_380_jh = ObtenerValor_380_jh<string>(estado, "Email", Email_380_jh);
            Nombre_380_jh = ObtenerValor_380_jh<string>(estado, "Nombre", Nombre_380_jh);
            Apellido_380_jh = ObtenerValor_380_jh<string>(estado, "Apellido", Apellido_380_jh);
            Idioma_380_jh = ObtenerValor_380_jh<string>(estado, "Idioma", Idioma_380_jh);
            IdiomaPreferidoId_380_jh = ObtenerValor_380_jh<int?>(estado, "IdiomaPreferidoId", IdiomaPreferidoId_380_jh);
            Estado_380_jh = ObtenerValor_380_jh<string>(estado, "Estado", Estado_380_jh);
            IntentosLoginFallidos_380_jh = ObtenerValor_380_jh<int>(estado, "IntentosLoginFallidos", IntentosLoginFallidos_380_jh);
            BloqueoDigitoVerificador_380_jh = ObtenerValor_380_jh<bool>(estado, "BloqueoDigitoVerificador", BloqueoDigitoVerificador_380_jh);
        }

        public bool TienePermiso_380_jh(string codigoPermiso)
        {
            if (ComponentesPermiso_380_jh == null || string.IsNullOrWhiteSpace(codigoPermiso))
            {
                return false;
            }

            foreach (ComponentePermiso_380_jh componente in ComponentesPermiso_380_jh)
            {
                if (componente.TienePermiso_380_jh(codigoPermiso))
                {
                    return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            string nombreCompleto = $"{Nombre_380_jh} {Apellido_380_jh}".Trim();

            if (!string.IsNullOrWhiteSpace(nombreCompleto))
            {
                return nombreCompleto;
            }

            if (!string.IsNullOrWhiteSpace(Username_380_jh))
            {
                return Username_380_jh;
            }

            return Email_380_jh ?? string.Empty;
        }

        private static T ObtenerValor_380_jh<T>(IReadOnlyDictionary<string, object> estado, string campo, T valorActual)
        {
            if (estado == null || !estado.ContainsKey(campo) || estado[campo] == null)
            {
                return valorActual;
            }

            if (estado[campo] is T)
            {
                return (T)estado[campo];
            }

            return valorActual;
        }
    }
}
