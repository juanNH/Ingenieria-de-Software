using System;

namespace Domain
{
    public class Donante_380_jh
    {
        public int Id_380_jh { get; set; }
        public string Documento_380_jh { get; set; }
        public string Nombre_380_jh { get; set; }
        public string Apellido_380_jh { get; set; }
        public DateTime? FechaNacimiento_380_jh { get; set; }
        public string Telefono_380_jh { get; set; }
        public string Email_380_jh { get; set; }
        public string Domicilio_380_jh { get; set; }
        public string Estado_380_jh { get; set; }
        public DateTime FechaAlta_380_jh { get; set; }
        public int IdUsuarioAlta_380_jh { get; set; }

        public string NombreCompleto_380_jh
        {
            get { return (Nombre_380_jh + " " + Apellido_380_jh).Trim(); }
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Documento_380_jh)
                ? NombreCompleto_380_jh
                : Documento_380_jh + " - " + NombreCompleto_380_jh;
        }
    }
}
