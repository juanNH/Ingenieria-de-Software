# Serializar y deserializar la tabla de unidades

En **Banco de sangre → Unidades**, usá **Serializar tabla** para guardar en XML todas las filas que aparecen en la grilla. No hace falta seleccionar una unidad. Si aplicaste una búsqueda, se serializan las filas visibles de esa búsqueda. El diálogo permite elegir la carpeta y el nombre del archivo.

Para probar la carga con una unidad ficticia:

1. Abrí una copia del archivo XML exportado con un editor de texto.
2. Dentro de `<TablaUnidadesPN1>`, copiá un elemento completo `<Unidad>...</Unidad>` y pegalo a continuación.
3. Cambiá `<CodigoIdentificacion>` por un código único dentro del archivo. Podés cambiar también el donante u otros datos, conservando la estructura y los valores válidos.
4. En la pantalla, elegí **Limpiar tabla**. La grilla queda vacía solo visualmente.
5. Elegí **Deserializar XML** y seleccioná el archivo editado. Las filas originales y la ficticia aparecen en la misma grilla. También podés volver a serializar esa vista.
6. Para recuperar las unidades de la base, usá **Actualizar** o hacé una nueva búsqueda. Se conserva el filtro que esté escrito.

El formato nuevo contiene una unidad por fila y usa fecha `AAAA-MM-DD`. Admite los estados `EN_REVISION`, `LIBERADA`, `BLOQUEADA`, `DESCARTADA`, `RESERVADA`, `ENTREGADA`, `TRANSFUNDIDA` y `DEVUELTA`. Grupo y Rh pueden quedar vacíos para una unidad en revisión; para los demás estados deben estar completos. Los códigos deben ser únicos. Los archivos `ProcesoPN1` exportados por la versión anterior también se pueden deserializar.

Las filas importadas son temporales y su existencia no se verifica contra la base. Las acciones para clasificar o cambiar el estado quedan deshabilitadas mientras se muestran. Limpiar o importar no modifica la base.
