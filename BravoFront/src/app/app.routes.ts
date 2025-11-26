import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard'; 
import { roleGuard } from './core/guards/role.guard'; 

export const routes: Routes = [

  // --- Rutas de autenticacion ---
  // Esta ruta es public y no requiere token
  {
    path: 'auth', // Ruta para login/registro
    loadChildren: () =>
      import('./features/auth/routes/auth.routes').then(r => r.AUTH_ROUTES)
  },

  // --- Dashboard Gerente (Protegido) ---
  // Solo usuarios con token y rol Gerente pueden acceder
  {
    path: 'gerente',
    canActivate: [authGuard, roleGuard], // Primero valida token, luego el rol
    data: { role: 'Gerente' },           // Rol esperado para el roleGuard
    loadChildren: () =>
      import('./features/dashboard-gerente/routes/gerente.routes')
        .then(r => r.GERENTE_ROUTES)
  },

  // --- Dashboard Conductor (Protegido) ---
  // Solo usuarios con token y rol Conductor pueden acceder
  {
    path: 'conductor',
    canActivate: [authGuard, roleGuard],
    data: { role: 'Conductor' },
    loadChildren: () =>
      import('./features/dashboard-conductor/routes/conductor.routes')
        .then(r => r.CONDUCTOR_ROUTES)
  },

  // Redireccion por defecto
  {
    path: '',
    redirectTo: 'auth', 
    pathMatch: 'full'
  },
  
  // Cualquier ruta desconocida redirige a login
  {
    path: '**',
    redirectTo: 'auth'
  }
];
