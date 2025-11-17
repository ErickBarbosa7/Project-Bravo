import { Component } from '@angular/core';
import { VehiculoFormComponent } from '../../components/vehiculo-form/vehiculo-form.component';

@Component({
  selector: 'app-vehiculos-crear-page',
  standalone: true,
  imports: [VehiculoFormComponent],
  templateUrl: './vehiculos-crear.page.html',
  styleUrls: ['./vehiculos-crear.page.scss'],
})
export class VehiculosCrearPage {}
