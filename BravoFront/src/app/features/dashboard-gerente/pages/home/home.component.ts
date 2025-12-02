import { Component, inject, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { VehiculoService } from '../../../../core/services/vehiculo.service';
import { KpiCardComponent } from '../../components/kpi-card/kpi-card.component';
import { SemaforoBadgeComponent } from '../../components/semaforo-badge/semaforo-badge.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, KpiCardComponent, SemaforoBadgeComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

  // Accedemos al servicio que maneja toda la info de vehiculos
  private vehiculoService = inject(VehiculoService);

  // Signal con la lista de vehiculos cargada desde el servicio
  public vehiculos = this.vehiculoService.vehiculos;

  // --- KPIs DEL DASHBOARD ---

  // Total de vehiculos registrados en el sistema
  public totalFlota = computed(() => this.vehiculos().length);

  // Vehiculos que estan en servicio o taller
  public autosEnServicio = computed(() =>
    this.vehiculos().filter(v =>
      v.estado === 2 ||            // En mantenimiento
      v.estado === 3 ||            // En taller
      v.estado.toString() === 'EnTaller' ||
      v.estado.toString() === 'NecesitaServicio'
    ).length
  );

  // Vehiculos que estan disponibles para circular
  public autosDisponibles = computed(() =>
    this.vehiculos().filter(v =>
      v.estado === 0 ||            // Disponible
      v.estado.toString() === 'Disponible'
    ).length
  );

  // Lista corta de vehiculos con algun tipo de alerta
  // Solo mostramos los primeros 5 para el widget
  public vehiculosEnAlerta = computed(() =>
    this.vehiculos()
      .filter(v => v.estado !== 0) // Todo lo que no sea disponible
      .slice(0, 5)
  );

  // Cargamos los vehiculos cuando el componente arranca
  ngOnInit() {
    this.vehiculoService.loadVehiculos();
  }
}
