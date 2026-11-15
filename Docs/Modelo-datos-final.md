# Modelo de datos final — G07

El modelo físico completo de `HemovidaGest` contiene **32 tablas de aplicación, 221 columnas, 32 claves primarias y 53 claves foráneas**, en el esquema `dbo`. También conserva 22 restricciones de unicidad, 24 restricciones `CHECK` y 36 valores predeterminados. Los objetos de soporte del diseñador de SSMS, como `sysdiagrams`, quedan fuera de este alcance funcional.

Los cuatro diagramas se abrieron, distribuyeron y guardaron con **SQL Server Management Studio**, con todas las tablas en vista **Standard**. Cada tabla muestra el nombre de sus columnas, el tipo de datos, la admisión de nulos y la marca de clave primaria; las líneas corresponden a las claves foráneas reales. Las imágenes provienen de **Copy Diagram to Clipboard** de SSMS y se conservan también como EMF vectorial, sin recortar el diagrama.

| Diagrama nativo | Tablas | Columnas | FK | Imagen completa | Vector |
| --- | ---: | ---: | ---: | --- | --- |
| G07 - Modelo de datos completo | 32 | 221 | 53 | [PNG](../Diagramas/Diagramas%20Entidad%20Relacion/G07%20-%20Modelo%20de%20datos%20completo.png) | [EMF](../Diagramas/Diagramas%20Entidad%20Relacion/G07%20-%20Modelo%20de%20datos%20completo.emf) |
| G07 - Seguridad, idiomas y auditoria | 16 | 93 | 15 | [PNG](../Diagramas/Diagramas%20Entidad%20Relacion/G07%20-%20Seguridad,%20idiomas%20y%20auditoria.png) | [EMF](../Diagramas/Diagramas%20Entidad%20Relacion/G07%20-%20Seguridad,%20idiomas%20y%20auditoria.emf) |
| G07 - Operaciones PN1 y PN2 | 14 | 129 | 33 | [PNG](../Diagramas/Diagramas%20Entidad%20Relacion/G07%20-%20Operaciones%20PN1%20y%20PN2.png) | [EMF](../Diagramas/Diagramas%20Entidad%20Relacion/G07%20-%20Operaciones%20PN1%20y%20PN2.emf) |
| G07 - Catalogos y compatibilidad | 8 | 30 | 6 | [PNG](../Diagramas/Diagramas%20Entidad%20Relacion/G07%20-%20Catalogos%20y%20compatibilidad.png) | [EMF](../Diagramas/Diagramas%20Entidad%20Relacion/G07%20-%20Catalogos%20y%20compatibilidad.emf) |

El general reúne todo el esquema. Las tres vistas de detalle repiten tablas de frontera —por ejemplo `Usuario` y los catálogos— para mantener las relaciones visibles. La unión de esos detalles cubre las mismas 32 tablas y 53 FK del general; sus cantidades no se suman porque hay elementos repetidos.

## Alcance del esquema

- **Operaciones:** `Donante`, `Donacion`, `Unidad`, `MovimientoUnidad`, `SolicitudTransfusional`, `Reserva` y `Entrega`.
- **Catálogos y compatibilidad:** `ComponenteTransfusional`, `GrupoSanguineo`, `FactorRh`, `EstadoUnidad`, `DestinoEntrega` y `CompatibilidadTransfusional`.
- **Usuarios y permisos:** `Usuario`, `ComponentePermiso`, `ComponentePermisoRelacion` y `UsuarioComponentePermiso`; también `Rol`, `Permiso`, `UsuarioRol` y `RolPermiso`, que siguen persistidos y participan en las rutas de compatibilidad de autorización.
- **Idiomas:** `Idioma`, `IdiomaEstadoHistorial`, `Etiqueta` y `Traduccion`.
- **Registro e integridad:** `Bitacora`, `Auditoria`, `DigitoVerificadorVertical`, `IntegridadIncidente`, `MaestrosPN1Control` y `CompatibilidadTransfusionalControl`.
- **Comprobantes:** `ComprobantePendiente`, con su instantánea XML y la ruta del PDF generado.

Los datos de identidad, tipificación y estudio del receptor se guardan en `SolicitudTransfusional`; no existe una tabla independiente `Paciente` o `Receptor`. `Auditoria` conserva cambios de usuarios, operaciones de PN1/PN2, maestros y compatibilidades.

`DigitoVerificadorVertical`, `IntegridadIncidente`, `MaestrosPN1Control` y `CompatibilidadTransfusionalControl` no tienen FK declaradas. Se incluyen igualmente en el general y en los detalles correspondientes. Las referencias genéricas, como `Auditoria.id_entidad` o `ComprobantePendiente.id_referencia`, no se convierten en relaciones inventadas.

## Instalación y edición

[16_DiagramasFinalesSSMS.sql](../Database/StoredProcedures/16_DiagramasFinalesSSMS.sql) incorpora los cuatro diagramas nativos al instalar la base. La definición de cada uno es una exportación exacta de `dbo.sysdiagrams`, identificada por su versión y SHA-256 en el [manifiesto](../Database/StoredProcedures/16_DiagramasFinalesSSMS.manifest.json). Se conservan también los cuatro archivos `.ssms.bin` junto a las imágenes como respaldo de esos mismos bytes.

Para editarlos, abrí la base en SSMS, expandí **Database Diagrams** y elegí el diagrama G07 correspondiente. El script restaura las definiciones guardadas; la instalación no necesita abrir SSMS ni generar imágenes. Reaplicarlo crea los diagramas que falten y actualiza solamente las definiciones G07 cuyo contenido difiera, conservando los otros diagramas.

Antes de restaurar el diseño, el script compara las tablas, las columnas y las FK de la base destino con el alcance exportado. La comparación de columnas utiliza sus nombres y propiedades, sin exigir el mismo orden físico que puede dejar una migración histórica. Si cambia el esquema, corresponde actualizar y guardar el modelo en SSMS y volver a exportarlo.

La herramienta [Exportar-DiagramasFinalesSSMS.ps1](../Database/Tools/Exportar-DiagramasFinalesSSMS.ps1) consulta únicamente metadatos mediante servidor y base explícitos, sin leer `App.local.config`. Verifica las tablas, las relaciones y el modo Standard antes de generar el SQL y el manifiesto. [Exportar-ImagenSSMS.ps1](../Database/Tools/Exportar-ImagenSSMS.ps1) conserva el EMF nativo copiado por SSMS y genera su PNG proporcional.

## Verificación de la entrega

Se comprobó la cobertura exacta de tablas y FK del general y de cada detalle, la presencia de todos los nombres de columna en los EMF y la ausencia de solapamientos entre rectángulos de tablas. Los 17 diagramas anteriores conservaron nombre, versión y hash. La copia temporal utilizada para preparar el general se retiró después de verificar el canónico y exportar su definición recuperable.

El orden de instalación y las opciones para una base existente se describen en [Database/README.md](../Database/README.md). La distinción entre persistencia SQL, intercambio XML temporal y archivos PDF se explica en [Persistencia.md](Persistencia.md).
