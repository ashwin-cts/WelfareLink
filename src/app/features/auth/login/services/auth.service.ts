import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { API_CONFIG, ApiConfig } from '../../../../core/config/api.config';
import { LoginCredentials, AuthResponse, RegisterCitizenRequest } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  
  // Use the strict AuthResponse interface instead of any
  private currentUserSubject = new BehaviorSubject<AuthResponse | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    @Inject(API_CONFIG) private apiConfig: ApiConfig // Inject the URL config!
  ) {
    // Check if user is already logged in upon app load
    const storedUser = localStorage.getItem('currentUser');
    if (storedUser) {
      this.currentUserSubject.next(JSON.parse(storedUser) as AuthResponse);
    }
  }

  // Strictly typed parameters and return type
  login(credentials: LoginCredentials): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiConfig.authApi}/auth/login`, credentials).pipe(
      tap(user => {
        // If the backend returns a token, store it in local storage
        if (user && (user.token || user.Token)) { 
          localStorage.setItem('currentUser', JSON.stringify(user));
          this.currentUserSubject.next(user);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem('currentUser');
    localStorage.removeItem('token'); // Clear the direct token as well
    this.currentUserSubject.next(null);
  }

  // Only Citizens self-register
  registerCitizen(citizenData: RegisterCitizenRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiConfig.citizenApi}/citizenapi`, citizenData);
  }
}