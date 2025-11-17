import { Routes } from "@angular/router";

export const GERENTE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../dashboard-gerente.component').then(m => m.DashboardGerenteComponent),

    children: [
      {
        path: 'vehiculos',
        children: [
          {
            path: '',
            loadComponent: () =>
              import('../pages/vehiculo-lista/vehiculo-lista.page')
                .then(m => m.VehiculosListaPage)
          },
          {
            path: 'nuevo',
            loadComponent: () =>
              import('../components/vehiculo-form/vehiculo-form.component')
                .then(m => m.VehiculoFormComponent)
          },
          {
            path: 'editar/:id',
            loadComponent: () =>
              import('../components/vehiculo-form/vehiculo-form.component')
                .then(m => m.VehiculoFormComponent)
          },
        ],
      },

      { path: '', redirectTo: 'vehiculos', pathMatch: 'full' }
    ]
  }
];
