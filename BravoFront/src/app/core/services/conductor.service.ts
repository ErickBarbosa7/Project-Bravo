import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class ConductorService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/api/conductores`; // base del endpoint de conductores

    loading = signal(false); // señal para mostrar loader en el UI

    // Registrar uso de vehiculo (gasolina y km)
    registrarUso(data: { vehiculoId: number; kilometrosRecorridos: number; litrosConsumidos: number }) {
        this.loading.set(true); // activamos loader
        return this.http.post(`${this.apiUrl}/registrar-uso`, data).pipe(
            tap(() => this.loading.set(false)), // quitamos loader si todo sale bien
            catchError(err => {
                this.loading.set(false); // quitamos loader si hay error
                return throwError(() => err); // propagamos error
            })
        );
    }

    // Lista de todos los conductores (para el gerente)
    getListaConductores() {
        return this.http.get<any[]>(`${this.apiUrl}`);
    }

    // Reporte de combustible de un conductor especifico
    getReporteIndividual(id: string) {
        return this.http.get<any>(`${this.apiUrl}/${id}/combustible`);
    }

    // Reporte General de Flotilla
    getReporteGeneral() {
        return this.http.get<any>(`${this.apiUrl}/reporte-general`);
    }
    
    // Recomendacion
    pedirRecomendacion(distancia: number) {
        // NOTA: Apuntamos a 'api/vehiculos', no a 'api/conductores'
        const url = `${environment.apiUrl}/api/vehiculos/recomendar?distancia=${distancia}`;
        return this.http.get<any[]>(url);
    }

}
