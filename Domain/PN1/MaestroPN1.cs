namespace Domain
{
    public class MaestroPN1_380_jh
    {
        public int Id_380_jh { get; set; }
        public string Codigo_380_jh { get; set; }
        public string Descripcion_380_jh { get; set; }
        public bool Activo_380_jh { get; set; }
        public bool MotivoObligatorio_380_jh { get; set; }
        public string CodigoDescripcion_380_jh => Codigo_380_jh == Descripcion_380_jh ? Codigo_380_jh : Codigo_380_jh + " · " + Descripcion_380_jh;
        public override string ToString() => Descripcion_380_jh;
    }
}
