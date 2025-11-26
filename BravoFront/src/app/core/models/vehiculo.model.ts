// Enum para el estado del vehiculo (Debe coincidir con el backend: 0,1,2,3)
export enum EstadoVehiculo {
  Disponible = 0,
  EnRuta = 1,
  EnTaller = 2,
  NecesitaServicio = 3
}

// Entidad principal (Lo que devuelve el GET)
export interface Vehiculo {
  id: number;
  placa: string;
  nombre: string; // Opcional si en backend se llama igual
  marca: string;
  modelo: string;
  anio: number;
  fotoUrl: string;
  kilometrajeActual: number;
  intervaloServicioKm: number;
  siguienteServicioKm: number;
  estado: number;
}

// DTO para crear vehiculo
export interface CreateVehiculoDto {
  placa: string;
  nombre: string;
  marca: string;
  modelo: string;
  anio: number;
  fotoUrl: string;
  kilometrajeActual: number;
  intervaloServicioKm: number;
}

// DTO para actualizar vehiculo (PUT) - Puede requerir id
export interface UpdateVehiculoDto {
  id?: number; // Opcional si va en la URL
  placa: string;
  nombre: string;
  marca: string;
  modelo: string;
  anio: number;
  fotoUrl: string;
  kilometrajeActual: number;
  intervaloServicioKm: number;
}

// Semaforo (Respuesta del endpoint de estatus)
export interface ReporteMantenimiento {
  estatus: number; // 0=Verde, 1=Amarillo, 2=Rojo
  color: string;   // "ROJO", "VERDE"
  kmRestantes: number;
  mensaje: string;
}
