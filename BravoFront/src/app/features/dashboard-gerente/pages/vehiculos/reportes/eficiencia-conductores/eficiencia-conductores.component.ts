import { Component, inject, OnInit, signal } from '@angular/core';
import { AlertService } from '../../../../../../shared/services/alert.service';
import { CommonModule } from '@angular/common';
import { ConductorService } from '../../../../../../core/services/conductor.service';

@Component({
  selector: 'app-eficiencia-conductores',
  imports: [CommonModule],
  templateUrl: './eficiencia-conductores.component.html',
  styleUrls: ['./eficiencia-conductores.component.scss'], 
})
export class EficienciaConductoresComponent implements OnInit {
  // Inyectamos servicios
  private conductorService = inject(ConductorService);
  private alert = inject(AlertService);

  // Signals para almacenar la lista de conductores y reportes
  conductores = signal<any[]>([]);
  selectedConductorId = signal<string | null>(null);
  reporteData = signal<any>(null);

  // Signals de carga
  isLoadingList = signal(true);
  isLoadingReport = signal(false);

  ngOnInit() {
    // Cargamos los conductores al iniciar
    this.cargarConductores();
  }

  // Metodo para obtener todos los conductores
  cargarConductores() {
    this.conductorService.getListaConductores().subscribe({
      next: (data) => {
        this.conductores.set(data);       // Guardamos los conductores
        this.isLoadingList.set(false);    // Ya no estamos cargando
      },
      error: () => {
        this.alert.error('Error al cargar conductores'); // Mostramos error
        this.isLoadingList.set(false);
      }
    });
  }

  // Selecciona un conductor y carga su reporte
  selectConductor(id: string) {
    this.selectedConductorId.set(id);
    this.isLoadingReport.set(true);
    this.reporteData.set(null); // Limpiamos reporte anterior

    this.conductorService.getReporteIndividual(id).subscribe({
      next: (data) => {
        this.reporteData.set(data);     // Guardamos reporte recibido
        this.isLoadingReport.set(false);
      },
      error: () => {
        this.alert.error('No se pudo cargar el reporte'); // Mensaje si falla
        this.isLoadingReport.set(false);
      }
    });
  }
}
