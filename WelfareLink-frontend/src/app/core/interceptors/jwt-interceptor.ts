import { HttpInterceptorFn } from '@angular/common/http';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  // 1. Check if we have a logged-in user in localStorage
  const storedUser = localStorage.getItem('currentUser');

  if (storedUser) {
    const user = JSON.parse(storedUser);
    
    // IMPORTANT: Make sure 'token' matches exactly what your C# backend returns
    // Sometimes C# returns 'Token' (capital T) or 'jwtToken'. Check your console!
    const token = user.token; 

    if (token) {
      // 2. Clone the request and inject the Authorization header
      const clonedRequest = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
      
      // 3. Send the modified request to the backend
      return next(clonedRequest);
    }
  }

  // If there is no token (like when they are trying to log in), just send the normal request
  return next(req);
};