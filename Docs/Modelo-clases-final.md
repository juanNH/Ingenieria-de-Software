# Modelo de clases final — G06

El modelo integrado reúne los **155 tipos** definidos por los siete proyectos de HemovidaGest: UI, Application, Repository, DAL, Domain, Services y Abstractions. Comprende 141 clases, 6 interfaces y 8 enumeraciones, incluidos los tipos anidados. El mapa general presenta las 155 definiciones UML completas en siete secciones por capa, con sus **121 relaciones internas**. Las **419 relaciones únicas del modelo completo** se muestran explícitamente en las vistas de colaboración, sin tender cables entre todas las secciones del mapa. Incluye las clases y operaciones del reporte de abastecimiento y riesgo de vencimiento.

La fuente contiene **29 páginas: «Clases por capa» y 28 vistas de colaboración**, con **1009 miembros seleccionados y 419 relaciones reconciliadas**. Las cajas UML representan clases, interfaces o enumeraciones. Las secciones de UI, Application, Repository, DAL, Domain, Services y Abstractions son contenedores reales del editable: al mover una sección se mueven sus clases y compartimentos. Cada imagen corresponde a una página completa del editable.

| Proyecto de origen | Tipos | Vistas de detalle |
| --- | ---: | --- |
| UI | 34 | Páginas 2–7: inicio, identidad, auditoría, donaciones, unidades y PN2 |
| Application | 23 | Páginas 8–12: PN2, identidad, auditoría, PN1 y XML |
| Repository | 14 | Páginas 13–16 |
| DAL | 16 | Páginas 17–20 |
| Domain | 51 | Páginas 21–27 |
| Services | 11 | Páginas 28–29 |
| Abstractions | 6 | Página 29 |
| **Total** | **155** | **28 vistas de detalle** |

## Fuente editable y trazabilidad

El [editable G06](../Diagramas/Diagramas%20de%20clases/G06%20-%20Modelo%20completo/G06%20-%20Modelo%20completo%20de%20clases.drawio) conserva todas las vistas en un archivo de draw.io. El [manifiesto versionado](../Diagramas/Diagramas%20de%20clases/G06%20-%20Modelo%20completo/G06%20-%20Modelo%20completo%20de%20clases.manifest.json) registra el SHA-256 de esa fuente y permite relacionar cada elemento gráfico con la implementación.

La [guía de G06](../Diagramas/Diagramas%20de%20clases/G06%20-%20Modelo%20completo/README.md) permite recorrer las páginas del modelo. Las [29 vistas PNG](../Diagramas/Diagramas%20de%20clases/G06%20-%20Modelo%20completo/Vistas/) se conservan junto al editable y el manifiesto, una imagen completa por página.

El [mapa general en alta resolución](../Diagramas/Diagramas%20de%20clases/G06%20-%20Modelo%20completo/Vistas/01%20-%20Clases%20por%20capa.png) y el editable permiten ampliar las clases. Las versiones sustituidas se conservan como antecedentes en el [índice de históricos](../Diagramas/Historicos/README.md).

Para cada tipo, el manifiesto conserva su identificador real, alias gráfico, capa, espacio de nombres, clase de declaración, ruta relativa y línea de origen, bases, firmas seleccionadas y páginas de definición completa. Para cada relación registra sus extremos reales, clase de relación, roles, multiplicidad cuando corresponde y páginas donde aparece. También identifica los archivos de código auditados por su hash.

## Convenciones y alcance

Los nombres gráficos omiten únicamente el sufijo `_380_jh` para facilitar la lectura. Los identificadores y las firmas del manifiesto conservan el nombre real. Los alias son únicos en el alcance representado.

Cada vista de colaboración asigna las relaciones a su clase de origen y coloca sus destinos de referencia próximos, en columnas identificadas por capa. Las conexiones se mantienen dentro de esa fila de colaboración; las cabeceras tienen una banda reservada. Una caja que contiene solamente el nombre remite a la definición completa registrada en el manifiesto. Puede repetirse incluso si la definición aparece en otra fila de la misma página: esa repetición mantiene la conexión local y no agrega tipos al total de 155.

Se representan **153 asociaciones, 216 dependencias, 30 generalizaciones, 13 realizaciones de interfaces y 7 relaciones de anidamiento**. Las relaciones de anidamiento emplean un conector continuo con un círculo y signo `+` en el tipo contenedor. Las cuatro referencias externas del CLR —`Form`, `UserControl`, `IDisposable` y `StringWriter`— aparecen como extremos UML de las relaciones reales y no se suman a los 155 tipos de la aplicación.

