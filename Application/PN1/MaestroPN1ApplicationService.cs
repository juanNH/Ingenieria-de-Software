using System;
using System.Collections.Generic;
using Domain;
using Repository;

namespace Application
{
    public class MaestroPN1ApplicationService_380_jh
    {
        public const string Componente_380_jh = "COMPONENTE";
        public const string Grupo_380_jh = "GRUPO";
        public const string Rh_380_jh = "RH";
        public const string Destino_380_jh = "DESTINO";
        private readonly MaestroPN1Repository_380_jh _repository = new MaestroPN1Repository_380_jh();

        public List<ReglaCompatibilidad_380_jh> ListarCompatibilidades_380_jh()
        {
            var usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            if (usuario == null)
                throw new UnauthorizedAccessException();
            return _repository.ListarCompatibilidades_380_jh(usuario.Id_380_jh);
        }

        public string GuardarCompatibilidad_380_jh(ReglaCompatibilidad_380_jh regla)
        {
            var usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            if (usuario == null)
                return "NO_AUTORIZADO";
            if (regla == null || regla.Id_380_jh < 0 || !CodigoValido_380_jh(regla.TipoComponente_380_jh, 100) ||
                !CodigoValido_380_jh(regla.GrupoUnidad_380_jh, 5) || !CodigoValido_380_jh(regla.RhUnidad_380_jh, 5) ||
                !CodigoValido_380_jh(regla.GrupoReceptor_380_jh, 5) || !CodigoValido_380_jh(regla.RhReceptor_380_jh, 5))
                return "DATOS_INVALIDOS";
            if (!new IntegridadApplicationService_380_jh().Verificar_380_jh().EsValida_380_jh) return "INTEGRIDAD_INVALIDA";
            return _repository.GuardarCompatibilidad_380_jh(regla, usuario.Id_380_jh);
        }

        private static bool CodigoValido_380_jh(string codigo, int longitud) => !string.IsNullOrWhiteSpace(codigo) && codigo.Length <= longitud;

        public List<MaestroPN1_380_jh> Listar_380_jh(string tipo, bool soloActivos = true)
        {
            if (tipo != Componente_380_jh && tipo != Grupo_380_jh && tipo != Rh_380_jh && tipo != Destino_380_jh) throw new ArgumentException("Tipo de maestro inválido.");
            return _repository.Listar_380_jh(tipo, soloActivos);
        }

        public string Guardar_380_jh(string tipo, MaestroPN1_380_jh item)
        {
            var usuario = Sesion_380_jh.ObtenerInstancia_380_jh().ObtenerUsuario_380_jh();
            if (usuario == null) return "NO_AUTORIZADO";
            if ((tipo != Componente_380_jh && tipo != Grupo_380_jh && tipo != Rh_380_jh && tipo != Destino_380_jh) || item == null ||
                string.IsNullOrWhiteSpace(item.Codigo_380_jh) || string.IsNullOrWhiteSpace(item.Descripcion_380_jh)) return "DATOS_INVALIDOS";
            item.Codigo_380_jh = item.Codigo_380_jh.Trim();
            if (tipo == Grupo_380_jh || tipo == Destino_380_jh) item.Codigo_380_jh = item.Codigo_380_jh.ToUpperInvariant();
            item.Descripcion_380_jh = item.Descripcion_380_jh.Trim();
            if (item.Codigo_380_jh.Length > (tipo == Componente_380_jh ? 100 : tipo == Destino_380_jh ? 20 : 5) || item.Descripcion_380_jh.Length > 150) return "DATOS_INVALIDOS";
            if (tipo == Destino_380_jh && (item.Codigo_380_jh == "SIN_DESTINO" ||
                ((item.Codigo_380_jh == "DEVUELTA" || item.Codigo_380_jh == "DESCARTADA") && !item.MotivoObligatorio_380_jh))) return "DATOS_INVALIDOS";
            if (!new IntegridadApplicationService_380_jh().Verificar_380_jh().EsValida_380_jh) return "INTEGRIDAD_INVALIDA";
            return _repository.Guardar_380_jh(tipo, item, usuario.Id_380_jh);
        }
    }
}
