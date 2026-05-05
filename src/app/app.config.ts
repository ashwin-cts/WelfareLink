import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes'; // to get the custem route file

// 1. Import withInterceptors and import for HTTP
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http'; 
// 2. Import your new interceptor
import { jwtInterceptor } from './core/interceptors/jwt-interceptor';



export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes), 
    
    // 2. ADD THIS PROVIDER so your AuthService can talk to C#
    provideHttpClient(withFetch(),
    withInterceptors([jwtInterceptor])
    )
  ]
};