# Integridad — implementación inicial para PN1

La pantalla, los permisos y los mensajes pertenecen al módulo transversal
**Integridad**. Este documento describe el primer proveedor implementado, PN1;
PN2 deberá agregar sus tablas y verificaciones al mismo módulo, sin crear otra
entrada de navegación.

## Base existente: HemovidaGest

No volver a ejecutar tecni_salud_schema.sql sobre una base existente.
No ejecutar I18n_Reset_Es_En.sql para esta actualización: ese script elimina y
recrea el catálogo de idiomas. La migración nueva agrega las traducciones sin
borrar las personalizadas.

1. Detener todas las instancias de la aplicación y respaldar la base actual.
2. Ejecutar **StoredProcedures/IntegridadPN1.sql** sobre HemovidaGest.
   Agrega las columnas, verificadores, historial protegido, registro de
   incidentes, permisos y traducciones que falten. No borra filas PN1 ni acepta
   automáticamente su contenido.
3. Ejecutar **StoredProcedures/PN1.sql**. Todos los procedimientos de escritura
   quedan protegidos por la verificación transaccional.
4. Compilar la solución y volver a iniciar sesión como administrador para cargar
   los permisos nuevos.
5. Abrir **Integridad**, revisar el informe y contrastar los datos iniciales
   con una fuente confiable. Conservar un backup externo.
6. Elegir **Aceptar base inicial**. Confirmar solamente si
   los datos revisados son correctos. Luego debe aparecer la verificación correcta.

La aceptación inicial calcula los dígitos sobre los datos existentes y guarda
sus primeras versiones; no puede determinar si ya estaban mal antes de instalar
la funcionalidad. No es una opción de reparación ni de recálculo general.
Después de la aceptación, no permite reinicializar; si falta parcialmente la
base de verificadores, se mantiene el bloqueo.

La instalación fue probada con SQL Server 2022. Utiliza CREATE OR ALTER,
FOR JSON, OPENJSON y HASHBYTES con SHA2_256; necesita compatibilidad
de base 130 o superior.

## Instalación nueva

Ejecutar, en orden:

1. tecni_salud_schema.sql.
2. StoredProcedures/AuthUsuario.sql.
3. StoredProcedures/Bitacora.sql.
4. StoredProcedures/Auditoria.sql.
5. StoredProcedures/Permisos.sql.
6. StoredProcedures/IntegridadPN1.sql.
7. StoredProcedures/PN1.sql.

Después, iniciar sesión y revisar Integridad. Si existen datos de negocio
previos, aceptar explícitamente la base inicial luego de validarlos. Una base
nueva sin datos de negocio se considera válida y puede comenzar a operar sin
aceptar una base vacía: la primera escritura crea el historial y los DVH/DVV.
El esquema base, el sembrado de permisos y el catálogo ES/EN incluyen los
nuevos permisos y textos. El reset opcional de idiomas también los conserva
en su catálogo de creación, pero sigue siendo un reset destructivo.

## Cobertura y arquitectura

| Tabla | Protección |
| --- | --- |
| Donante | DVH de todos sus campos persistidos y DVV de la tabla |
| Donacion | DVH de todos sus campos persistidos y DVV de la tabla |
| Unidad | DVH de todos sus campos persistidos y DVV de la tabla |
| MovimientoUnidad | DVH de todos sus campos persistidos y DVV de la tabla |
| Auditoria (solo entidades PN1) | DVH del evento completo y DVV separado AuditoriaPN1 |

El DVH excluye el propio dígito e incluye el ID. La representación canónica
es JSON Unicode con columnas explícitas y nulos incluidos; no depende de la
cultura de Windows ni concatena campos con separadores ambiguos.
El DVV usa IDs y dígitos ordenados, y detecta inserciones/eliminaciones.
También se compara cada fila con su última versión auditada.

Se mantiene la separación UI → Application → Repository → DAL → SQL.
SQL es la única implementación de la representación canónica y los hashes:
evita que C# y SQL difieran en fechas, nulos o codificación.

Cada escritura PN1:

1. Valida permiso y datos desde Application.
2. Verifica integridad y notifica el modo de trabajo a la UI.
3. Dentro de una transacción SQL, bloquea las tablas y verifica nuevamente.
4. Revalida las condiciones del negocio dentro de esa misma transacción.
5. Guarda datos, movimientos, versiones auditadas y dígitos.
6. Confirma conjuntamente; un error revierte el conjunto.

Los eventos habituales conservan el registro de bitácora de los servicios
existentes. Inicialización y recuperación registran su bitácora dentro de la
misma transacción SQL. Los intentos fallidos desde la aplicación también se
registran. Si la base está inaccesible, no puede garantizarse escribir una
bitácora en ella; la verificación falla de forma cerrada y deja una traza técnica.

El helper de sellado tiene DENY EXECUTE TO public; se utiliza internamente
por la cadena de propiedad de los procedimientos. No hay botón ni API
administrativa para recalcular libremente datos alterados.
No ejecutar la aplicación con sysadmin/db_owner en un entorno operativo:
esos privilegios pueden eludir los controles SQL.
El procedimiento de restauración ejecuta como propietario para reconstruir
identidades sin otorgar ALTER sobre las tablas al login de la aplicación.
Los permisos funcionales del actor se comprueban dentro del procedimiento.

Los bloqueos exclusivos priorizan la consistencia. Esta implementación
verifica el conjunto PN1 completo: su costo aumenta con los datos y limita la
concurrencia; hay que medirlo antes de usarla con volúmenes grandes.

## Qué ve cada usuario

