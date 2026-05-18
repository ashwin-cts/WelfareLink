import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const storedUser = localStorage.getItem('currentUser');

  if (storedUser) {
    // They have a token! Let them in.
    return true;
  } else {
    // No token! Kick them back to the login page.
    router.navigate(['/login']);
    return false;
  }
};