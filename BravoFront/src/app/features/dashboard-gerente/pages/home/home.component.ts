import { Component, inject, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { VehiculoService } from '../../services/vehiculo.service';
import { KpiCardComponent } from '../../components/kpi-card/kpi-card.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, KpiCardComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  // Inyectamos el servicio de vehiculos para acceder a la flota
  private vehiculoService = inject(VehiculoService);

  // Señal que contiene la lista de vehiculos
  public vehiculos = this.vehiculoService.vehiculos;

  // --- KPIs calculados usando computed ---
  
  // Total de autos en la flota
  public totalFlota = computed(() => this.vehiculos().length);
  
  // Autos que estan en mantenimiento o necesitan servicio
  public autosEnServicio = computed(() => 
    this.vehiculos().filter(v => v.estado === 2 || v.estado === 3).length
  );

  // Autos disponibles para uso
  public autosDisponibles = computed(() => 
    this.vehiculos().filter(v => v.estado === 0).length
  );

  ngOnInit() {
    // Si la lista de vehiculos esta vacia, cargamos los datos del backend
    if (this.vehiculos().length === 0) {
      this.vehiculoService.loadVehiculos();
    }
  }
}
