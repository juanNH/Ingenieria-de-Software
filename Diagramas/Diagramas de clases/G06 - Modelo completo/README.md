# G06 · Modelo completo de clases por secciones

El [editable G06 - Modelo completo de clases.drawio](G06%20-%20Modelo%20completo%20de%20clases.drawio) contiene **155 tipos: 141 clases, 6 interfaces y 8 enumeraciones**, con **1009 miembros seleccionados y 419 relaciones únicas**.

La primera página, **Clases por capa**, reúne las 155 definiciones UML en contenedores de UI, Application, Repository, DAL, Domain, Services y Abstractions, con sus **121 relaciones internas**. Al mover una sección en draw.io se mueven sus clases y compartimentos. No se tienden cables entre las siete secciones del mapa general.

Las **28 vistas de colaboración** muestran las **419 relaciones explícitas, cada una una vez**, según su clase de origen. Cada fila mantiene sus conexiones locales y repite los destinos como referencias nominales en columnas por capa. Una referencia puede repetirse aunque el tipo esté definido en otra fila de la misma página; no agrega un tipo nuevo. Las 121 relaciones repetidas en el mapa no se suman a las 419 únicas.

Los **1009 miembros son seleccionados de 1831 declarados auditados**; no se muestran todos los auxiliares privados ni los controles del diseñador. Las cuatro referencias externas del CLR (`Form`, `UserControl`, `IDisposable` y `StringWriter`) no se suman a los 155 tipos de la aplicación.

- [Manifiesto y trazabilidad](G06%20-%20Modelo%20completo%20de%20clases.manifest.json).
- [Alcance, convenciones y balance con los modelos parciales](../../../Docs/Modelo-clases-final.md).
- [Índice de todos los diagramas](../../README.md).
- [Modelo anterior sin secciones](../../Historicos/G06%20-%20Modelo%20sin%20secciones/README.md).

El mapa PNG mide **10000 × 5717 píxeles**; los detalles tienen un lado largo de **4000 píxeles**. El tamaño impreso depende de la escala de cada figura: usá zoom en el editable o las imágenes para leer los miembros. Cada imagen conserva una página completa del archivo fuente.

| Página | Vista | Imagen completa |
| --- | --- | --- |
| 1 | Clases por capa | [PNG](Vistas/01%20-%20Clases%20por%20capa.png) |
| 2 | UI · Inicio y soporte | [PNG](Vistas/02%20-%20UI%20-%20Inicio%20y%20soporte.png) |
| 3 | UI · Identidad y permisos | [PNG](Vistas/03%20-%20UI%20-%20Identidad%20y%20permisos.png) |
| 4 | UI · Auditoría e idiomas | [PNG](Vistas/04%20-%20UI%20-%20Auditor%C3%ADa%20e%20idiomas.png) |
| 5 | UI · Donantes, donaciones y trazabilidad | [PNG](Vistas/05%20-%20UI%20-%20Donantes%2C%20donaciones%20y%20trazabilidad.png) |
| 6 | UI · Unidades y controles | [PNG](Vistas/06%20-%20UI%20-%20Unidades%20y%20controles.png) |
| 7 | UI · Operaciones PN2 | [PNG](Vistas/07%20-%20UI%20-%20Operaciones%20PN2.png) |
| 8 | Application · PN2, maestros e integridad | [PNG](Vistas/08%20-%20Application%20-%20PN2%2C%20maestros%20e%20integridad.png) |
| 9 | Application · Identidad y permisos | [PNG](Vistas/09%20-%20Application%20-%20Identidad%20y%20permisos.png) |
| 10 | Application · Auditoría, bitácora e idiomas | [PNG](Vistas/10%20-%20Application%20-%20Auditor%C3%ADa%2C%20bit%C3%A1cora%20e%20idiomas.png) |
| 11 | Application · PN1, comprobantes e intercambio XML | [PNG](Vistas/11%20-%20Application%20-%20PN1%2C%20comprobantes%20e%20intercambio%20XML.png) |
| 12 | Application · Unidades y tabla XML | [PNG](Vistas/12%20-%20Application%20-%20Unidades%20y%20tabla%20XML.png) |
| 13 | Repository · Idiomas, auditoría y comprobantes | [PNG](Vistas/13%20-%20Repository%20-%20Idiomas%2C%20auditor%C3%ADa%20y%20comprobantes.png) |
| 14 | Repository · Donaciones y unidades | [PNG](Vistas/14%20-%20Repository%20-%20Donaciones%20y%20unidades.png) |
| 15 | Repository · Usuarios y permisos | [PNG](Vistas/15%20-%20Repository%20-%20Usuarios%20y%20permisos.png) |
| 16 | Repository · Operaciones PN2 | [PNG](Vistas/16%20-%20Repository%20-%20Operaciones%20PN2.png) |
| 17 | DAL · Contexto, idiomas y auditoría | [PNG](Vistas/17%20-%20DAL%20-%20Contexto%2C%20idiomas%20y%20auditor%C3%ADa.png) |
| 18 | DAL · Donaciones, unidades y maestros | [PNG](Vistas/18%20-%20DAL%20-%20Donaciones%2C%20unidades%20y%20maestros.png) |
| 19 | DAL · Usuarios y verificadores | [PNG](Vistas/19%20-%20DAL%20-%20Usuarios%20y%20verificadores.png) |
| 20 | DAL · Operaciones PN2 | [PNG](Vistas/20%20-%20DAL%20-%20Operaciones%20PN2.png) |
| 21 | Domain · Auditoría y bitácora | [PNG](Vistas/21%20-%20Domain%20-%20Auditor%C3%ADa%20y%20bit%C3%A1cora.png) |
| 22 | Domain · Donación y comprobantes | [PNG](Vistas/22%20-%20Domain%20-%20Donaci%C3%B3n%20y%20comprobantes.png) |
| 23 | Domain · Donante, cantidad e integridad | [PNG](Vistas/23%20-%20Domain%20-%20Donante%2C%20cantidad%20e%20integridad.png) |
| 24 | Domain · Unidades y abastecimiento | [PNG](Vistas/24%20-%20Domain%20-%20Unidades%20y%20abastecimiento.png) |
| 25 | Domain · Solicitudes y trazabilidad | [PNG](Vistas/25%20-%20Domain%20-%20Solicitudes%20y%20trazabilidad.png) |
| 26 | Domain · Usuarios y permisos | [PNG](Vistas/26%20-%20Domain%20-%20Usuarios%20y%20permisos.png) |
| 27 | Domain · Usuario y contraseña | [PNG](Vistas/27%20-%20Domain%20-%20Usuario%20y%20contrase%C3%B1a.png) |
| 28 | Services · Idiomas, PDF y cifrado | [PNG](Vistas/28%20-%20Services%20-%20Idiomas%2C%20PDF%20y%20cifrado.png) |
| 29 | Services y Abstractions · Serialización y contratos | [PNG](Vistas/29%20-%20Services%20y%20Abstractions%20-%20Serializaci%C3%B3n%20y%20contratos.png) |
