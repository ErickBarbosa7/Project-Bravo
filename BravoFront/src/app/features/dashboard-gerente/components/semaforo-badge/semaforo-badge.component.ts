import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-semaforo-badge',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './semaforo-badge.component.html',
  styleUrls: ['./semaforo-badge.component.scss']
})
export class SemaforoBadgeComponent {
  
  // Input obligatorio para indicar el estado del semaforo
  @Input({ required: true }) estado!: number | string; // permitimos numero o texto por seguridad

  // Computed que devuelve la configuracion de la tarjeta segun el estado
  get config() {
    // Convertimos a numero para manejar valores como "0" o 0
    // Si viene un string que no sea numerico, ira al default
    const estadoNumero = Number(this.estado);

    // switch principal por numero
    switch (estadoNumero) {
      case 0: return { label: 'Disponible', class: 'green', icon: '●' };
      case 1: return { label: 'En Ruta', class: 'blue', icon: '➤' };
      case 2: return { label: 'En Taller', class: 'orange', icon: '🔧' };
      case 3: return { label: 'Servicio Urgente', class: 'red', icon: '⚠️' };
      default: return this.handleStringState(this.estado); // si no es numero, revisamos si es string
    }
  }

  // Helper para cuando el backend manda strings como "Disponible" en vez de numeros
  private handleStringState(val: any) {
    const texto = String(val).toLowerCase(); // lowercase para comparar sin errores
    if (texto === 'disponible') return { label: 'Disponible', class: 'green', icon: '●' };
    if (texto === 'enruta') return { label: 'En Ruta', class: 'blue', icon: '➤' };
    if (texto === 'entaller') return { label: 'En Taller', class: 'orange', icon: '🔧' };
    if (texto === 'necesitaservicio') return { label: 'Servicio Urgente', class: 'red', icon: '⚠️' };
    
    // fallback por si no coincide ningun estado
    return { label: 'Desconocido', class: 'gray', icon: '?' };
  }
}
