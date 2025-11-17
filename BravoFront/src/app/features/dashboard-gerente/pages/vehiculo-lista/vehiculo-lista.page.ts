import { Component, inject, signal } from '@angular/core';
import { NgClass } from '@angular/common';
import { Router } from '@angular/router';
import { VehiculoService } from '../../services/vehiculo.service';
import { APP_PIPES } from '../../../../shared/pipes';
import { VehiculoDetalleComponent } from '../../components/vehiculo-detalle/vehiculo-detalle.component';

@Component({
  selector: 'app-vehiculos-lista-page',
  standalone: true,
  imports: [NgClass, ...APP_PIPES,VehiculoDetalleComponent],
  templateUrl: './vehiculo-lista.page.html',
  styleUrls: ['./vehiculo-lista.page.scss'],
})
export class VehiculosListaPage {
  vehiculoService = inject(VehiculoService);
  router = inject(Router);
  selectedVehiculo = signal<number | null>(null);

  
  constructor() {
    // Carga automática al entrar
    this.vehiculoService.loadVehiculos();
  }

  openDetalle(id: number) {
    this.selectedVehiculo.set(id);
  }

  closeDetalle() {
    this.selectedVehiculo.set(null);
  }

  onEditVehiculo(id: number) {
    this.router.navigate(['/dashboard-gerente/vehiculos/editar', id]);
  }

  onDeleteVehiculo(vehiculo: any) {
    const confirmar = confirm(`¿Eliminar el vehículo ${vehiculo.placa}?`);

    if (confirmar) {
      this.vehiculoService.deleteVehiculo(vehiculo.id);
    }
  }
}
