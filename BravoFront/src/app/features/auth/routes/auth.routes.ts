import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    // Carga lazy del componente de login
    loadComponent: () => 
      import('../login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    // Carga lazy del componente de registro
    loadComponent: () => 
      import('../register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: '',
    // Redirige a login si no se especifica ruta
    redirectTo: 'login',
    pathMatch: 'full'
  }
];
