// Enum para el estado del vehiculo 
export enum EstadoVehiculo {
  Disponible = 0,
  EnRuta = 1,
  EnTaller = 2,
  NecesitaServicio = 3
}

// Entidad principal 
export interface Vehiculo {
  id: number;
  placa: string;
  nombre: string;
  marca: string;
  modelo: string;
  anio: number;
  fotoUrl: string;
  kilometrajeActual: number;
  intervaloServicioKm: number;
  siguienteServicioKm: number;
  estado: EstadoVehiculo; 
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

// DTO para actualizar vehiculo (PUT)
export interface UpdateVehiculoDto {
  id?: number;
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
  color: string;   // "ROJO", "VERDE", "AMARILLO"
  kmRestantes: number;
  mensaje: string;
  estadoVehiculo: EstadoVehiculo;
}
