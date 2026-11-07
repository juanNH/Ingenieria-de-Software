namespace Domain
{
    // El documento identifica al receptor a traves de sus solicitudes persistidas.
    public class Receptor_380_jh
    {
        public string Documento_380_jh { get; set; }
        public string Nombre_380_jh { get; set; }
        public string GrupoSanguineo_380_jh { get; set; }
        public string FactorRh_380_jh { get; set; }

        public override string ToString()
        {
            return string.Format("{0} · {1} · {2}{3}", Nombre_380_jh, Documento_380_jh,
                GrupoSanguineo_380_jh, FactorRh_380_jh).TrimEnd(' ', '·');
        }
    }
}
