# Analisis y Planeacion: Proyecto Bravo

Despues de revisar la logica de negocio en la capa de servicios (VehiculoService, ConductorService) y la estructura de los modelos, se han detectado deficiencias arquitectonicas y de dominio. A continuacion se presenta una evaluacion detallada de los problemas actuales y las soluciones propuestas con un enfoque de ingenieria de software solido.

---

## 1. Problemas Arquitectonicos y Logica de Negocio

### A. Desplazamiento del Intervalo de Servicio (Drift)
- **Problema:** En SimularPagoServicio, el proximo servicio se calcula como `SiguienteServicioKm = KilometrajeActual + IntervaloServicioKm`. Si un vehiculo ingresa tarde a mantenimiento (ej. tocaba a los 20,000 km pero ingreso a los 22,000 km), el proximo hito se recorre a los 32,000 km, causando un desfase acumulativo en el cronograma original.
- **Solucion:** Calcular el proximo servicio sumando el intervalo a la meta original superada (`SiguienteServicioKm += IntervaloServicioKm`), o permitir al gerente ajustar el parametro con base en la recomendacion del area de mantenimiento.

### B. Ausencia de Control de Concurrencia (Condiciones de Carrera)
- **Problema:** En RegistrarUsoVehiculo, multiples despachadores podrian visualizar un mismo vehiculo como Disponible. Si envian peticiones en el mismo instante, las validaciones pasarian y el vehiculo podria ser asignado a dos operaciones o viajes distintos, corrompiendo la integridad del sistema.
- **Solucion:** Implementar control de concurrencia optimista utilizando un Concurrency Token (ej. un campo `RowVersion` de Entity Framework) en la entidad Vehiculo. Si el estado cambia entre la lectura y la escritura, la base de datos abortara la transaccion subsecuente.

### C. Sobrescritura Forzada de Estados y Validaciones en Taller
- **Problema:** Registrar un pago en SimularPagoServicio altera automaticamente el vehiculo a `EstadoVehiculo.Disponible`. Este acoplamiento asume que la facturacion equivale a una reparacion finalizada. Adicionalmente, el sistema no bloquea el uso de autos que esten en estado `EnTaller`.
- **Solucion:** Desacoplar el flujo financiero del flujo operativo. Marcar un auto como disponible debe ser una accion explicita e independiente del registro del pago. Adicionalmente, bloquear cualquier operacion operativa si `vehiculo.Estado != Disponible`.

### D. La Dualidad de Dominio: Viajes vs Usos
- **Problema:** Existe la entidad BitacoraViaje sin uso aparente en la API, delegando toda la logica a BitacoraUso. Sugerir la eliminacion de uno podria ser prematuro.
- **Solucion:** Comprender la separacion del dominio. BitacoraViaje corresponde al area de Operaciones (logistica, chofer, horarios, destinos) y BitacoraUso al area de Finanzas (consumibles, tickets de gasolina, desgaste). Si el negocio requiere auditorias precisas, se debe establecer una relacion de uno a muchos (Viaje -> Gastos/Usos). Si no existe la necesidad de separar logistica de gasto, Operaciones y Finanzas deben acordar la unificacion antes de deprecar entidades.

### E. Proyeccion de Gastos Distorsionada
- **Problema:** CalcularProyeccionMensual divide el historico global de gastos de la empresa entre todos los kilometros recorridos. Este enfoque asume que el costo por kilometro de un vehiculo utilitario ligero es el mismo que el de un transporte de carga pesada, diluyendo los costos.
- **Solucion:** Las proyecciones deben calcularse a nivel de flota segmentada por modelo o por vehiculo individual, evaluando que unidades se aproximan a sus limites de mantenimiento en el siguiente ciclo contable.

---

## 2. Problemas de Rendimiento y ORM

### A. Problema de Consultas N+1
- **Problema:** RecomendarVehiculos itera sobre los vehiculos en memoria y ejecuta una peticion a la base de datos (`await _context.BitacorasUso.Where(...)`) por cada vehiculo en el ciclo. Al crecer la flota, esto agotara los recursos de la base de datos.
- **Solucion:** Utilizar consultas Eager Loading (`.Include()`) o proyectar los promedios agrupadamente desde SQL en una sola consulta antes de traer los datos a la memoria de la aplicacion.

### B. El Anti-patron del Estado Derivado
- **Problema:** Extraer el estatus "Preventivo" en memoria requiere traer los datos para calcular la diferencia de kilometraje, perdiendo la capacidad de indexacion. Sin embargo, guardar un estado "Preventivo" directamente en la base de datos es una trampa de diseño, ya que obligaria a actualizar el estado con cada kilometro avanzado; una falla de sincronizacion generaria inconsistencias.
- **Solucion:** No modificar la definicion estatica de `EstadoVehiculo`. Emplear el ORM para trasladar la logica aritmetica directamente a SQL sin materializar el estado derivado en una columna fisica:
```csharp
var vehiculosPreventivos = await _context.Vehiculos
    .Where(v => (v.SiguienteServicioKm - v.KilometrajeActual) <= UMBRAL_PREVENTIVO)
    .ToListAsync();
```

---

## 3. Plan de Accion (Estrategia de Ingenieria)

Para garantizar la estabilidad del sistema durante la refactorizacion, se propone el siguiente plan escalonado:

**Fase 0: Cobertura de Pruebas (Red de Seguridad)**
Antes de modificar calculos de mantenimiento o refactorizar servicios, implementar pruebas unitarias que certifiquen el comportamiento de la logica actual. Esto provee una red de seguridad contra regresiones funcionales.

**Fase 1: Optimizacion de Lectura (Quick Wins)**
Resolver el cuello de botella N+1 en RecomendarVehiculos. Esto disminuye la carga en la base de datos de manera inmediata sin modificar las reglas de negocio base.

**Fase 2: Blindaje Transaccional**
Implementar validaciones estructurales: bloquear el registro de kilometraje en vehiculos en el taller e incluir bloqueos de concurrencia optimista (`RowVersion`) para evitar dobles asignaciones.

**Fase 3: Refactorizacion de Dominio**
Desacoplar la confirmacion de pago del cambio de estado del vehiculo. Corregir la formula matematica del calculo de intervalos de mantenimiento para prevenir el desfase (Drift).

**Fase 4: Inteligencia de Negocio**
Reestructurar el algoritmo de proyecciones financieras y estadisticas del dashboard para trabajar con segmentos de vehiculos y parametros individuales, mejorando la toma de decisiones.
