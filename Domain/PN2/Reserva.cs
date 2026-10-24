using System;

namespace Domain
{
    public class Reserva_380_jh
    {
        public int Id_380_jh { get; set; }
        public int IdSolicitudTransfusional_380_jh { get; set; }
        public int IdUnidad_380_jh { get; set; }
        public string CodigoIdentificacion_380_jh { get; set; }
        public string PacienteReceptor_380_jh { get; set; }
        public string TipoComponente_380_jh { get; set; }
        public string EstadoReserva_380_jh { get; set; }
        public DateTime FechaReserva_380_jh { get; set; }
        public int IdUsuarioResponsable_380_jh { get; set; }
        public string Responsable_380_jh { get; set; }
        public string Observacion_380_jh { get; set; }
        public DateTime? FechaCierre_380_jh { get; set; }
        public string MotivoCancelacion_380_jh { get; set; }
    }
}
