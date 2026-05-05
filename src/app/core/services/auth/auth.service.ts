import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // TODO: Replace with your actual Visual Studio backend URL (e.g., https://localhost:7123/api)
  private readonly API_URL = 'https://localhost:7242/api'; 
  
  // We use a BehaviorSubject to keep track of the logged-in state across the app
  private currentUserSubject = new BehaviorSubject<any>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    // Check if user is already logged in upon app load
    const storedUser = localStorage.getItem('currentUser');
    if (storedUser) {
      this.currentUserSubject.next(JSON.parse(storedUser));
    }
  }

  login(credentials: { username: string, password: string, userType: string }): Observable<any> {
    // Pointing directly to your /auth/login endpoint
    return this.http.post<any>(`${this.API_URL}/auth/login`, credentials).pipe(
      tap(user => {
        // If the backend returns a token, store it in local storage
        if (user && user.token) { 
          localStorage.setItem('currentUser', JSON.stringify(user));
          this.currentUserSubject.next(user);
        }
      })
    );
  }

  logout() {
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
  }

  // Only Citizens self-register
  registerCitizen(citizenData: any): Observable<any> {
    return this.http.post(`${this.API_URL}/citizenapi`, citizenData);
  }
}