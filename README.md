# HemovidaGest · Gestión de banco de sangre

Sistema de software para registrar donaciones y unidades de sangre y atender solicitudes transfusionales. Este proyecto forma parte de la materia Ingeniería de Software, con el objetivo de aplicar conceptos de diseño, patrones y documentación técnica.

---

## Objetivo

Gestionar el registro, la clasificación y la disponibilidad de unidades de sangre, así como su reserva, entrega y destino, con trazabilidad y controles de integridad.

---

## Funcionalidades

El proceso de banco de sangre PN1 incluye protección DVH/DVV, detección de
inconsistencias y recuperación administrativa.
Ver [instalación, tratamiento de incidentes y pruebas](Database/INTEGRIDAD_PN1.md).

PN2 incluye solicitudes transfusionales, reservas, entregas parciales y registro del
destino de las unidades. Ver [flujo, diagramas draw.io, instalación y recorrido de
comprobación funcional](Docs/PN2.md).

El menú **Consultas** contiene una única pantalla: [Abastecimiento y riesgo de
vencimiento](Docs/ReporteInteligente.md), habilitada por `REPORTE_VER`. Relaciona
stock vigente, demanda registrada, saldos y vencimientos por componente, grupo y Rh.

Los [CDU del sistema](Docs/CDU.md) tienen sus diagramas de clases y secuencia
normalizados y contrastados con la implementación. El [modelo final G06](Docs/Modelo-clases-final.md)
reúne las 155 clases, interfaces y enumeraciones, con una vista global y 28 detalles sin agrupaciones por capa. El [modelo final G07](Docs/Modelo-datos-final.md)
reúne el esquema físico completo en diagramas nativos de SSMS. La
[arquitectura y persistencia T01](Docs/Arquitectura.md) documenta los componentes,
la navegación y el almacenamiento actuales.

El [índice de diagramas](Diagramas/README.md) organiza las fuentes por tipo y permite acceder al [editable G06 y sus 29 vistas PNG](Diagramas/Diagramas%20de%20clases/G06%20-%20Modelo%20completo/README.md), incluido el modelo global en PNG de alta resolución y SVG.

El menú **Maestros** permite administrar componentes, grupos sanguíneos, factor Rh y destinos de entrega.
Para actualizar una base existente, ejecutar [InstalarMaestrosPN1.ps1](Database/InstalarMaestrosPN1.ps1); la migración conserva los códigos históricos y crea un respaldo verificado.
El [orden de ejecución de los scripts SQL](Database/README.md) distingue instalaciones nuevas y actualizaciones PN1/PN2.


---

## Tecnologías

- Lenguaje: C#
- Base de datos: Sql server

## Configuración de la base de datos

La cadena de conexión se configura para la DAL. Copiá `DAL/App.local.example.config` como `DAL/App.local.config` y ajustá el servidor si hace falta. El archivo local está excluido de Git. `DatabaseContext` de la DAL carga esa configuración al abrir la conexión; la UI no contiene la cadena.

---

