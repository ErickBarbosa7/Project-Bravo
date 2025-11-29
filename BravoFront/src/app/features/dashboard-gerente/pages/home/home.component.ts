import { Component, inject, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router'; // Importar esto
import { VehiculoService } from '../../../../core/services/vehiculo.service';
import { KpiCardComponent } from '../../components/kpi-card/kpi-card.component';
import { SemaforoBadgeComponent } from '../../components/semaforo-badge/semaforo-badge.component'; // Reutilizamos el badge

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, KpiCardComponent, SemaforoBadgeComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  private vehiculoService = inject(VehiculoService);

  public vehiculos = this.vehiculoService.vehiculos;

  // KPIs (Ya los tenías)
  public totalFlota = computed(() => this.vehiculos().length);
  
  public autosEnServicio = computed(() => 
    this.vehiculos().filter(v => 
      v.estado === 2 || v.estado === 3 || 
      v.estado.toString() === 'EnTaller' || 
      v.estado.toString() === 'NecesitaServicio'
    ).length
  );

  public autosDisponibles = computed(() => 
    this.vehiculos().filter(v => 
      v.estado === 0 || v.estado.toString() === 'Disponible'
    ).length
  );

  // --- NUEVO: LISTA DE ALERTAS ---
  // Filtramos los autos que NO están disponibles (Rojos, Amarillos, Taller)
  // Tomamos solo los primeros 5 para no saturar
  public vehiculosEnAlerta = computed(() => 
    this.vehiculos()
      .filter(v => v.estado !== 0) // Todo lo que no sea verde
      .slice(0, 5)
  );

  ngOnInit() {
    this.vehiculoService.loadVehiculos();
  }
}