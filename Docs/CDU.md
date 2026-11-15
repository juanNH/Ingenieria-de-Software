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

Los diagramas de clases comparten paquetes UI, Application, Repository, DAL, Domain y Services. Cada caja muestra las firmas relevantes al caso; los colaboradores de integridad, permisos, idioma, sesión y auditoría aparecen como clases e interfaces con sus relaciones UML. Las firmas y los participantes de las secuencias se contrastaron con el código.

| Modelo de clases | Clases e interfaces | Relaciones |
| --- | ---: | ---: |
| CDU-01 | 41 | 61 |
| CDU-02 | 27 | 39 |
| CDU-03 | 12 | 12 |
| CDU-04 | 15 | 22 |
| CDU-05 | 27 | 42 |
| CDU-06 | 29 | 55 |
| CDU-07 | 27 | 53 |
| T04 · Roles y permisos, soporte de CDU-05 | 17 | 24 |

La vista complementaria [T04 · Roles y permisos](../Diagramas/Diagramas%20de%20clases/CDU-05%20-%20Edici%C3%B3n%20de%20usuario%20-%20Clases%20-%20T04%20Roles%20y%20permisos%20%28soporte%29.drawio) representa `RolesView`: crear familias y administrar sus relaciones. La vista principal de CDU-05 representa `UsuariosView` y la asignación de un rol a una cuenta.

## Artefactos

Las 20 fuentes principales draw.io comprenden cinco diagramas de casos de uso, siete de clases y ocho de secuencia. Se conserva además la fuente complementaria de clases T04 · Roles y permisos: 21 archivos editables en total. Sus páginas generan las vistas necesarias para los siete casos. El documento incluye 36 figuras principales:

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

La verificación cubre las siete fichas de ocho filas, numeración y referencias de alternativas, 36 figuras completas con sus pies, siete DER Standard y concordancia de las 21 fuentes publicadas. Los ocho modelos de clases se revisan por firmas, tipos, relaciones UML y correspondencia con las secuencias. El documento se exporta a PDF para comprobar la paginación y la lectura de PN2 y los CDU.
