import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.scss']
})
export class ModalComponent {
  // Título que aparece arriba
  @Input() title: string = '';
  
  // Si quieres ocultar los botones de abajo (opcional)
  @Input() showFooter: boolean = true;

  // Evento para avisar al padre que debe cerrarse
  @Output() onClose = new EventEmitter<void>();

  close() {
    this.onClose.emit();
  }
}