- Se verifica al abrir la sesión, abrir/actualizar pantallas PN1 y antes de
  escribir. No es un monitor continuo de modificaciones hechas fuera de la app.
- Una barra persistente muestra la última verificación; **Verificar / actualizar**
  permite comprobar nuevamente en cualquier momento, incluso después de una
  reparación realizada por otra sesión.
- Una inconsistencia o la imposibilidad de verificar bloquea las escrituras de
  todo PN1 y marca el campo de bloqueo de los usuarios no administradores.
  Esos usuarios no pueden iniciar sesión mientras el estado sea inválido;
  Administrador conserva el acceso a Integridad para reparar. Se mantienen
  las consultas, señaladas como no confiables.
- Una verificación correcta libera los bloqueos operativos y vuelve a permitir
  el acceso de los usuarios no administradores. El bloqueo se sincroniza con
  la verificación; no se debe editar manualmente.
- Seguridad, roles, traducciones y herramientas administrativas siguen
  disponibles. No se cambia el estado operativo de las unidades para representar
  problemas técnicos de integridad.
- Por defecto, solo Administrador recibe INTEGRIDAD_VER e
  INTEGRIDAD_RESTAURAR. La recuperación exige ambos permisos y los vuelve a
  comprobar en SQL.
- **Integridad** muestra entidad, ID, tipo de error, fecha y JSON
  actual/última versión. Las versiones mostradas no son confiables si el informe
  indica que el historial está alterado.
- **Auditoría de cambios** permite elegir las cuatro entidades PN1. Su historial
  es de consulta: la restauración de campos de usuarios no se reutiliza para
  deshacer arbitrariamente datos de negocio.

## Recuperar un incidente

1. Detener la operación de negocio, conservar el informe y respaldar el estado
   averiado para investigación. No editar los dígitos manualmente.
2. Como administrador, verificar y revisar los registros afectados.
3. Si el historial está íntegro y todos los datos afectados tienen una versión,
   elegir **Restaurar desde historial**.
4. Se recuperan conjuntamente las últimas versiones registradas, preservando
   IDs y relaciones Donante → Donacion → Unidad → MovimientoUnidad.
   También permite reconstruir filas eliminadas. Si solo se dañó un DVV y
   las filas coinciden con un historial íntegro, lo restablece explícitamente.
5. Una revisión desactualizada se rechaza: actualizar el informe y volver a
   revisar antes de confirmar.
6. La operación deja versiones RESTORE, bitácora y movimientos
   RESTAURACION para unidades/movimientos afectados. La bitácora registra
   automáticamente el actor, la fecha y la acción. Se conserva el estado
   corrupto previo dentro de la auditoría de recuperación.
7. Solo una nueva verificación correcta habilita el negocio. Los incidentes
   resueltos permanecen en IntegridadIncidente con fecha de resolución.

No es un “undo” a cualquier fecha: una unidad liberada legítimamente no vuelve
a revisión por elegir una versión histórica. No se borran inserciones
desconocidas para forzar un resultado correcto.

### Cuando hace falta backup

Si el historial no pasa su verificación, faltan verificadores de referencia o
hay filas sin versión, la recuperación automática se rechaza.
También puede ser necesaria una recuperación externa si las restricciones
relacionales impiden reconstruir el conjunto desde las versiones disponibles.

La aplicación no restaura archivos .bak. Un administrador de SQL Server debe:

1. Preservar la base dañada y los registros del incidente.
2. Restaurar un backup conocido como válido en **otra base** de diagnóstico.
3. Verificar datos, DVH/DVV e historial de esa copia con los mismos scripts,
   ajustando explícitamente el destino USE a la copia.
4. Evaluar las operaciones posteriores al backup y reconciliar los datos antes
   de planificar el reemplazo controlado de la base operativa.
5. Volver a verificar antes de habilitar la aplicación.

Restaurar un backup completo puede perder operaciones posteriores; nunca
sobrescribir automáticamente la base operativa ni mezclar tablas relacionadas
de distintas fechas.

Los hashes y el historial residen en la misma base. Detectan cambios
inconsistentes, pero no autentican los datos frente a alguien capaz de
reescribir coordinadamente datos, historial y todos los verificadores.
Para ese escenario se necesitan backups externos y permisos SQL restringidos.
La verificación de negocio no modifica el cálculo del DV de usuarios. Solo
sincroniza el campo operativo `bloqueo_digitoverificador` para impedir el
acceso no administrativo durante un incidente y lo libera al verificar la
recuperación.

## Pruebas reproducibles

Desde la raíz, con Windows, .NET Framework 4.8 y SQL Server local:

~~~powershell
dotnet build IngDeSoftware.sln --no-restore -v:q
./Tests/IntegridadPN1.Tests.ps1
./Tests/IntegridadPN1.Tests.ps1 -DatosExistentes
~~~

Las pruebas crean una base con nombre único HemovidaGest_Integridad_Test_*.
No escriben en HemovidaGest, no cambian App.local.config y eliminan
únicamente su base temporal al finalizar. -Servidor permite otra instancia;
-ConservarBase conserva la base de prueba para diagnóstico.

Cubren instalación nueva y actualización sin DVH, repetición de migración,
aceptación explícita, permisos, operaciones PN1, alteraciones de las cuatro
tablas, eliminación completa, filas desconocidas, historial alterado, revisión
desactualizada, rollback por fallo intermedio, concurrencia, bitácora,
traducciones y construcción/redimensionado de controles WinForms sin abrir
ventanas. También ejercitan Application/DAL y el bloqueo de pantallas ya abiertas.
