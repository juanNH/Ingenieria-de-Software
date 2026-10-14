using System;
using System.Security.Cryptography;
using System.Text;

namespace Services
{
    /// <summary>
    /// Protects donor email addresses with Windows DPAPI for the current Windows user.
    /// Values are versioned so older clear-text database values can still be read.
    /// </summary>
    public class CifradoEmailService_380_jh
    {
        private const string Prefijo_380_jh = "dpapi:v1:";
        private static readonly byte[] Entropia_380_jh = Encoding.UTF8.GetBytes("HemovidaGest|Donante|Email|v1");

        public string Cifrar_380_jh(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            byte[] protegido = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(valor),
                Entropia_380_jh,
                DataProtectionScope.CurrentUser);
            return Prefijo_380_jh + Convert.ToBase64String(protegido);
        }

        public string Descifrar_380_jh(string valorProtegido)
        {
            if (string.IsNullOrWhiteSpace(valorProtegido))
            {
                return null;
            }

            // Supports donor records created before email encryption was deployed.
            if (!valorProtegido.StartsWith(Prefijo_380_jh, StringComparison.Ordinal))
            {
                return valorProtegido;
            }

            byte[] protegido = Convert.FromBase64String(valorProtegido.Substring(Prefijo_380_jh.Length));
            byte[] valor = ProtectedData.Unprotect(
                protegido,
                Entropia_380_jh,
                DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(valor);
        }

        public bool EstaCifrado_380_jh(string valor)
        {
            return !string.IsNullOrWhiteSpace(valor) && valor.StartsWith(Prefijo_380_jh, StringComparison.Ordinal);
        }
    }
}
