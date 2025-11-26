// Guard para proteger rutas, verifica si hay token en localStorage
import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { LocalstorageService } from '../services/localstorage.service';

export const authGuard: CanActivateFn = () => {
  // Inyectamos el servicio de almacenamiento local
  const local_storage = inject(LocalstorageService);
  // Inyectamos el router para redirigir si no hay token
  const router = inject(Router);

  // Obtenemos el token guardado
  const token = local_storage.getToken();

  // Si no hay token, redirigimos al login y bloqueamos el acceso
  if (!token) {
    router.navigate(['/login']);
    return false;
  }

  // Si hay token, permitimos el acceso
  return true;
};
