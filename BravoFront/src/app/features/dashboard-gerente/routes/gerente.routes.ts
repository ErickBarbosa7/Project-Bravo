import { Routes } from '@angular/router';
import { GerenteLayoutComponent } from '../layout/gerente-layout/gerente-layout.component';

export const GERENTE_ROUTES: Routes = [
  {
    path: '',
    component: GerenteLayoutComponent, // Layout principal para gerente
    children: [
      {
        path: 'home',
        loadComponent: () => import('./../pages/home/home.component').then(m => m.HomeComponent) // Home del dashboard
      },
      {
        path: 'vehiculos',
        loadComponent: () => import('./../pages/vehiculos/vehiculos-list/vehiculos-list.component').then(m => m.VehiculosListComponent) // Lista de vehiculos
      },
      {
        path: 'vehiculos/nuevo',
        loadComponent: () => import('../pages/vehiculos/vehiculos-form/vehiculos-form.component').then(m => m.VehiculosFormComponent) // Formulario para crear nuevo vehiculo
      },
      {
        path: 'vehiculos/editar/:id',
        loadComponent: () => import('../pages/vehiculos/vehiculos-form/vehiculos-form.component').then(m => m.VehiculosFormComponent) // Formulario para editar vehiculo existente
      },
      {
        path: 'reportes/proyeccion',
        loadComponent: () => import('./../pages/vehiculos/reportes/proyeccion-gastos/proyeccion-gastos.component').then(m => m.ProyeccionGastosComponent) // Reporte de proyeccion de gastos
      },
      {
        path: 'reportes/eficiencia',
        loadComponent: () => import('../pages/vehiculos/reportes/eficiencia-conductores/eficiencia-conductores.component').then(m => m.EficienciaConductoresComponent) // Reporte de eficiencia de conductores
      },
      {
        path: '',
        redirectTo: 'home', // Si no se especifica ruta, va a home
        pathMatch: 'full'
      }
    ]
  }
];
