import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-kpi-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './kpi-card.component.html',
  styleUrls: ['./kpi-card.component.scss']
})
export class KpiCardComponent {
    // Titulo que se muestra en la tarjeta
    @Input({ required: true }) title!: string;

    // Valor numerico principal que se muestra
    @Input({ required: true }) value!: number;

    // Icono que se muestra en la tarjeta (opcional)
    @Input() iconName: 'car' | 'check' | 'tool' | 'money' = 'car';

    // Variante de color de la tarjeta
    @Input() variant: 'purple' | 'green' | 'red' | 'blue' = 'purple';
}
