import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap, catchError, throwError, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateVehiculoDto, UpdateVehiculoDto, Vehiculo } from '../models/vehiculo.model';

@Injectable({
  providedIn: 'root'
})
export class VehiculoService {

  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/vehiculos`;

  // State (signals)
  public vehiculos = signal<Vehiculo[]>([]);
  public loading = signal<boolean>(false);
  public error = signal<string | null>(null);

  constructor() {}

  // Cargar vehiculos
  loadVehiculos(): void {
    this.loading.set(true);
    this.error.set(null);

    this.http.get<Vehiculo[]>(this.apiUrl).subscribe({
      next: (data) => {
        this.vehiculos.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error cargando vehículos:', err);
        this.error.set('No se pudo cargar la lista de vehículos.');
        this.loading.set(false);
      }
    });
  }

  // Obtener vehiculo por id
  getVehiculoById(id: number): Observable<Vehiculo> {
    return this.http.get<Vehiculo>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  // Crear vehiculo
  createVehiculo(dto: CreateVehiculoDto): Observable<Vehiculo> {
    this.loading.set(true);
    return this.http.post<Vehiculo>(this.apiUrl, dto).pipe(
      tap((nuevoVehiculo) => {
        this.vehiculos.update(lista => [...lista, nuevoVehiculo]);
        this.loading.set(false);
      }),
      catchError((err) => {
        this.loading.set(false);
        return this.handleError(err);
      })
    );
  }

  // Actualizar vehiculo
  updateVehiculo(id: number, dto: UpdateVehiculoDto): Observable<Vehiculo> {
    this.loading.set(true);
    return this.http.put<Vehiculo>(`${this.apiUrl}/${id}`, dto).pipe(
      tap((vehiculoActualizado) => {
        this.vehiculos.update(lista =>
          lista.map(v => v.id === id ? vehiculoActualizado : v)
        );
        this.loading.set(false);
      }),
      catchError((err) => {
        this.loading.set(false);
        return this.handleError(err);
      })
    );
  }

  // Eliminar vehiculo
  deleteVehiculo(id: number): Observable<void> {
    this.loading.set(true);
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        this.vehiculos.update(lista => lista.filter(v => v.id !== id));
        this.loading.set(false);
      }),
      catchError((err) => {
        this.loading.set(false);
        return this.handleError(err);
      })
    );
  }

  // Obtener estatus del servicio, por ejemplo cuanto le falta para el proximo servicio
  getEstatusServicio(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}/estatus-servicio`).pipe(
      catchError(this.handleError)
    );
  }

  // Obtener proyeccion de gastos
  getProyeccionGastos(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/proyeccion-gastos`).pipe(
      catchError(this.handleError)
    );
  }

  // Simular pago
  simularPago(dto: { vehiculoId: number; monto: number; concepto: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/simular-pago`, dto).pipe(
      tap(() => this.loadVehiculos()),
      catchError(this.handleError)
    );
  }

  // Enviar a taller
  enviarATaller(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/enviar-taller`, {}).pipe(
      tap(() => this.loadVehiculos()),
      catchError(this.handleError)
    );
  }

  // Helper manejo de errores
  private handleError(error: any) {
    console.error('Error en VehiculoService:', error);
    return throwError(() => new Error(error.error?.message || 'Error en el servidor'));
  }
}
