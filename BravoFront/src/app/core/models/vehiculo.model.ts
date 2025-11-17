
export interface VehiculoDto {
  id: number;
  placa: string;
  nombre: string;
  marca?: string;
  modelo?: string;
  año?: number;
  fotoUrl?: string;
  kilometrajeActual: number;
  estado: string; // "Disponible", "EnRuta", "EnTaller", "NecesitaServicio"
  intervaloServicioKm: number;
  siguienteServicioKm: number;
}

export interface CreateVehiculoDto {
  placa: string;
  nombre: string;
  marca?: string;
  modelo?: string;
  año?: number;
  kilometrajeActual: number;
  intervaloServicioKm: number;
}

export interface UpdateVehiculoDto extends CreateVehiculoDto {
  id: number;
}