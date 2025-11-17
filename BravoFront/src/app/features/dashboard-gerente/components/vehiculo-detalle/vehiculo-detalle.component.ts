import { Component, Input, Output, EventEmitter, inject, OnInit } from '@angular/core';
import { VehiculoService } from '../../services/vehiculo.service';
import { computed, signal } from '@angular/core';
import { VehiculoDto } from '../../../../core/models/vehiculo.model';

@Component({
  selector: 'app-vehiculo-detalle',
  standalone: true,
  templateUrl: './vehiculo-detalle.component.html',
  styleUrls: ['./vehiculo-detalle.component.scss']
})
export class VehiculoDetalleComponent implements OnInit {
  private vehiculoService = inject(VehiculoService);

  // Recibimos el ID del vehículo
  @Input() vehiculoId!: number;

  // Emitimos evento para cerrar modal
  @Output() close = new EventEmitter<void>();

  // Signal local para almacenar detalle
  #vehiculo = signal<VehiculoDto | null>(null);
  #loading = signal(false);
  #error = signal<string | null>(null);

  // Signals públicos
  public readonly vehiculo = computed(() => this.#vehiculo());
  public readonly loading = computed(() => this.#loading());
  public readonly error = computed(() => this.#error());

  ngOnInit() {
    if (this.vehiculoId) {
      this.loadVehiculo(this.vehiculoId);
    }
  }

  loadVehiculo(id: number) {
    this.#loading.set(true);
    this.#error.set(null);

    this.vehiculoService.getVehiculoById(id).subscribe({
      next: (v) => {
        this.#vehiculo.set(v);
        this.#loading.set(false);
      },
      error: (err) => {
        this.#error.set('No se pudo cargar el detalle del vehículo.');
        this.#loading.set(false);
        console.error(err);
      }
    });
  }

  // Función para cerrar modal
  closeModal() {
    this.close.emit();
  }
}
