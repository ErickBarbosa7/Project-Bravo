import { Component, inject, input, output, signal, effect, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConductorService } from '../../../../../../core/services/conductor.service';
import { VehiculoService } from '../../../../../../core/services/vehiculo.service';
import { AlertService } from '../../../../../../shared/services/alert.service';

@Component({
  selector: 'app-formulario-carga',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './formulario-carga.component.html',
  styleUrls: ['./formulario-carga.component.scss']
})
export class FormularioCargaComponent implements OnInit {
  private fb = inject(FormBuilder);
  private conductorService = inject(ConductorService);
  private vehiculoService = inject(VehiculoService);
  private alert = inject(AlertService);

  // INPUTS / OUTPUTS 
  // Recibimos el viaje desde el padre 
  public viajeActivo = input<any>(null);
  
  // Avisamos al padre cuando terminamos para que limpie la memoria
  public onViajeTerminado = output<void>();
  public onSolicitarIA = output<void>(); // Para cambiar de tab desde aquí
    public todosLosVehiculos = this.vehiculoService.vehiculos;
  // --- ESTADO LOCAL ---
  public vehiculos = this.vehiculoService.vehiculos;
  public isLoading = this.conductorService.loading;
  public isManualMode = signal(false);

  public vehiculosDisponibles = computed(() => {
    return this.todosLosVehiculos().filter(v => 
      v.estado === 0 || // 0 = Disponible (Verde)
      (this.viajeActivo() && v.id === this.viajeActivo().id) // O es el que ya traigo
    );
  });
  public form = this.fb.group({
    vehiculoId: ['', [Validators.required]],
    kilometrosRecorridos: ['', [Validators.required, Validators.min(1)]],
    litrosConsumidos: ['', [Validators.required, Validators.min(0.1)]]
  });

  constructor() {
    // Efecto: Si llega un viaje activo desde el padre, pre-llenamos el ID
    effect(() => {
      const viaje = this.viajeActivo();
      if (viaje) {
        this.form.patchValue({ vehiculoId: viaje.id.toString() });
        this.isManualMode.set(false); // Apagamos manual si hay viaje asignado
      }
    });
  }

  ngOnInit() {
    this.vehiculoService.loadVehiculos();
  }

  // --- MÉTODOS DE UI ---
  activarManual() {
    this.isManualMode.set(true);
    this.form.reset();
  }

  cancelar() {
    this.isManualMode.set(false);
    this.onViajeTerminado.emit(); // Esto limpiará el viaje en el padre también
    this.form.reset();
  }

  irAAsistente() {
    this.onSolicitarIA.emit();
  }

  // --- ENVÍO ---
  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.alert.warning('Completa todos los datos.');
      return;
    }

    // Defensa: Si hay viaje activo, aseguramos el ID
    if (this.viajeActivo() && !this.form.value.vehiculoId) {
        this.form.patchValue({ vehiculoId: this.viajeActivo().id.toString() });
    }

    const data = {
      vehiculoId: Number(this.form.value.vehiculoId),
      kilometrosRecorridos: Number(this.form.value.kilometrosRecorridos),
      litrosConsumidos: Number(this.form.value.litrosConsumidos)
    };

    this.conductorService.registrarUso(data).subscribe({
      next: () => {
        this.alert.success('Carga registrada correctamente.');
        this.form.reset();
        this.isManualMode.set(false);
        this.vehiculoService.loadVehiculos();
        
        // Avisamos al padre que ya acabamos
        this.onViajeTerminado.emit();
      },
      error: (err) => {
        console.error(err);
        this.alert.error('Error al registrar.');
      }
    });
  }
}