namespace Domain
{
    using System;

    public static class ComponentesTransfusionales_380_jh
    {
        public const string Eritrocitos_380_jh = "Concentrado de eritrocitos";
        public const string Plasma_380_jh = "Plasma fresco congelado";
        public const string Plaquetas_380_jh = "Concentrado de plaquetas";

        public static string[] CrearOpciones_380_jh()
        {
            return new[] { Eritrocitos_380_jh, Plasma_380_jh, Plaquetas_380_jh };
        }

        public static bool EsValido_380_jh(string valor)
        {
            return string.Equals(valor, Eritrocitos_380_jh, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(valor, Plasma_380_jh, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(valor, Plaquetas_380_jh, StringComparison.OrdinalIgnoreCase);
        }
    }
}
