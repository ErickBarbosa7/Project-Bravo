import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { VehiculoService } from '../../../../../../core/services/vehiculo.service';

// Interfaz para la proyeccion de gastos
interface ProyeccionDto {
  costoPromedioPorKm: number;
  kmRecorridosUltimoMes: number;
  presupuestoSugerido: number;
  mensaje: string;
}

@Component({
  selector: 'app-proyeccion-gastos',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './proyeccion-gastos.component.html',
  styleUrls: ['./proyeccion-gastos.component.scss']
})
export class ProyeccionGastosComponent implements OnInit {
  // Inyectamos el servicio de vehiculos
  private vehiculoService = inject(VehiculoService);

  // Signal para almacenar los datos de la proyeccion
  data = signal<ProyeccionDto | null>(null);

  // Signal para indicar si esta cargando
  loading = signal(true);

  ngOnInit() {
    // Llamamos al servicio para obtener la proyeccion de gastos
    this.vehiculoService.getProyeccionGastos().subscribe({
      next: (res) => {
        this.data.set(res);   // Guardamos los datos recibidos
        this.loading.set(false); // Terminamos de cargar
      },
      error: () => this.loading.set(false) // Si falla, dejamos de mostrar loading
    });
  }
}
