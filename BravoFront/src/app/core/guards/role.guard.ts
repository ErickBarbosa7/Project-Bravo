import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { LocalstorageService } from '../services/localstorage.service';

export const roleGuard: CanActivateFn = (route) => {
  const local_storage = inject(LocalstorageService);
  const router = inject(Router);

  const user = local_storage.getUser();
  const requiredRole = route.data?.['role'];

  if (!user || user.rol !== requiredRole) {
    router.navigate(['/login']);
    return false;
  }

  return true;
};
