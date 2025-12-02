import { Component, EventEmitter, Input, Output, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.scss']
})
export class ModalComponent {

  // Titulo que se muestra en la parte de arriba del modal
  @Input() title: string = '';

  // Controla si el modal muestra o no el footer con botones
  @Input() showFooter: boolean = true;

  // Evento que le avisa al componente padre que el modal se cerro
  @Output() onClose = new EventEmitter<void>();

  // Funcion que dispara el evento para cerrar el modal
  close() {
    this.onClose.emit();
  }

  // Escucha la tecla ESC en toda la pagina
  // Si el usuario presiona ESC, se cierra el modal
  @HostListener('document:keydown.escape')
  onKeydownHandler() {
    this.close();
  }
}
