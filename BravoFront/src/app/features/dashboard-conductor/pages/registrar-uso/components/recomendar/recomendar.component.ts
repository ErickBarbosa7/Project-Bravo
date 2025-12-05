import { Component, inject, output, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConductorService } from '../../../../../../core/services/conductor.service';
import { AlertService } from '../../../../../../shared/services/alert.service';

@Component({
  selector: 'app-recomendar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './recomendar.component.html',
  styleUrls: ['./recomendar.component.scss']
})
export class RecomendarComponent {
  // Inyecciones
  private conductorService = inject(ConductorService);
  private alert = inject(AlertService);

  // Output: Emite el vehículo elegido al componente padre
  public onVehiculoSeleccionado = output<{ vehiculo: any, distancia: number }>();
  // Estado Local (Signals)
  public distanciaInput = signal<number | null>(null);
  public recomendaciones = signal<any[]>([]); // Array para guardar toda la lista
  public buscandoAuto = signal(false);
  public hasSearched = signal(false); // Para saber si ya buscó al menos una vez

  // Computed: Helpers para el template (Opcional, para limpieza)
  public mejorOpcion = computed(() => this.recomendaciones()[0] || null);
  public alternativas = computed(() => this.recomendaciones().slice(1));

  // 1. Buscar en el Backend
  buscarAutoIdeal() {
    const dist = this.distanciaInput();
    
    if (!dist || dist <= 0) {
      this.alert.warning('Por favor ingresa una distancia válida.');
      return;
    }

    this.buscandoAuto.set(true);
    this.hasSearched.set(true);
    this.recomendaciones.set([]); // Limpiar resultados anteriores

    this.conductorService.pedirRecomendacion(dist).subscribe({
      next: (res) => {
        this.buscandoAuto.set(false);
        
        if (res && res.length > 0) {
          this.recomendaciones.set(res); // Guardamos TODO el array
        } else {
          this.alert.warning('No hay vehículos disponibles para esa distancia.');
        }
      },
      error: (err) => {
        console.error(err);
        this.buscandoAuto.set(false);
        this.alert.error('Error al consultar el asistente.');
      }
    });
  }

  // 2. Seleccionar un vehículo (El botón "Usar")
  seleccionar(vehiculo: any) {
    const dist = this.distanciaInput(); // Obtenemos lo que escribió el usuario

    if (vehiculo && dist) {
      // Emitimos el paquete completo
      this.onVehiculoSeleccionado.emit({ 
        vehiculo: vehiculo, 
        distancia: dist 
      });
    } else {
      // Si por alguna razón no hay distancia (raro, pero posible), enviamos 0
      this.onVehiculoSeleccionado.emit({ 
        vehiculo: vehiculo, 
        distancia: 0 
      });
    }
  }
}