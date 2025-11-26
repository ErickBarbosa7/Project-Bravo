import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-search-bar',
  imports: [FormsModule],              
  templateUrl: './search-bar.component.html',
  styleUrl: './search-bar.component.scss',
})
export class SearchBar {
  // Valor actual del input, viene del componente padre
  @Input() value: string = '';

  // Evento que emite cuando el valor cambia, para que el padre lo capture
  @Output() valueChange = new EventEmitter<string>();

  // Metodo para actualizar el valor y avisar al padre
  updateValue(newValue: string) {
    this.valueChange.emit(newValue);
  }
}