Los 28 detalles muestran cada una de las **419 relaciones exactamente una vez**, según su clase de origen. El mapa general repite las 121 relaciones internas de las capas para orientar la lectura; esas ocurrencias no se suman como relaciones nuevas. Las otras 298 relaciones entre capas o con el CLR se desarrollan en los detalles.

Los miembros emplean las visibilidades UML `+` pública, `#` protegida, `~` interna y `-` privada. Los miembros implícitamente públicos de las interfaces se muestran con `+`. La marca `{static}` identifica miembros estáticos y `*` indica multiplicidad múltiple en las asociaciones correspondientes.

Los **1009 miembros son una selección de los 1831 miembros declarados auditados**, no un inventario de todos los procedimientos privados. Se incluyen los atributos de dominio y los contratos declarados seleccionados, sin repetir miembros heredados. Se omiten los controles generados por el diseñador, los procedimientos privados rutinarios de presentación, dibujo, traducción y carga de opciones, y otros auxiliares privados de implementación. Los campos privados que representan colaboradores permanecen expresados mediante asociaciones estructurales. Esta selección permite conservar todos los tipos sin convertir el modelo en un inventario de controles de Windows Forms.

## Balance con los modelos parciales

La reconciliación revisó **20 fuentes parciales, con 23 páginas y 116 nombres de tipo distintos**. Los 116 corresponden a tipos existentes; G06 agrega los otros 39 tipos de la implementación auditada. Las ocurrencias repetidas de una clase en diferentes casos de uso no se suman como clases nuevas.

Se corrigieron tres fuentes parciales para mantener esa correspondencia:

| Fuente parcial | Balance realizado |
| --- | --- |
| [Maestro de componentes](../Diagramas/Diagramas%20de%20clases/T06b%20-%20Maestro%20de%20componentes%20-%20Clases.drawio) | Conserva el recorrido real de UI, Application, Repository y DAL con `MaestroPN1_380_jh`. Retira la propuesta `VersionComponente_380_jh` y sus relaciones de historial/restauración, ausentes de la implementación actual. Las operaciones corresponden a listar y guardar maestros. |
| [Cantidad e historial de donación](../Diagramas/Diagramas%20de%20clases/T06b%20-%20Correccion%20de%20cantidad%20de%20donacion%20-%20Clases.drawio) | Representa `CantidadDonacion_380_jh` y `CambioCantidadDonacion_380_jh`, declarados en [Donacion.cs](../Domain/PN1/Donacion.cs), con las operaciones reales de consulta y ajuste. Retira `CorreccionCantidadDonacion_380_jh` y reemplaza los nombres propuestos `HistorialCantidadDonacion_380_jh` y `DonacionCantidadVersion_380_jh`. |
| [XML auxiliar de PN1](../Diagramas/Diagramas%20de%20clases/PN1%20-%20XML%20auxiliar%20-%20Clases.drawio) | Identifica la vista como `UnidadesView_380_jh` y retira `PuedeConsultarUnidades_380_jh`, método ausente de `UnidadApplicationService_380_jh`. Conserva las operaciones reales de serialización y deserialización. |

Después del balance no quedaron nombres de tipo inexistentes ni relaciones parciales sin respaldo dentro del alcance reconciliado. Los controles `MainForm.cmbIdiomas_380_jh` y `BitacoraView.listViewBitacora_380_jh` existen en sus archivos Designer; su exclusión del detalle G06 responde a la convención de presentación, no a una ausencia en el código.

## Verificación y mantenimiento

Se verificaron la cobertura de los 155 tipos, los 1009 miembros seleccionados y las 419 relaciones; la unicidad de los alias; la correspondencia de los 139 archivos de código identificados por SHA-256; y la validez del XML, de sus padres y de los extremos de las aristas en las 29 páginas. El modelo global no tiene cajas superpuestas ni rutas que atraviesen el interior de una clase ajena.

El PNG global mide **10000 × 5717 píxeles** y los 28 detalles tienen un lado largo de **4000 píxeles**, con proporciones conservadas. La copia de inserción del documento mide 4000 × 2287 píxeles y deriva del mismo render vectorial del mapa general. La fuente utiliza texto de al menos 24 píxeles. El tamaño impreso varía según la escala de cada vista; cada figura ocupa una página y su enlace permite ampliar el editable o la imagen. El mapa orienta sobre el conjunto y las vistas de colaboración permiten seguir las conexiones de cada clase.

Al cambiar un contrato o una relación de la implementación, corresponde actualizar las vistas afectadas, sus modelos parciales y el manifiesto. La vista de componentes y el recorrido arquitectónico se describen en [Arquitectura.md](Arquitectura.md). El [modelo de datos G07](Modelo-datos-final.md) documenta por separado las tablas, columnas y claves foráneas reales de SQL Server.
