using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Domain;

namespace DAL
{
    public class PermisoDataMapper_380_jh
    {
        private readonly DatabaseContext_380_jh _databaseContext_380_jh;

        public PermisoDataMapper_380_jh()
            : this(new DatabaseContext_380_jh())
        {
        }

        public PermisoDataMapper_380_jh(DatabaseContext_380_jh databaseContext)
        {
            _databaseContext_380_jh = databaseContext;
        }

        public List<ComponentePermiso_380_jh> ListarAsignadosPorUsuario_380_jh(int idUsuario)
        {
            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                Dictionary<int, ComponentePermiso_380_jh> componentes = CargarComponentes_380_jh();
                CargarRelaciones_380_jh(componentes);

                List<int> idsAsignados = CargarIdsAsignados_380_jh(idUsuario);
                List<ComponentePermiso_380_jh> asignados = new List<ComponentePermiso_380_jh>();
                foreach (int idAsignado in idsAsignados)
                {
                    if (componentes.ContainsKey(idAsignado))
                    {
                        asignados.Add(componentes[idAsignado]);
                    }
                }

                return asignados;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public List<ComponentePermiso_380_jh> ListarArbolCompleto_380_jh()
        {
            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                Dictionary<int, ComponentePermiso_380_jh> componentes = CargarComponentes_380_jh();
                HashSet<int> idsHijos = CargarRelaciones_380_jh(componentes);
                List<ComponentePermiso_380_jh> raices = new List<ComponentePermiso_380_jh>();

                foreach (ComponentePermiso_380_jh componente in componentes.Values)
                {
                    if (!idsHijos.Contains(componente.Id_380_jh))
                    {
                        raices.Add(componente);
                    }
                }

                OrdenarPorNombre_380_jh(raices);
                return raices;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public List<ComponentePermiso_380_jh> ListarComponentes_380_jh()
        {
            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                List<ComponentePermiso_380_jh> componentes = new List<ComponentePermiso_380_jh>(CargarComponentes_380_jh().Values);
                componentes.Sort(CompararPorTipoYNombre_380_jh);
                return componentes;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public List<ComponentePermiso_380_jh> ListarFamilias_380_jh()
        {
            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                List<ComponentePermiso_380_jh> familias = new List<ComponentePermiso_380_jh>(CargarComponentesPorTipo_380_jh("FAMILIA").Values);
                OrdenarPorNombre_380_jh(familias);
                return familias;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public List<int> ListarIdsComponentesAsignadosPorUsuario_380_jh(int idUsuario)
        {
            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                return CargarIdsAsignados_380_jh(idUsuario);
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public bool CrearFamilia_380_jh(string codigo, string nombre, string descripcion)
        {
            const string sql = @"
                IF EXISTS (
                    SELECT 1
                    FROM dbo.ComponentePermiso
                    WHERE codigo = @codigo
                      AND tipo <> 'FAMILIA'
                )
                BEGIN
                    SELECT CAST(0 AS INT) AS resultado;
                    RETURN;
                END;

                IF EXISTS (
                    SELECT 1
                    FROM dbo.ComponentePermiso
                    WHERE codigo = @codigo
                      AND tipo = 'FAMILIA'
                )
                BEGIN
                    UPDATE dbo.ComponentePermiso
                    SET nombre = @nombre,
                        descripcion = @descripcion,
                        estado_componente = 'ACTIVO'
                    WHERE codigo = @codigo
                      AND tipo = 'FAMILIA';
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.ComponentePermiso
                    (
                        codigo,
                        nombre,
                        descripcion,
                        tipo,
                        estado_componente
                    )
                    VALUES
                    (
                        @codigo,
                        @nombre,
                        @descripcion,
                        'FAMILIA',
                        'ACTIVO'
                    );
                END;

                SELECT CAST(1 AS INT) AS resultado;";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@codigo", codigo),
                _databaseContext_380_jh.CrearParametro_380_jh("@nombre", nombre),
                _databaseContext_380_jh.CrearParametro_380_jh("@descripcion", descripcion)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);
                return tabla.Rows.Count > 0 && Convert.ToInt32(tabla.Rows[0]["resultado"]) == 1;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public string AgregarRelacion_380_jh(int idPadre, int idHijo)
        {
            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_padre", idPadre),
                _databaseContext_380_jh.CrearParametro_380_jh("@id_hijo", idHijo)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                DataTable tabla = _databaseContext_380_jh.Leer_380_jh("sp_ComponentePermiso_AgregarRelacion", parametros);
                return tabla.Rows.Count == 0 ? "ERROR" : tabla.Rows[0]["codigo_resultado"].ToString();
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public bool QuitarRelacion_380_jh(int idPadre, int idHijo)
        {
            const string sql = @"
                DELETE FROM dbo.ComponentePermisoRelacion
                WHERE id_padre = @id_padre
                  AND id_hijo = @id_hijo";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_padre", idPadre),
                _databaseContext_380_jh.CrearParametro_380_jh("@id_hijo", idHijo)
            };

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                return _databaseContext_380_jh.EscribirTexto_380_jh(sql, parametros) >= 0;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public bool GuardarComponentesUsuario_380_jh(int idUsuario, List<int> idsComponentes)
        {
            const string desactivarSql = @"
                UPDATE dbo.UsuarioComponentePermiso
                SET estado_usuario_componente = 'INACTIVO'
                WHERE id_usuario = @id_usuario";

            const string guardarSql = @"
                IF EXISTS (
                    SELECT 1
                    FROM dbo.UsuarioComponentePermiso
                    WHERE id_usuario = @id_usuario
                      AND id_componente = @id_componente
                )
                BEGIN
                    UPDATE dbo.UsuarioComponentePermiso
                    SET estado_usuario_componente = 'ACTIVO'
                    WHERE id_usuario = @id_usuario
                      AND id_componente = @id_componente;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.UsuarioComponentePermiso
                    (
                        id_usuario,
                        id_componente,
                        estado_usuario_componente
                    )
                    VALUES
                    (
                        @id_usuario,
                        @id_componente,
                        'ACTIVO'
                    );
                END";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                _databaseContext_380_jh.IniciarTx_380_jh();

                List<SqlParameter> parametrosUsuario = new List<SqlParameter>
                {
                    _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", idUsuario)
                };

                if (_databaseContext_380_jh.EscribirTexto_380_jh(desactivarSql, parametrosUsuario) < 0)
                {
                    _databaseContext_380_jh.Deshacer_380_jh();
                    return false;
                }

                foreach (int idComponente in idsComponentes ?? new List<int>())
                {
                    List<SqlParameter> parametros = new List<SqlParameter>
                    {
                        _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", idUsuario),
                        _databaseContext_380_jh.CrearParametro_380_jh("@id_componente", idComponente)
                    };

                    if (_databaseContext_380_jh.EscribirTexto_380_jh(guardarSql, parametros) < 0)
                    {
                        _databaseContext_380_jh.Deshacer_380_jh();
                        return false;
                    }
                }

                _databaseContext_380_jh.Confirmar_380_jh();
                return true;
            }
            catch
            {
                _databaseContext_380_jh.Deshacer_380_jh();
                return false;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        public bool PuedeAgregarRelacion_380_jh(int idPadre, int idHijo)
        {
            if (idPadre == idHijo)
            {
                return false;
            }

            const string sql = @"
                ;WITH Descendientes AS
                (
                    SELECT id_hijo
                    FROM dbo.ComponentePermisoRelacion
                    WHERE id_padre = @id_hijo

                    UNION ALL

                    SELECT r.id_hijo
                    FROM dbo.ComponentePermisoRelacion r
                    INNER JOIN Descendientes d
                        ON d.id_hijo = r.id_padre
                )
                SELECT TOP (1) id_hijo
                FROM Descendientes
                WHERE id_hijo = @id_padre";

            try
            {
                _databaseContext_380_jh.Abrir_380_jh();
                List<System.Data.SqlClient.SqlParameter> parametros = new List<System.Data.SqlClient.SqlParameter>
                {
                    _databaseContext_380_jh.CrearParametro_380_jh("@id_padre", idPadre),
                    _databaseContext_380_jh.CrearParametro_380_jh("@id_hijo", idHijo)
                };

                return _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros).Rows.Count == 0;
            }
            finally
            {
                _databaseContext_380_jh.Cerrar_380_jh();
            }
        }

        private Dictionary<int, ComponentePermiso_380_jh> CargarComponentes_380_jh()
        {
            const string sql = @"
                SELECT
                    id_componente,
                    codigo,
                    nombre,
                    descripcion,
                    tipo,
                    estado_componente
                FROM dbo.ComponentePermiso
                WHERE UPPER(estado_componente) = 'ACTIVO'
                ORDER BY nombre";

            DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql);
            Dictionary<int, ComponentePermiso_380_jh> componentes = new Dictionary<int, ComponentePermiso_380_jh>();

            foreach (DataRow fila in tabla.Rows)
            {
                ComponentePermiso_380_jh componente = CrearComponente_380_jh(fila);
                componentes[componente.Id_380_jh] = componente;
            }

            return componentes;
        }

        private Dictionary<int, ComponentePermiso_380_jh> CargarComponentesPorTipo_380_jh(string tipo)
        {
            const string sql = @"
                SELECT
                    id_componente,
                    codigo,
                    nombre,
                    descripcion,
                    tipo,
                    estado_componente
                FROM dbo.ComponentePermiso
                WHERE UPPER(estado_componente) = 'ACTIVO'
                  AND tipo = @tipo
                ORDER BY nombre";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@tipo", tipo)
            };

            DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);
            Dictionary<int, ComponentePermiso_380_jh> componentes = new Dictionary<int, ComponentePermiso_380_jh>();

            foreach (DataRow fila in tabla.Rows)
            {
                ComponentePermiso_380_jh componente = CrearComponente_380_jh(fila);
                componentes[componente.Id_380_jh] = componente;
            }

            return componentes;
        }

