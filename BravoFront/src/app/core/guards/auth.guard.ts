import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { LocalstorageService } from '../services/localstorage.service';

export const authGuard: CanActivateFn = () => {
  const local_storage = inject(LocalstorageService);
  const router = inject(Router);

  const token = local_storage.getToken();

  if (!token) {
    router.navigate(['/login']);
    return false;
  }

  return true;
};
