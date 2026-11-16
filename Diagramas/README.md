# Diagramas de HemovidaGest

Los diagramas están agrupados por su tipo. Los archivos conservan los códigos de proceso y caso de uso para relacionarlos con las fichas del documento.

## Modelos completos

- **[G06 · Modelo completo de clases](Diagramas%20de%20clases/G06%20-%20Modelo%20completo/README.md):** editable draw.io, manifiesto y 29 vistas PNG. El mapa general agrupa los 155 tipos y sus 1009 miembros seleccionados en siete secciones por capa, con 121 relaciones internas. Las 28 vistas de colaboración muestran explícitamente las 419 relaciones únicas, con referencias próximas a cada clase de origen.
- **[G07 · Modelo de datos completo](Diagramas%20Entidad%20Relacion/G07%20-%20Modelo%20de%20datos%20completo.png):** imagen general de las 32 tablas. Sus tres vistas de detalle, exportaciones EMF y definiciones nativas `.ssms.bin` están en la misma carpeta. [Cómo abrir y editar en SSMS](../Docs/Modelo-datos-final.md).

## Carpetas por tipo

| Carpeta | Contenido |
| --- | --- |
| [Diagramas de clases](Diagramas%20de%20clases/) | Clases parciales de CDU y CDUN, modelos complementarios y el modelo completo G06. |
| [Diagramas de Secuencia](Diagramas%20de%20Secuencia/) | Secuencias de clases de los casos de uso y operaciones complementarias. |
| [Casos de Uso](Casos%20de%20Uso/) | Casos de uso de negocio y del sistema. |
| [Diagramas Entidad Relacion](Diagramas%20Entidad%20Relacion/LEEME.md) | DER vigentes de SQL Server Management Studio, con vista Standard, y modelos finales G07. |
| [Diagramas de actividad](Diagramas%20de%20actividad/) | Procesos de negocio PN1 y PN2. |
| [Entrada Comportamiento Salida](Entrada%20Comportamiento%20Salida/) | Entradas, actividades y salidas de PN1 y PN2. |
| [Modelos conceptuales](Modelos%20conceptuales/) | Conceptos y relaciones de negocio de PN1 y PN2. |
| [Roles](Roles/) | Responsabilidades de los participantes de PN1 y PN2. |
| [Secuencia de Roles](Secuencia%20de%20Roles/) | Interacciones entre los roles de negocio. |
| [Arquitectura](Arquitectura/) | Componentes, navegación y esquema de persistencia de T01. |
| [Seguridad](Seguridad/) | Hash y cifrado. |
| [Historicos](Historicos/README.md) | Antecedentes conservados, identificados como ajenos al modelo vigente. |

Los códigos `CDUN-01` a `CDUN-04` corresponden a PN1; `CDUN-05` a `CDUN-08`, a PN2. Los códigos `CDU-01` a `CDU-07` corresponden a las funciones de estructura del sistema. Los archivos de bitácora y cantidad se ubican junto a los diagramas de su mismo tipo.

## Fuentes y copias

Los `.drawio` son las fuentes editables. G06 reúne sus 29 páginas en un solo editable y ofrece las imágenes completas en su subcarpeta `Vistas/`. Los DER se editan desde **Database Diagrams** en SSMS; sus copias y respaldos están en `Diagramas Entidad Relacion/`.

Los scripts que instalan los diagramas nativos con la base permanecen en `Database/StoredProcedures/`, junto con el resto de las migraciones. Las capturas de pantallas de la aplicación permanecen en `Docs/Pantallas/`. `output/` contiene evidencias y exportaciones de trabajo; el modelo completo de clases ya tiene aquí sus copias canónicas.
