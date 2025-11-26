import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router'; 
import { FormsModule } from '@angular/forms';
import { AlertService } from '../../../../../shared/services/alert.service';
import { VehiculoService } from '../../../services/vehiculo.service';
import { SemaforoBadgeComponent } from '../../../components/semaforo-badge/semaforo-badge.component';
import { ReporteMantenimiento } from '../../../../../core/models/vehiculo.model';
import { SearchBar } from '../../../../../shared/components/search-bar/search-bar.component';

@Component({
  selector: 'app-vehiculos-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, SemaforoBadgeComponent, SearchBar],
  templateUrl: './vehiculos-list.component.html',
  styleUrls: ['./vehiculos-list.component.scss']
})
export class VehiculosListComponent implements OnInit {
  // Inyectamos servicios necesarios
  private vehiculoService = inject(VehiculoService);
  private alertService = inject(AlertService);
  private router = inject(Router);

  // --- Signals para controlar estado local ---
  public searchTerm = signal<string>(''); // Busqueda por placa, marca o modelo
  public viewMode = signal<'list' | 'grid'>('list'); // Vista lista o grid
  public isStatusModalOpen = signal(false); // Modal de estado abierto?
  public selectedStatus = signal<ReporteMantenimiento | null>(null); // Reporte seleccionado
  public selectedVehiculoId = signal<number | null>(null); // Id del vehiculo seleccionado

  // Computed para filtrar la lista segun searchTerm
  public vehiculosFiltrados = computed(() => {
    const term = this.searchTerm().toLowerCase().trim();
    const lista = this.vehiculoService.vehiculos();

    if (!term) return lista; // Si no hay termino, devolvemos todos

    return lista.filter(v =>
      v.placa?.toLowerCase().includes(term) ||
      v.marca?.toLowerCase().includes(term) ||
      v.modelo?.toLowerCase().includes(term)
    );
  });

  ngOnInit() {
    // Cargamos vehiculos al iniciar
    this.vehiculoService.loadVehiculos();
  }

  // Cerrar modal de estado
  closeStatusModal() {
    this.isStatusModalOpen.set(false);
    this.selectedStatus.set(null);
    this.selectedVehiculoId.set(null);
  }

  // Consultar estado de un vehiculo y mostrar modal
  checkStatus(id: number) {
    this.vehiculoService.getEstatusServicio(id).subscribe({
      next: (res) => {
        let type: 'success' | 'warning' | 'failure' = 'success';
        if (res.estatus === 2) type = 'failure'; // Rojo
        if (res.estatus === 1) type = 'warning'; // Amarillo

        // Mostramos alerta tipo modal con info
        this.alertService.report(
          'Estatus del Vehiculo',
          `${res.mensaje} <br><br> <strong>Km Restantes:</strong> ${res.kmRestantes}`,
          type
        );
      }
    });
  }

  // --- ELIMINAR VEHICULO ---
  async deleteVehiculo(id: number) {
    const confirmado = await this.alertService.confirm(
      'Eliminar Vehiculo',
      'Estas seguro de eliminar este vehiculo? Esta accion no se puede deshacer.',
      'Si, Eliminar',
      'Cancelar'
    );

    if (confirmado) {
      this.vehiculoService.deleteVehiculo(id).subscribe({
        next: () => this.alertService.success('Vehiculo eliminado correctamente'),
        error: () => this.alertService.error('No se pudo eliminar el vehiculo')
      });
    }
  }

  // Enviar vehiculo al taller
  sendToMaintenance() {
    const id = this.selectedVehiculoId();
    if (!id) return;

    this.alertService.confirm(
      'Enviar a Taller',
      'Deseas marcar este vehiculo como "En Mantenimiento"?',
      'Si, enviar', 'Cancelar'
    ).then(confirmado => {
      if (confirmado) {
        this.vehiculoService.enviarATaller(id).subscribe({
          next: () => {
            this.alertService.success('El vehiculo ahora esta en el taller.');
            this.closeStatusModal(); 
          },
          error: () => this.alertService.error('No se pudo actualizar el estado.')
        });
      }
    });
  }

  // Editar vehiculo: navega a la ruta de edicion
  editVehiculo(id: number) {
    this.router.navigate(['/gerente/vehiculos/editar', id]);
  }

  // Cambiar modo de vista lista o grid
  setViewMode(mode: 'list' | 'grid') {
    this.viewMode.set(mode);
  }
}