        private HashSet<int> CargarRelaciones_380_jh(Dictionary<int, ComponentePermiso_380_jh> componentes)
        {
            const string sql = @"
                SELECT r.id_padre, r.id_hijo
                FROM dbo.ComponentePermisoRelacion r
                INNER JOIN dbo.ComponentePermiso padre
                    ON padre.id_componente = r.id_padre
                INNER JOIN dbo.ComponentePermiso hijo
                    ON hijo.id_componente = r.id_hijo
                WHERE UPPER(padre.estado_componente) = 'ACTIVO'
                  AND UPPER(hijo.estado_componente) = 'ACTIVO'";

            DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql);
            HashSet<int> idsHijos = new HashSet<int>();

            foreach (DataRow fila in tabla.Rows)
            {
                int idPadre = Convert.ToInt32(fila["id_padre"]);
                int idHijo = Convert.ToInt32(fila["id_hijo"]);

                if (!componentes.ContainsKey(idPadre) || !componentes.ContainsKey(idHijo))
                {
                    continue;
                }

                FamiliaPermiso_380_jh padre = componentes[idPadre] as FamiliaPermiso_380_jh;
                if (padre == null)
                {
                    continue;
                }

                padre.Agregar_380_jh(componentes[idHijo]);
                idsHijos.Add(idHijo);
            }

