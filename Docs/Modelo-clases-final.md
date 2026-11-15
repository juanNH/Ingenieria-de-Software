# Modelo de clases final — G06

El modelo integrado reúne los **156 tipos** definidos por los siete proyectos de HemovidaGest: UI, Application, Repository, DAL, Domain, Services y Abstractions. Comprende 142 clases, 6 interfaces y 8 enumeraciones, incluidos los tipos anidados. La primera vista presenta las dependencias entre capas; las siguientes desarrollan los atributos de dominio, constructores y contratos declarados de cada tipo. Incluye las clases y operaciones del reporte de abastecimiento y riesgo de vencimiento.

La fuente contiene **29 páginas: una vista de integración y 28 vistas de detalle**, con **1011 miembros seleccionados y 422 relaciones reconciliadas**. Cada imagen corresponde a una página completa del editable.

| Capa | Tipos | Páginas de definición completa |
| --- | ---: | --- |
| UI | 35 | 2–6 |
| Application | 23 | 7–12 |
| Repository | 14 | 13–16 |
| DAL | 16 | 17–20 |
| Domain | 51 | 21–27 |
| Services | 11 | 28–29 |
| Abstractions | 6 | 29, junto con servicios de serialización |
| **Total** | **156** | **28 vistas de detalle** |

## Fuente editable y trazabilidad

El [editable G06](../Diagramas/Arquitectura/G06%20-%20Clases%20completas%20por%20capa.drawio) conserva todas las vistas en un archivo de draw.io. El [manifiesto versionado](../Diagramas/Arquitectura/G06%20-%20Clases%20completas%20por%20capa.manifest.json) registra el SHA-256 de esa fuente y permite relacionar cada elemento gráfico con la implementación.

Para cada tipo, el manifiesto conserva su identificador real, alias gráfico, capa, espacio de nombres, clase de declaración, ruta relativa y línea de origen, bases, firmas seleccionadas y páginas de definición completa. Para cada relación registra sus extremos reales, clase de relación, roles, multiplicidad cuando corresponde y páginas donde aparece. También identifica los archivos de código auditados por su hash.

## Convenciones y alcance

Los nombres gráficos omiten únicamente el sufijo `_380_jh` para facilitar la lectura. Los identificadores y las firmas del manifiesto conservan el nombre real. Los alias son únicos en el alcance representado.

Las relaciones entre vistas repiten los tipos de frontera como extremos de referencia. Una caja que contiene solamente el nombre remite a la definición completa del tipo registrada en el manifiesto. Estas repeticiones conservan los extremos de las relaciones y no agregan tipos al total de 156.

Se representan **155 asociaciones, 216 dependencias, 31 generalizaciones, 13 realizaciones de interfaces y 7 relaciones de anidamiento**. La vista inicial corresponde a las dependencias entre proyectos; Domain y Abstractions participan como modelos y contratos compartidos, y no como pasos sucesivos del acceso a datos.

Los miembros emplean las visibilidades UML `+` pública, `#` protegida, `~` interna y `-` privada. Los miembros implícitamente públicos de las interfaces se muestran con `+`. La marca `{static}` identifica miembros estáticos y `*` indica multiplicidad múltiple en las asociaciones correspondientes.

Se incluyen los atributos de dominio y los contratos declarados seleccionados, sin repetir miembros heredados. Se omiten los controles generados por el diseñador, los procedimientos privados rutinarios de presentación, dibujo, traducción y carga de opciones, y otros auxiliares privados de implementación. Los campos privados que representan colaboradores permanecen expresados mediante asociaciones estructurales. Esta selección permite conservar todos los tipos sin convertir el modelo en un inventario de controles de Windows Forms.

## Balance con los modelos parciales

La reconciliación revisó **20 fuentes parciales, con 23 páginas y 116 nombres de tipo distintos**. Los 116 corresponden a tipos existentes; G06 agrega los otros 40 tipos de la implementación auditada. Las ocurrencias repetidas de una clase en diferentes casos de uso no se suman como clases nuevas.

Se corrigieron tres fuentes parciales para mantener esa correspondencia:

| Fuente parcial | Balance realizado |
| --- | --- |
| [Maestro de componentes](../Diagramas/Bitacora%20de%20cambios/T06b%20-%20Maestro%20de%20componentes%20-%20Clases.drawio) | Conserva el recorrido real de UI, Application, Repository y DAL con `MaestroPN1_380_jh`. Retira la propuesta `VersionComponente_380_jh` y sus relaciones de historial/restauración, ausentes de la implementación actual. Las operaciones corresponden a listar y guardar maestros. |
| [Cantidad e historial de donación](../Diagramas/Bitacora%20de%20cantidad%20de%20donacion/T06b%20-%20Correccion%20de%20cantidad%20de%20donacion%20-%20Clases.drawio) | Representa `CantidadDonacion_380_jh` y `CambioCantidadDonacion_380_jh`, declarados en [Donacion.cs](../Domain/PN1/Donacion.cs), con las operaciones reales de consulta y ajuste. Retira `CorreccionCantidadDonacion_380_jh` y reemplaza los nombres propuestos `HistorialCantidadDonacion_380_jh` y `DonacionCantidadVersion_380_jh`. |
| [XML auxiliar de PN1](../Diagramas/Diagramas%20de%20clases/PN1%20-%20XML%20auxiliar%20-%20Clases.drawio) | Identifica la vista como `UnidadesView_380_jh` y retira `PuedeConsultarUnidades_380_jh`, método ausente de `UnidadApplicationService_380_jh`. Conserva las operaciones reales de serialización y deserialización. |

Después del balance no quedaron nombres de tipo inexistentes ni relaciones parciales sin respaldo dentro del alcance reconciliado. Los controles `MainForm.cmbIdiomas_380_jh` y `BitacoraView.listViewBitacora_380_jh` existen en sus archivos Designer; su exclusión del detalle G06 responde a la convención de presentación, no a una ausencia en el código.

## Verificación y mantenimiento

Se verificaron la cobertura de los 156 tipos, los 1011 miembros seleccionados y las 422 relaciones; la unicidad de los alias; y la validez del XML, de sus padres y de los extremos de las aristas en las 29 páginas. Los PNG se exportaron proporcionalmente con un lado largo de 4000 píxeles. La fuente utiliza texto de al menos 24 píxeles; el tamaño impreso depende de la escala de inserción del documento.

Al cambiar un contrato o una relación de la implementación, corresponde actualizar las vistas afectadas, sus modelos parciales y el manifiesto. La vista de componentes y el recorrido arquitectónico se describen en [Arquitectura.md](Arquitectura.md). El [modelo de datos G07](Modelo-datos-final.md) documenta por separado las tablas, columnas y claves foráneas reales de SQL Server.
