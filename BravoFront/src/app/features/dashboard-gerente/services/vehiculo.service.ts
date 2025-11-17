import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class VehiculoService {

  private apiUrl = `${environment.apiUrl}/api/vehiculos`;
  // Signals SOLO para la lista
  loading = signal(false);
  error = signal<string | null>(null);
  vehiculos = signal<any[]>([]);

  constructor(private http: HttpClient) {}

  // -------------------------
  // GET LISTA (usa signals)
  // -------------------------
  loadVehiculos() {
    this.loading.set(true);
    this.error.set(null);

    this.http.get<any[]>(this.apiUrl).subscribe({
      next: (data) => {
        this.vehiculos.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Error al cargar vehículos');
        this.loading.set(false);
      }
    });
  }
  getVehiculos(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl).pipe(
      catchError(err => {
        console.error('Error en getVehiculos():', err);
        return throwError(() => err);
      })
    );
  }

  // -------------------------
  // GET /vehiculos/:id
  // -------------------------
  getVehiculoById(id: number) {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      catchError(err => {
        console.error('Error obteniendo vehículo:', err);
        return throwError(() => err);
      })
    );
  }

  // -------------------------
  // POST /vehiculos
  // -------------------------
  createVehiculo(dto: any) {
    return this.http.post<any>(this.apiUrl, dto).pipe(
      catchError(err => {
        console.error('Error creando vehículo:', err);
        return throwError(() => err);
      })
    );
  }

  // -------------------------
  // PUT /vehiculos/:id
  // -------------------------
  updateVehiculo(id: number, dto: any) {
    return this.http.put<any>(`${this.apiUrl}/${id}`, dto).pipe(
      catchError(err => {
        console.error('Error actualizando vehículo:', err);
        return throwError(() => err);
      })
    );
  }

  // -------------------------
  // DELETE /vehiculos/:id
  // -------------------------
  deleteVehiculo(id: number) {
    return this.http.delete(`${this.apiUrl}/${id}`).pipe(
      catchError(err => {
        console.error('Error eliminando vehículo:', err);
        return throwError(() => err);
      })
    );
  }
}
