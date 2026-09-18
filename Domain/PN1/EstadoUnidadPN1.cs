using System;

namespace Domain
{
    public static class EstadoUnidadPN1_380_jh
    {
        public const string EnRevision_380_jh = "EN_REVISION";
        public const string Liberada_380_jh = "LIBERADA";
        public const string Bloqueada_380_jh = "BLOQUEADA";
        public const string Descartada_380_jh = "DESCARTADA";

        public static bool EsEstadoFinal_380_jh(string estado)
        {
            return string.Equals(estado, Liberada_380_jh, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(estado, Bloqueada_380_jh, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(estado, Descartada_380_jh, StringComparison.OrdinalIgnoreCase);
        }
    }
}
