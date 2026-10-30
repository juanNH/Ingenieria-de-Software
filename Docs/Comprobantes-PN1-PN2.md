# Comprobantes de los procesos PN1 y PN2

## Decisiones y alcance

Los comprobantes son documentos PDF que el usuario genera desde datos guardados. El botón abre un diálogo para elegir dónde guardar el archivo. El documento indica fecha de emisión, usuario que lo generó y estado al momento de la consulta. No se modifica el estado de la operación al generar el archivo.

- **PN1:** en Unidades, seleccionar una unidad y elegir **Comprobante PDF**. Se consultan todas las unidades guardadas de esa donación y se detallan identificación, clasificación disponible, vencimiento, estado, fechas de registro y última modificación, usuario de última modificación y observaciones. La fecha de registro de una unidad no se presenta como fecha de donación.
- **PN2, solicitud:** en Solicitudes transfusionales, seleccionar una solicitud y elegir **Comprobante PDF**. Incluye solicitud, paciente, profesional, componente, cantidades, estado e historial de entregas, incluidas las devueltas o descartadas. Puede emitirse con atención parcial.
- **PN2, entrega:** en Destino de unidades, seleccionar una entrega y elegir **Comprobante PDF**. Incluye esa entrega y el estado de su solicitud al emitirlo. Cada entrega parcial tiene su propio identificador.

Los números mostrados son identificadores de la donación, solicitud o entrega; no son folios únicos de emisión. Cada archivo es una fotografía consultada al generarlo, no una copia archivada ni un documento inmutable en la base. Reemitirlo puede mostrar estados distintos. No existe un cierre definitivo de solicitud en PN2: una devolución o descarte puede reabrir el saldo. Las consultas de solicitud y entregas son independientes, por lo que una modificación simultánea puede ocurrir entre ambas lecturas.

PN1 requiere acceso de consulta a unidades. PN2 requiere los permisos existentes `SOLICITUD_VER` y `RESERVA_VER`; por ello un profesional que solo ve sus solicitudes no puede generar estos comprobantes. El sistema vuelve a verificar permisos, selección e integridad antes de generar el PDF. No se agregan permisos ni cambios de esquema de base de datos. Los mensajes y botones admiten español e inglés; el contenido del PDF está en español.

## Balanceo de entrada, comportamiento y salida

| Proceso | Entrada | Comportamiento | Salida |
| --- | --- | --- | --- |
| PN1 | E4. Unidad guardada seleccionada y pedido de comprobante. | C8. Consultar las unidades de la donación y generar el PDF. | S5. Comprobante de la donación con los estados al emitirlo. |
| PN2 | E9. Solicitud o entrega guardada seleccionada y pedido de comprobante. | C11. Consultar solicitud y entregas autorizadas y generar el PDF. | S10. Comprobante individual o consolidado con los estados y cantidades al emitirlo. |

Estas referencias aparecen en los diagramas editables de [entrada, comportamiento y salida PN1](../Diagramas/Secuencia%20de%20Roles/PN1_DiagramaEntradaComportamientoSalida.drawio) y [PN2](../Diagramas/Secuencia%20de%20Roles/PN2_DiagramaEntradaComportamientoSalida.drawio), así como en los diagramas de roles, actividad y secuencia de roles. También se agregó la acción a los diagramas de casos de uso. El modelo conceptual no incorpora una entidad de comprobante: el archivo se genera bajo demanda y no se persiste como entidad del sistema.

## Implementación y verificación

El formato común usa PDFsharp desde [ComprobantePdfService](../Services/Documentos/ComprobantePdfService.cs). La interacción de guardado está en [ComprobantePdfUI](../UI/Common/ComprobantePdfUI.cs). Para compilar, restaurar los paquetes NuGet y compilar `IngDeSoftware.sln` con Visual Studio o MSBuild. `Tests/Comprobantes.Tests.ps1` verifica PDFs individuales y consolidados, paginación y rechazo de datos inconsistentes con la compilación Debug.

El reporte inteligente de estadísticas se reserva para una etapa posterior; estos comprobantes operativos no agregan esa vista.
