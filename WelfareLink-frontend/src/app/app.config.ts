import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes'; // to get the custom route file

// Import for HTTP and Interceptors
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http'; 
import { jwtInterceptor } from './core/interceptors/jwt-interceptor';

// NEW: Import your API Configuration token and default values
import { API_CONFIG, defaultApiConfig } from './core/config/api.config';

export const appConfig: ApplicationConfig = {
  providers: [
    // Recommended by Angular for better performance (since you imported it)
    provideZoneChangeDetection({ eventCoalescing: true }), 
    
    provideRouter(routes), 
    
    // Your HTTP Client with the JWT Interceptor
    provideHttpClient(
      withFetch(),
      withInterceptors([jwtInterceptor])
    ),

    // NEW: Provide the global API Configuration
    { provide: API_CONFIG, useValue: defaultApiConfig }
  ]
};