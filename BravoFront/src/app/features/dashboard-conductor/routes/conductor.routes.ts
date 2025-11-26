import { Routes } from '@angular/router';
import { ConductorLayoutComponent } from '../layout/conductor-layout/conductor-layout.component';

export const CONDUCTOR_ROUTES: Routes = [
  {
    path: '', // ruta base del dashboard conductor
    component: ConductorLayoutComponent, // layout principal con menu y header
    children: [
      {
        path: 'registrar', // pagina para registrar uso de vehiculo
        loadComponent: () =>
          import('../pages/registrar-uso/registrar-uso.component')
            .then(m => m.RegistrarUsoComponent) // carga bajo demanda
      },
      {
        path: '', // si la ruta esta vacia redirige a registrar
        redirectTo: 'registrar',
        pathMatch: 'full' // solo redirige si la ruta exacta es vacia
      }
    ]
  }
];
