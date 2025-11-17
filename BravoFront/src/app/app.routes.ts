import { Routes } from '@angular/router';

export const routes: Routes = [

  // --- Auth ---
  {
    path: '',
    loadChildren: () =>
      import('./features/auth/routes/auth.routes').then(r => r.AUTH_ROUTES)
  },

  // --- Gerente ---
  {
    path: 'gerente',
    loadChildren: () =>
      import('./features/dashboard-gerente/routes/gerente.routes')
        .then(r => r.GERENTE_ROUTES)
  },

  // --- Conductor ---
  {
    path: 'conductor',
    loadChildren: () =>
      import('./features/dashboard-conductor/routes/conductor.routes')
        .then(r => r.CONDUCTOR_ROUTES)
  },

  {
    path: '**',
    redirectTo: '/login'
  }
];
