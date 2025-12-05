import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AlertService } from '../../../../../shared/services/alert.service';
import { ReporteMantenimiento } from '../../../../../core/models/vehiculo.model';
import { VehiculoService } from '../../../../../core/services/vehiculo.service';
import { SearchBar } from '../../../../../shared/components/search-bar/search-bar.component';
import { ModalComponent } from '../../../../../shared/ui/modal/modal.component';
import { SemaforoBadgeComponent } from '../../../components/semaforo-badge/semaforo-badge.component';

// 🔹 Enum equivalente al backend
export enum EstadoVehiculo {
  Disponible = 0,
  EnRuta = 1,
  EnTaller = 2,
  NecesitaServicio = 3
}

@Component({
  selector: 'app-vehiculos-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    FormsModule,
    SemaforoBadgeComponent,
    ModalComponent,
    SearchBar
  ],
  templateUrl: './vehiculos-list.component.html',
  styleUrls: ['./vehiculos-list.component.scss']
})
export class VehiculosListComponent implements OnInit {

  private vehiculoService = inject(VehiculoService);
  private alertService = inject(AlertService);
  private router = inject(Router);

  // Texto que escribe el usuario en la barra de busqueda
  public searchTerm = signal<string>('');

  // Controla si se ve la vista tipo lista o tipo cuadricula
  public viewMode = signal<'list' | 'grid'>('list');
    public EstadoVehiculo = EstadoVehiculo;
  // Filtra los vehiculos en base al texto que busca el usuario
  public vehiculosFiltrados = computed(() => {
    const term = this.searchTerm().toLowerCase();
    const lista = this.vehiculoService.vehiculos();

    if (!term) return lista;

    return lista.filter(v => 
      v.placa.toLowerCase().includes(term) ||
      v.modelo.toLowerCase().includes(term)
    );
  });

  // Control general del modal
  public modalOpen = signal(false);
  public modalType = signal<'status' | 'payment' | 'delete' | 'edit' | null>(null);

  // Vehiculo seleccionado para cualquier accion
  public selectedVehiculoId = signal<number | null>(null);

  // Info del semaforo (estatus de mantenimiento)
  public selectedStatus = signal<ReporteMantenimiento | null>(null);

  // Monto para el modal de pagos
  public paymentAmount = signal<number | null>(null);

  ngOnInit() {
    // Carga toda la lista al entrar en la vista
    this.vehiculoService.loadVehiculos();
  }

  // Abre un modal para mostrar el semaforo del vehiculo
  checkStatus(id: number) {
    this.selectedVehiculoId.set(id);
    this.selectedStatus.set(null);

    this.modalType.set('status');
    this.modalOpen.set(true);

    this.vehiculoService.getEstatusServicio(id).subscribe({
      next: (res) => this.selectedStatus.set(res),
      error: () => {
        this.closeModal();
        this.alertService.error('No se pudo obtener el estatus');
      }
    });
  }

  // Marca el vehiculo como "En Taller"
  sendToMaintenance() {
    const id = this.selectedVehiculoId();
    if (!id) return;

    this.closeModal();

    setTimeout(() => {
      this.alertService
        .confirm(
          'Enviar a Taller',
          'Deseas cambiar el estado del vehiculo a En Mantenimiento?',
          'Enviar',
          'Cancelar'
        )
        .then(confirmado => {
          if (confirmado) {
            this.vehiculoService.enviarATaller(id).subscribe({
              next: () => this.alertService.success('Vehiculo enviado al taller'),
              error: () => this.alertService.error('No se pudo actualizar')
            });
          }
        });
    }, 200);
  }

  // Abre modal para capturar pago
  payService() {
    const id = this.selectedVehiculoId();
    if (!id) return;

    this.closeModal();

    setTimeout(() => {
      this.paymentAmount.set(null);
      this.modalType.set('payment');
      this.modalOpen.set(true);

      this.selectedVehiculoId.set(id);
    }, 200);
  }

  // Registra el pago y cambia el vehiculo a estado disponible
  confirmPayment() {
    const id = this.selectedVehiculoId();
    const monto = this.paymentAmount();

    if (!id || !monto || monto <= 0) {
      this.alertService.warning('Ingresa un monto valido');
      return;
    }

    const dto = {
      vehiculoId: id,
      monto: monto,
      concepto: 'Mantenimiento Correctivo'
    };

    this.vehiculoService.simularPago(dto).subscribe({
      next: () => {
        this.alertService.success('Pago aplicado correctamente');
        this.closeModal();
        this.vehiculoService.loadVehiculos();
      },
      error: () => this.alertService.error('No se pudo registrar el pago')
    });
  }

  // Cierra cualquier modal abierto
  closeModal() {
    this.modalOpen.set(false);
    this.modalType.set(null);
  }

  // Elimina un vehiculo con confirmacion
  async deleteVehiculo(id: number) {
    const confirmado = await this.alertService.confirm(
      'Eliminar Vehiculo',
      'Esta accion no se puede deshacer',
      'Eliminar',
      'Cancelar'
    );

    if (confirmado) {
      this.vehiculoService.deleteVehiculo(id).subscribe({
        next: () => this.alertService.success('Vehiculo eliminado'),
        error: () => this.alertService.error('No se pudo eliminar')
      });
    }
  }

  // Redirige al formulario de edicion
  editVehiculo(id: number) {
    this.router.navigate(['/gerente/vehiculos/editar', id]);
  }

  // Metodo para obtener el estado del vehículo seleccionado
  getSelectedVehiculoEstado(): number {
    const id = this.selectedVehiculoId();
    if (!id) return -1; // Retorno seguro si no hay ID

    const vehiculo = this.vehiculoService.vehiculos().find(v => v.id === id);
    if (!vehiculo) return -1;

    const estado = vehiculo.estado;

    // Si ya es un número, lo devolvemos directo
    if (typeof estado === 'number') return estado;

    // Si es texto, lo traducimos al Enum numérico
    const estadoStr = String(estado).toLowerCase();
    switch (estadoStr) {
      case 'disponible': return 0;       // EstadoVehiculo.Disponible
      case 'enruta': return 1;           // EstadoVehiculo.EnRuta
      case 'entaller': return 2;         // EstadoVehiculo.EnTaller
      case 'necesitaservicio': return 3; // EstadoVehiculo.NecesitaServicio
      default: 
        // Por si el backend manda "2" como string
        const num = Number(estado);
        return isNaN(num) ? 0 : num;
    }
  }

  // Cambia la vista entre lista y cuadricula
  setViewMode(mode: 'list' | 'grid') {
    this.viewMode.set(mode);
  }
}
