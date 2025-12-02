import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { CreateVehiculoDto, UpdateVehiculoDto } from '../../../../../core/models/vehiculo.model';
import { AlertService } from '../../../../../shared/services/alert.service';
import { VehiculoService } from '../../../../../core/services/vehiculo.service';

@Component({
  selector: 'app-vehiculos-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './vehiculos-form.component.html',
  styleUrls: ['./vehiculos-form.component.scss']
})
export class VehiculosFormComponent implements OnInit {
  
  // --- Inyecciones de servicios ---
  private fb = inject(FormBuilder);
  private vehiculoService = inject(VehiculoService);
  private alertService = inject(AlertService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  // --- Signals de estado ---
  public isLoading = signal(false);      // Indica si estamos cargando datos
  public isEditMode = signal(false);     // Saber si es edicion o creacion
  public currentId = signal<number | null>(null); // ID del vehiculo en edicion

  // --- Formulario de vehiculo ---
  public vehicleForm: FormGroup = this.fb.group({
    placa: ['', [Validators.required, Validators.minLength(6)]],
    marca: ['', [Validators.required]],
    modelo: ['', [Validators.required]],
    anio: [new Date().getFullYear(), [Validators.required, Validators.min(1990), Validators.max(new Date().getFullYear() + 1)]],
    fotoUrl: [''], 
    kilometrajeActual: [0, [Validators.required, Validators.min(0)]],
    intervaloServicioKm: [10000, [Validators.required, Validators.min(1000)]]
  });

  ngOnInit(): void {
    // Revisamos si la ruta tiene ID (editar)
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode.set(true);
      this.currentId.set(Number(idParam));
      this.loadVehicleData(Number(idParam));
    }
  }

  // Cargar datos existentes para editar
  loadVehicleData(id: number) {
    this.isLoading.set(true);
    this.vehiculoService.getVehiculoById(id).subscribe({
      next: (vehiculo) => {
        // Rellenamos formulario con datos del vehiculo
        this.vehicleForm.patchValue({
          placa: vehiculo.placa,
          marca: vehiculo.marca,
          modelo: vehiculo.modelo,
          anio: vehiculo.anio,
          fotoUrl: vehiculo.fotoUrl,
          kilometrajeActual: vehiculo.kilometrajeActual,
          intervaloServicioKm: vehiculo.intervaloServicioKm
        });
        this.isLoading.set(false);
      },
      error: () => {
        // Si falla, mostramos error y regresamos a lista
        this.alertService.error('No se pudo cargar el vehiculo.');
        this.router.navigate(['/gerente/vehiculos']);
      }
    });
  }

  // Manejo del submit
  onSubmit() {
    if (this.vehicleForm.invalid) {
      this.vehicleForm.markAllAsTouched();
      this.alertService.warning('Revisa los campos marcados.');
      return;
    }

    this.isLoading.set(true);
    const formValue = this.vehicleForm.value;

    // Dependiendo de si es edicion o creacion llamamos al metodo correspondiente
    if (this.isEditMode()) {
      this.update(formValue);
    } else {
      this.create(formValue);
    }
  }

  // Crear vehiculo
  private create(dto: CreateVehiculoDto) {
    this.vehiculoService.createVehiculo(dto)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          this.alertService.success('Vehiculo creado exitosamente');
          this.router.navigate(['/gerente/vehiculos']);
        },
        error: (err) => {
          console.error(err);
          this.alertService.error('Error al crear vehiculo. Verifica la placa.');
        }
      });
  }

  // Actualizar vehiculo
  private update(dto: UpdateVehiculoDto) {
    const id = this.currentId();
    if (!id) return;

    this.vehiculoService.updateVehiculo(id, dto)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          this.alertService.success('Vehiculo actualizado correctamente');
          this.router.navigate(['/gerente/vehiculos']);
        },
        error: (err) => {
          console.error(err);
          this.alertService.error('Error al actualizar vehiculo.');
        }
      });
  }

  
  // Validacion de campos individuales 
  isFieldInvalid(fieldName: string): boolean {
    const field = this.vehicleForm.get(fieldName);
    return !!(field?.invalid && (field?.touched || field?.dirty));
  }
}
