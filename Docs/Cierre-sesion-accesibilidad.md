# Cierre de sesión y accesibilidad

La regresión real del 15/11/2026 pasó desde Visual Studio: cinco ciclos de ingreso, apertura del reporte, selección de una fila y cierre de sesión; luego un sexto ingreso y cierre de la aplicación mediante la X con el reporte abierto. La interrupción en primera oportunidad para `System.NullReferenceException` permaneció activada. El proceso corregido terminó con código `0`.

## Problema reproducido

Antes del cambio, el primer cierre de sesión desde el reporte detuvo el depurador en `System.Windows.Forms.DataGridViewCell.DataGridViewCellAccessibleObject.get_FragmentRoot()`. La pila completa, con código externo visible, fue:

```text
DataGridViewCell.DataGridViewCellAccessibleObject.FragmentRoot.get()
AccessibleObject.IRawElementProviderFragment.FragmentRoot.get()
InternalAccessibleObject.IRawElementProviderFragment.FragmentRoot.get()
[Native to Managed Transition]
[Managed to Native Transition]
Application.ComponentManager.IMsoComponentManager.FPushMessageLoop(...)
Application.ThreadContext.RunMessageLoopInner(...)
Application.ThreadContext.RunMessageLoop(...)
Application.RunDialog(...)
Form.ShowDialog(IWin32Window owner)
Form.ShowDialog()
UI.Program_380_jh.Main() — línea 23 de la versión anterior
```

La excepción tuvo origen en `System.Windows.Forms`, HResult `0x80004003` e `InnerException = null`. Visual Studio mostró el getter descompilado detenido en `return owner.DataGridView.AccessibilityObject;`. El módulo optimizado no permitió inspeccionar `owner`, por lo que no se atribuye el valor nulo a uno de los dos objetos sin más evidencia.

La pila ya estaba atendiendo el siguiente ingreso: un cliente de UI Automation conservaba un proveedor de una celda del formulario anterior. Esa referencia podía volver a consultarse después de destruir la grilla. La línea del bucle principal indicada por Visual Studio era el punto de entrada al procesamiento de mensajes.

## Corrección

[Program.cs](../UI/Program.cs) desconecta los proveedores UI Automation del proceso al finalizar `Application.Run(mainForm)` y el `using` del formulario principal, antes de construir el siguiente ingreso. Los controles nuevos publican sus propios proveedores y siguen siendo accesibles.

La llamada pública `UiaDisconnectAllProviders` comprueba el HRESULT mediante `Marshal.ThrowExceptionForHR`. Se ejecuta fuera del manejador de cierre y del procesamiento síncrono de `SendMessage`. Un guard de versión evita invocar una API inexistente en Windows anteriores a 8; la corrección de UIA se validó en el equipo Windows actual.

Microsoft documenta esta API para liberar los recursos de los proveedores antes del cierre, así como sus restricciones de reentrada y su disponibilidad desde Windows 8 / Server 2012. [Referencia de Microsoft](https://learn.microsoft.com/en-us/windows/win32/api/uiautomationcoreapi/nf-uiautomationcoreapi-uiadisconnectallproviders).

## Validación desde Visual Studio

Se ejecutó `UI/bin/Release/UI.exe` mediante F5, con el acceso real de la cuenta inicial del entorno y consultas a accesibilidad sobre la grilla. En cada sesión se abrió **Consultas → Abastecimiento y riesgo de vencimiento** y se seleccionó una celda de la única cohorte disponible.

| Recorrido | Resultado |
| --- | --- |
| Sesiones 1 a 5: reporte → fila seleccionada → Cerrar sesión | Regreso al ingreso en el mismo proceso; sin interrupción por la excepción original. |
| Ingresos posteriores | Autenticación correcta; campos del ingreso y celdas del reporte nuevamente accesibles. |
| Sesión 6: reporte → fila seleccionada → X de la aplicación | Terminación normal, código `0`. |
| Configuración del depurador al finalizar | `System.NullReferenceException`: **Break When Thrown** activado. |

El ensayo original se detuvo manualmente después de capturar la pila; su código de salida forzado no demuestra una excepción sin controlar. La salida de la versión corregida también incluyó notificaciones de primera oportunidad `UnauthorizedAccessException` y `ObjectDisposedException` de WinForms; no provocaron interrupción ni impidieron los recorridos. El resultado comprobado es la desaparición de la `NullReferenceException` reproducida y el cierre normal.

Binario Release probado: SHA-256 `2a1a9149e382f1a48a401e3f76fd5a55dc335af6db5bcb8b18d74d370a35ac1d`.

## Repetición de la prueba

1. Mantener **Break When Thrown** para `System.NullReferenceException` e iniciar con F5.
2. Ingresar, abrir el reporte y seleccionar una fila. Inspeccionar la grilla con el cliente de accesibilidad para publicar sus proveedores.
3. Elegir **Cerrar sesión**, confirmar que vuelve el ingreso y repetir cinco veces.
4. Ingresar nuevamente, abrir el reporte, seleccionar una fila y cerrar mediante la X.
5. Comprobar salida `0`. Si aparece una excepción, conservar la primera pila con código externo visible y sus detalles antes de continuar.

Las pruebas de `Tests/CicloSesion.UI.Tests.ps1` cubren el bucle Run/Dispose/Retry y los observadores, pero no ejecutan `Program.Main` ni retienen proveedores desde un cliente UIA externo. Por eso no sustituyen esta regresión real.
