# Sistema de Gestión de Clínica

Sistema de software orientado a la administración de una clínica médica. Este proyecto forma parte de la materia Ingeniería de Software, con el objetivo de aplicar conceptos de diseño, patrones y documentación técnica.

---

## Objetivo

Desarrollar un sistema que permita gestionar las operaciones principales de una clínica, aplicando buenas prácticas de ingeniería de software.

---

## Funcionalidades

El proceso de banco de sangre PN1 incluye protección DVH/DVV, detección de
inconsistencias y recuperación administrativa.
Ver [instalación, tratamiento de incidentes y pruebas](Database/INTEGRIDAD_PN1.md).


---

## Tecnologías

- Lenguaje: C#
- Base de datos: Sql server

## Configuración de la base de datos

La cadena de conexión se configura para la DAL. Copiá `DAL/App.local.example.config` como `DAL/App.local.config` y ajustá el servidor si hace falta. El archivo local está excluido de Git. `DatabaseContext` de la DAL carga esa configuración al abrir la conexión; la UI no contiene la cadena.

---