            return idsHijos;
        }

        private List<int> CargarIdsAsignados_380_jh(int idUsuario)
        {
            const string sql = @"
                SELECT uc.id_componente
                FROM dbo.UsuarioComponentePermiso uc
                INNER JOIN dbo.ComponentePermiso c
                    ON c.id_componente = uc.id_componente
                WHERE uc.id_usuario = @id_usuario
                  AND UPPER(uc.estado_usuario_componente) = 'ACTIVO'
                  AND UPPER(c.estado_componente) = 'ACTIVO'
                ORDER BY c.nombre";

            List<System.Data.SqlClient.SqlParameter> parametros = new List<System.Data.SqlClient.SqlParameter>
            {
                _databaseContext_380_jh.CrearParametro_380_jh("@id_usuario", idUsuario)
            };

            DataTable tabla = _databaseContext_380_jh.LeerTexto_380_jh(sql, parametros);
            List<int> ids = new List<int>();

            foreach (DataRow fila in tabla.Rows)
            {
                ids.Add(Convert.ToInt32(fila["id_componente"]));
            }

            return ids;
        }

        private static ComponentePermiso_380_jh CrearComponente_380_jh(DataRow fila)
        {
            string tipo = fila["tipo"].ToString();
            ComponentePermiso_380_jh componente = string.Equals(tipo, "FAMILIA", StringComparison.OrdinalIgnoreCase)
                ? (ComponentePermiso_380_jh)new FamiliaPermiso_380_jh()
                : new Permiso_380_jh();

            componente.Id_380_jh = Convert.ToInt32(fila["id_componente"]);
            componente.Codigo_380_jh = fila["codigo"].ToString();
            componente.Nombre_380_jh = fila["nombre"].ToString();
            componente.Descripcion_380_jh = fila["descripcion"] == DBNull.Value ? null : fila["descripcion"].ToString();
            componente.Estado_380_jh = fila["estado_componente"].ToString();

            return componente;
        }

        private static void OrdenarPorNombre_380_jh(List<ComponentePermiso_380_jh> componentes)
        {
            componentes.Sort(delegate (ComponentePermiso_380_jh primero, ComponentePermiso_380_jh segundo)
            {
                return string.Compare(primero.Nombre_380_jh, segundo.Nombre_380_jh, StringComparison.OrdinalIgnoreCase);
            });
        }

        private static int CompararPorTipoYNombre_380_jh(ComponentePermiso_380_jh primero, ComponentePermiso_380_jh segundo)
        {
            int comparacionTipo = primero.Tipo_380_jh.CompareTo(segundo.Tipo_380_jh);
            if (comparacionTipo != 0)
            {
                return comparacionTipo;
            }

            return string.Compare(primero.Nombre_380_jh, segundo.Nombre_380_jh, StringComparison.OrdinalIgnoreCase);
        }
    }
}
