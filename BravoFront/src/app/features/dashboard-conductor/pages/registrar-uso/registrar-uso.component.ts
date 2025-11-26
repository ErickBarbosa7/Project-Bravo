import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AlertService } from '../../../../shared/services/alert.service';
import { VehiculoService } from '../../../dashboard-gerente/services/vehiculo.service';
import { ConductorService } from '../../services/conductor.service';

@Component({
  selector: 'app-registrar-uso',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './registrar-uso.component.html',
  styleUrls: ['./registrar-uso.component.scss']
})
export class RegistrarUsoComponent implements OnInit {
    // Inyeccion de dependencias
    private fb = inject(FormBuilder);
    private vehiculoService = inject(VehiculoService);
    private conductorService = inject(ConductorService);
    private alert = inject(AlertService);
    
    // Señales / observables para UI
    vehiculos = this.vehiculoService.vehiculos; // lista de vehiculos disponibles
    isLoading = this.conductorService.loading; // indicador de carga

    // Formulario reactivo
    form = this.fb.group({
        vehiculoId: ['', [Validators.required]],
        kilometrosRecorridos: ['', [Validators.required, Validators.min(1)]],
        litrosConsumidos: ['', [Validators.required, Validators.min(0.1)]]
    });

    ngOnInit(): void {
        // Cargar vehículos al iniciar el componente
        this.vehiculoService.loadVehiculos();
    }

    // Metodo para enviar formulario
    onSubmit() {
        if (this.form.invalid) {
            this.form.markAllAsTouched(); // marca todos los campos para mostrar errores
            this.alert.warning('Completa todos los datos');
            return;
        }

        // Preparamos los datos en numeros
        const data = {
            vehiculoId: Number(this.form.value.vehiculoId),
            kilometrosRecorridos: Number(this.form.value.kilometrosRecorridos),
            litrosConsumidos: Number(this.form.value.litrosConsumidos)
        };

        // Llamada al servicio para registrar uso
        this.conductorService.registrarUso(data).subscribe({
            next: () => {
                this.alert.success('Carga registrada correctamente.');
                this.form.reset(); // resetear formulario
                // Opcional: recargar vehiculos para actualizar kilometraje
                this.vehiculoService.loadVehiculos();
            },
            error: (err) => {
                console.error(err);
                this.alert.error('Error al registrar. Intenta de nuevo.');
            }
        });
    }
}
