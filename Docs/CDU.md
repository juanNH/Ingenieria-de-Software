# CDU de estructura del sistema

CDU-01 a CDU-07 normalizados y alineados con la implementación vigente. La especificación completa está en el [Trabajo de Diploma](https://docs.google.com/document/d/1EUrkuuLmCq6YnOGA_lSNvpeQSwCAK-Xd34jLIo4du58/edit?tab=t.0).

| Código | Caso de uso | Actor principal |
| --- | --- | --- |
| CDU-01 | Iniciar sesión | Usuario registrado |
| CDU-02 | Registrarse | Usuario no registrado |
| CDU-03 | Cerrar sesión | Usuario autenticado |
| CDU-04 | Consultar bitácora | Usuario autorizado |
| CDU-05 | Gestionar usuarios | Administrador |
| CDU-06 | Gestionar idiomas y traducciones | Administrador |
| CDU-07 | Consultar y restaurar cambios | Usuario autorizado |

Cada ficha tiene ocho filas: nombre, objetivo, actor principal, actor secundario, precondiciones, postcondiciones, flujo principal y alternativas. El flujo principal usa numeración nativa y las alternativas remiten al paso correspondiente. Los actores representan responsabilidades funcionales; UI, servicios y base de datos se describen como componentes internos.

Por caso se mantiene la correspondencia entre ficha, casos de uso, clases, secuencia, DER y pantalla. Las vistas de soporte quedan diferenciadas del recorrido principal. CDU-05 muestra modificar e inhabilitar; CDU-06, guardar traducción y administrar idioma; CDU-07, consultar y restaurar.

## Artefactos

Las 20 fuentes draw.io comprenden cinco diagramas de casos de uso, siete de clases y ocho de secuencia. Sus páginas generan las vistas necesarias para los siete casos. La entrega incluye 36 figuras:

| Tipo | Cantidad |
| --- | ---: |
| Casos de uso | 5 |
| Clases | 7 |
| Secuencias | 10 |
| DER de SSMS | 7 |
| Pantallas | 7 |

T02 comparte un diagrama de casos de uso entre CDU-01, CDU-02 y CDU-03. Las fuentes editables y las imágenes completas se encuentran en el [Drive de la entrega](https://drive.google.com/drive/folders/1sDKMvSKL4r6z-f9lsa3skDga-wkOR7HT). Cada figura del documento enlaza su fuente o imagen completa.

## DER nativos e instalación

Los siete DER se guardaron con el diseñador de SQL Server Management Studio en vista **Standard**, con columnas, tipos, nulabilidad, claves y relaciones físicas. Las definiciones binarias nativas se exportan en [14_DiagramasCDUSSMS.sql](../Database/StoredProcedures/14_DiagramasCDUSSMS.sql); las capturas EMF y las referencias están en [Diagramas de entidad-relación](../Diagramas/Diagramas%20Entidad%20Relacion/LEEME.md).

La instalación nueva mediante [InstalarBase.ps1](../Installer/InstalarBase.ps1) y la actualización mediante [InstalarPN2.ps1](../Database/InstalarPN2.ps1) incluyen el soporte de SSMS y las siete definiciones. El paquete de Inno Setup incorpora esos scripts. También están disponibles el [SQL nativo en Drive](https://drive.google.com/file/d/1rLIp0yW6KC1CiUeF7nkPtY_d5KnkQbso/view) y su [manifiesto](https://drive.google.com/file/d/1yB1Bto4ZzDtCFiOAB1JxLmIxJ7E-ANol/view).

La verificación final cubre las siete fichas de ocho filas, numeración y referencias de alternativas, 36 figuras completas con sus pies, siete DER Standard y concordancia de las 20 fuentes publicadas. El documento se exporta a PDF para comprobar la paginación y la lectura de PN2 y los CDU.
