import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AlertService } from '../../../../shared/services/alert.service';

// Importamos los dos hijos
import { RecomendarComponent } from './components/recomendar/recomendar.component';
import { FormularioCargaComponent } from './components/formulario-carga/formulario-carga.component';

@Component({
  selector: 'app-registrar-uso',
  standalone: true,
  imports: [CommonModule, RecomendarComponent, FormularioCargaComponent],
  templateUrl: './registrar-uso.component.html',
  styleUrls: ['./registrar-uso.component.scss']
})
export class RegistrarUsoComponent implements OnInit {
  private alert = inject(AlertService);

  // Estado UI
  activeTab = signal<'recomendar' | 'registrar'>('registrar');
  
  // Datos del viaje activo (Persistencia)
  viajeEnCurso = signal<any>(null);

  ngOnInit() {
    // Recuperar sesión al recargar página
    const viajeGuardado = localStorage.getItem('viaje_activo');
    if (viajeGuardado) {
      this.viajeEnCurso.set(JSON.parse(viajeGuardado));
      this.activeTab.set('registrar');
    }
  }

  // Ahora recibimos un objeto compuesto: { vehiculo, distancia }
  iniciarViaje(evento: { vehiculo: any, distancia: number }) {
    
    const vehiculo = evento.vehiculo;
    const kmEstimados = evento.distancia;

    const datosViaje = {
      id: vehiculo.vehiculoId,
      modelo: vehiculo.modelo,
      placa: vehiculo.placa,
      inicio: new Date(),
      distanciaEstimada: kmEstimados 
    };

    localStorage.setItem('viaje_activo', JSON.stringify(datosViaje));
    this.viajeEnCurso.set(datosViaje);
    
    this.alert.success(`Viaje iniciado con ${vehiculo.modelo}`);
    this.activeTab.set('registrar');
  }

  // Cuando el hijo "Formulario" termina o cancela
  limpiarViaje() {
    localStorage.removeItem('viaje_activo');
    this.viajeEnCurso.set(null);
  }

  // Cuando el hijo solicita cambiar de tab
  cambiarTab(tab: 'recomendar' | 'registrar') {
    this.activeTab.set(tab);
  }
}