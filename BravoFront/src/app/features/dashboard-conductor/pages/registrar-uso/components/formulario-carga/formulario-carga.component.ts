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
  // --- Inyecciones ---
  private fb = inject(FormBuilder);
  private conductorService = inject(ConductorService);
  private vehiculoService = inject(VehiculoService);
  private alert = inject(AlertService);

  // --- Inputs / Outputs ---
  public viajeActivo = input<any>(null); // Recibe el viaje desde el padre
  public onViajeTerminado = output<void>(); // Avisa para limpiar
  public onSolicitarIA = output<void>(); // Cambiar de tab

  // --- Estado Local ---
  public todosLosVehiculos = this.vehiculoService.vehiculos; // Lista completa (Signal)
  public isLoading = this.conductorService.loading;
  public isManualMode = signal(false);

  // 🛡️ FILTRO DE SEGURIDAD:
  // Solo mostramos los disponibles (Verdes) o el que ya trae asignado
  public vehiculosDisponibles = computed(() => {
    return this.todosLosVehiculos().filter(v => 
      v.estado === 0 || // 0 = Disponible
      (this.viajeActivo() && v.id === this.viajeActivo().id) // O es el mío
    );
  });

  // --- Formulario ---
  public form = this.fb.group({
    vehiculoId: ['', [Validators.required]],
    kilometrosRecorridos: ['', [Validators.required, Validators.min(1)]],
    litrosConsumidos: ['', [Validators.required, Validators.min(0.1), Validators.max(120)]],
    
  });

  constructor() {
    // Efecto: Reacciona cuando cambia el viaje activo (desde el padre/localStorage)
    effect(() => {
      const viaje = this.viajeActivo();
      
      if (viaje) {
        // Preparamos los datos para el formulario
        const patchData: any = { 
          vehiculoId: viaje.id.toString() 
        };

        if (viaje.distanciaEstimada) {
          patchData.kilometrosRecorridos = viaje.distanciaEstimada;
        }

        this.form.patchValue(patchData);
        this.isManualMode.set(false); // Apagamos modo manual
      }
    });
  }

  ngOnInit() {
    this.vehiculoService.loadVehiculos();
  }

  // --- Métodos de UI ---
  activarManual() {
    this.isManualMode.set(true);
    this.form.reset();
  }

  cancelar() {
    this.isManualMode.set(false);
    this.onViajeTerminado.emit(); 
    this.form.reset();
  }

  irAAsistente() {
    this.onSolicitarIA.emit();
  }

  // --- Envío ---
  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.alert.warning('Completa todos los datos.');
      return;
    }

    // Defensa: Si hay viaje activo, aseguramos el ID aunque el input esté oculto
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
        
        // Avisamos al padre que ya acabamos para que limpie localStorage
        this.onViajeTerminado.emit();
      },
      error: (err) => {
        console.error(err);
        this.alert.error('Error al registrar.');
      }
    });
  }
}