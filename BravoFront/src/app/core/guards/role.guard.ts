// Guard para proteger rutas basado en el rol del usuario
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { LocalstorageService } from '../services/localstorage.service';

export const roleGuard: CanActivateFn = (route) => {
  // Inyectamos servicio de almacenamiento local para obtener usuario
  const local_storage = inject(LocalstorageService);
  // Inyectamos router para redirigir si no cumple el rol
  const router = inject(Router);

  // Obtenemos el usuario almacenado
  const user = local_storage.getUser();
  // Leemos el rol requerido desde la data de la ruta
  const requiredRole = route.data?.['role'];

  // Si no hay usuario o su rol no coincide, redirigimos al login
  if (!user || user.role !== requiredRole) {
    router.navigate(['/login']);
    return false;
  }

  // Si cumple el rol, permitimos el acceso
  return true;
};
