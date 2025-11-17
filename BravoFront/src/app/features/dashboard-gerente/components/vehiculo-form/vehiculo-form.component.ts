import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, ActivatedRoute } from '@angular/router'; // <== ActivatedRoute
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { finalize } from 'rxjs';

// Imports de Lógica
import { VehiculoService } from '../../services/vehiculo.service';
import { CreateVehiculoDto, UpdateVehiculoDto } from '../../../../core/models/vehiculo.model';
import { NotyfService } from '../../../../shared/services/notyf.service';

@Component({
  selector: 'app-vehiculo-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './vehiculo-form.component.html',
  styleUrl: './vehiculo-form.component.scss'
})
export class VehiculoFormComponent implements OnInit {

  // --- Inyección de Dependencias ---
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute); // Para leer el ID de la URL
  private vehiculoService = inject(VehiculoService);
  private notyf = inject(NotyfService);

  // --- Signals de Estado Local ---
  public isLoading = signal(false);
  public editMode = signal(false);
  private vehiculoId = signal<number | null>(null);

  // --- Formulario ---
  public vehiculoForm!: FormGroup; // Lo inicializamos en ngOnInit

  ngOnInit(): void {
    this.initForm();
    this.checkRouteParams();
  }

  // Inicializa el formulario vacío
  private initForm(): void {
    this.vehiculoForm = this.fb.group({
      // Validaciones del lado del cliente (para feedback instantáneo)
      // Tu backend tiene la validación final
      placa: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(10)]],
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      marca: [''],
      modelo: [''],
      año: [null, [Validators.min(1990), Validators.max(new Date().getFullYear() + 1)]], // Max año actual + 1
      kilometrajeActual: [0, [Validators.required, Validators.min(0)]],
      intervaloServicioKm: [10000, [Validators.required, Validators.min(100)]]
    });
  }

  // Revisa la URL para ver si estamos en modo "Crear" o "Editar"
  private checkRouteParams(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      
      if (id) {
        // --- MODO EDITAR ---
        this.editMode.set(true);
        this.vehiculoId.set(+id); // Convierte el string 'id' a número
        this.isLoading.set(true);
        
        // Pedimos el vehículo a la API
        this.vehiculoService.getVehiculoById(+id)
          .pipe(finalize(() => this.isLoading.set(false)))
          .subscribe(vehiculo => {
            // Llenamos el formulario con los datos
            this.vehiculoForm.patchValue(vehiculo);
          });
      }
      // else: MODO CREAR (el formulario ya está vacío)
    });
  }

  // --- Método de Envío ---
  onSubmit(): void {
    if (this.vehiculoForm.invalid) {
      this.vehiculoForm.markAllAsTouched(); // Muestra todos los errores
      this.notyf.error('El formulario tiene errores. Por favor, revísalo.');
      return;
    }

    this.isLoading.set(true);

    if (this.editMode()) {
      // --- Lógica de ACTUALIZAR (PUT) ---
      const dto: UpdateVehiculoDto = {
        ...this.vehiculoForm.value,
        id: this.vehiculoId()!
      };
      
      this.vehiculoService.updateVehiculo(this.vehiculoId()!, dto)
        .pipe(finalize(() => this.isLoading.set(false)))
        .subscribe({
          next: () => this.router.navigate(['/gerente/vehiculos']),
          // (El servicio ya maneja los toasts de error)
        });

    } else {
      // --- Lógica de CREAR (POST) ---
      const dto: CreateVehiculoDto = this.vehiculoForm.value;
      
      this.vehiculoService.createVehiculo(dto)
        .pipe(finalize(() => this.isLoading.set(false)))
        .subscribe({
          next: () => this.router.navigate(['/gerente/vehiculos']),
          // (El servicio ya maneja los toasts de error)
        });
    }
  }
}