# PN2: estudios del receptor y búsqueda de unidades

## Alcance y decisiones

El profesional solicitante presenta la orden y el estudio del receptor. El personal habilitado registra el pedido y transcribe nombre y documento del receptor, solicitante, componente, cantidad, grupo ABO, RhD, resultado de anticuerpos, laboratorio, número de informe y fecha. El usuario que carga el pedido queda registrado por separado del nombre del solicitante. Los datos y la validación pertenecen a cada solicitud; no existe aún un padrón independiente de receptores.

El estudio empieza pendiente. La solicitud puede registrarse sin estudio; cuando se disponga de él, se cargan todos sus datos desde la solicitud seleccionada. El operador autorizado revisa la identidad, el informe y su vigencia según el protocolo del servicio; al validarlo quedan registrados usuario y fecha. Un estudio pendiente se puede corregir. Una solicitud antigua sin estudio puede completarse si no tiene reservas activas o entregadas. No se modifican estudios ya validados: para corregirlos se necesita un circuito de rectificación aún pendiente. Una solicitud antigua con reservas activas o entregadas tampoco admite completar el estudio desde esta pantalla.

La reserva requiere estudio validado, saldo pendiente y una unidad liberada y vigente del componente indicado. El sistema filtra por grupo como ayuda preliminar. El operador comprueba las pruebas y requisitos aplicables, ingresa la referencia de la comprobación y confirma expresamente la compatibilidad de esa unidad con ese receptor. La referencia queda en la observación de la reserva. Antes de entregar se vuelven a comprobar el estudio, la regla de grupo y la existencia de esa referencia.

## Reglas del filtro preliminar

Las combinaciones se administran en **Maestros → Compatibilidades** y se guardan en `CompatibilidadTransfusional`. La función `fn_PN2_GrupoCandidato` busca una fila activa para componente, grupo/Rh de la unidad y grupo/Rh del receptor. Sin fila activa no hay candidata. El componente del pedido y el de la unidad también deben coincidir.

La primera instalación convierte las reglas anteriores en datos para los componentes reconocidos que ya están en Maestros. Con los tres componentes iniciales se cargan 75 combinaciones: 27 de eritrocitos, 36 de plasma y 12 de plaquetas. Reinstalar conserva reglas personalizadas y bajas; un maestro nuevo requiere configurar explícitamente sus combinaciones.

La misma configuración se consulta al buscar, reservar y entregar. El filtro sigue siendo preliminar y el operador debe registrar la confirmación correspondiente. Ver [Compatibilidad configurable](Compatibilidad-configurable.md) para entender cada columna, usar la pantalla, instalar y probar el cambio.

## Cambios y despliegue

El cambio abarca interfaz, Application, Repository, DAL y procedimientos de PN2. Los campos nuevos son anulables para preservar registros anteriores. Integridad incluye los nuevos datos en el hash y la restauración, y conserva la representación anterior cuando todos están vacíos. Las reservas previas sin estudio validado y sin referencia de compatibilidad no pueden entregarse bajo el nuevo circuito; requieren una resolución operativa antes de actualizar una base que las contenga.

Para actualizar una base existente, cerrar la aplicación y ejecutar `Database/InstalarPN2.ps1`. El instalador comprueba integridad, crea y verifica un respaldo y ejecuta los scripts en una transacción. Reiniciar sesión para cargar permisos y traducciones. Si PN2 ya está instalado y solo se actualiza la configuración de compatibilidad, usá `Database/InstalarCompatibilidad.ps1`.

## Comprobación funcional propuesta

1. Crear un pedido con datos de receptor y estudio; comprobar que figura pendiente y no muestra candidatos.
2. Corregir un dato pendiente y validar el informe con un usuario autorizado.
3. Para glóbulos rojos, verificar candidatos compatibles y exclusión de un grupo o RhD incompatible; repetir con plasma para comprobar la regla específica.
4. Verificar que un componente desconocido y una solicitud sin estudio validado no devuelvan candidatos.
5. Intentar reservar sin referencia y rechazar la confirmación; ninguna de las dos acciones debe crear reserva. Confirmar con una referencia y verificar saldo, usuario y observación.
6. Comprobar entrega, cancelación, concurrencia y protección de integridad, incluida la recuperación de datos y solicitudes anteriores.

Referencias clínicas para revisar las reglas: [Lifeblood: compatibilidad por componente](https://www.lifeblood.com.au/health-professionals/products/component-compatibility) y [Canadian Blood Services: pruebas pretransfusionales](https://professionaleducation.blood.ca/en/transfusion/clinical-guide/pre-transfusion-testing).
