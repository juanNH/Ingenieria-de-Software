# Comprobantes de PN1 y PN2

## Cuándo se generan

- **PN1:** al registrarse la última decisión de una donación. Todas sus unidades deben tener una decisión PN1 guardada: liberada, bloqueada o descartada. Se consulta el historial: una unidad que ya pasó a PN2 sigue contando por su decisión PN1. El PDF conserva las decisiones registradas cuando se completó la donación.
- **PN2:** al registrar el destino de cada entrega: transfundida, devuelta o descartada. Cada entrega tiene su comprobante. Registrar una entrega sin destino todavía no lo genera. Una reserva cancelada o una solicitud sin stock no equivalen a una entrega con destino.

La solicitud de PN2 puede tener varias entregas y saldo pendiente. La aplicación no tiene un cierre formal de solicitud: el botón **Exportar reporte PDF** en Solicitudes genera un resumen manual del estado actual; no es un cierre ni un comprobante automático consolidado.

El evento y su instantánea se guardan en la misma transacción que la decisión o el destino. Una vez confirmada la operación, la aplicación intenta crear el PDF y muestra su ubicación, con opción para abrirlo. El documento incluye un identificador único y las fechas de la operación y de emisión; el archivo no se sobrescribe. Las operaciones fallidas o revertidas no producen un evento.

Si falla el guardado, la operación queda confirmada y el comprobante permanece pendiente en SQL. La aplicación vuelve a intentarlo al iniciar sesión y al registrar una operación; también se puede usar **Reintentar comprobantes** en Unidades o Destino de unidades. Cada usuario procesa sus eventos mientras conserve permisos para la operación; un administrador puede procesar los pendientes de todos. Si el archivo ya se creó y faltó confirmar su registro, el reintento reutiliza el archivo existente.

**Copiar comprobante PDF** recupera el original automático sin sobrescribir archivos existentes. **Exportar reporte PDF** sigue disponible para consultar datos actuales, incluso de operaciones anteriores a la instalación, y genera un documento distinto del original. La migración no crea comprobantes retroactivos. Las copias requieren los permisos de consulta correspondientes. Los textos de la aplicación admiten español e inglés; el contenido de los PDF está en español.

## Instalación y ubicación

Con la aplicación cerrada y PN2 e IntegridadPN2 instalados, ejecutar desde la raíz:

```powershell
./Database/InstalarComprobantes.ps1
```

El instalador verifica integridad, crea y verifica un respaldo SQL, instala [ComprobantesAutomaticos.sql](../Database/StoredProcedures/ComprobantesAutomaticos.sql) en una transacción y vuelve a verificar integridad. `InstalarPN2.ps1` también instala esta actualización. Si falta la migración o el disparador está deshabilitado, la aplicación informa el problema e impide registrar la decisión PN1 o el destino PN2 hasta instalarla.

Por defecto, los archivos se guardan en la carpeta **Documentos/HemovidaGest/Comprobantes**, en subcarpetas `PN1` y `PN2`. Para que distintos puestos puedan acceder a los originales, configurar la misma ruta absoluta compartida en la clave `ComprobantesDirectorio` del archivo `UI.exe.config` de cada instalación. La cuenta que ejecuta la aplicación necesita permiso de lectura y escritura en esa carpeta. Una ruta vacía usa Documentos del usuario del equipo.

La tabla `ComprobantePendiente` conserva la instantánea necesaria para reintentar y la ruta del original. Forma parte del respaldo de la base, pero sus datos no están incluidos en el cálculo DVH/DVV de las tablas de negocio. La carpeta compartida y la base deben tener acceso restringido a los usuarios autorizados. Las instalaciones con rutas distintas pueden generar copias en distintos equipos; para compartir y reimprimir el mismo original se debe configurar una ruta común.

## Balanceo de entrada, comportamiento y salida

| Proceso | Entrada | Comportamiento | Salida |
| --- | --- | --- | --- |
| PN1 | E4. Decisiones guardadas para todas las unidades de la donación. | C8. Registrar el evento y generar automáticamente el comprobante. | S5. PDF con las decisiones o comprobante pendiente de reintento. |
| PN2 | E9. Destino de una entrega guardado. | C11. Registrar el evento y generar automáticamente el comprobante de esa entrega. | S10. PDF con el destino o comprobante pendiente de reintento. |

Las referencias figuran en los diagramas editables de [entrada, comportamiento y salida PN1](../Diagramas/Secuencia%20de%20Roles/PN1_DiagramaEntradaComportamientoSalida.drawio) y [PN2](../Diagramas/Secuencia%20de%20Roles/PN2_DiagramaEntradaComportamientoSalida.drawio), y en roles, actividad, secuencia de roles y casos de uso. Los modelos conceptuales incluyen el comprobante automático y su relación con la donación o la entrega.

## Verificación

Compilar `IngDeSoftware.sln` con restauración de NuGet. `Tests/Comprobantes.Tests.ps1` verifica los PDF manuales; `Tests/Comprobantes.Automaticos.Tests.ps1` utiliza una base SQL temporal y comprueba los resultados alternativos, rollback, instantáneas, reintento, permisos y ausencia de duplicados. El reporte inteligente de estadísticas queda para una etapa posterior.
