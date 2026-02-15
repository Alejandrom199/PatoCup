import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../../environments/src/environments/environment.development';
import { User } from '../../models/user';
import { ApiResponse } from '../../models/api-response';

@Injectable({
  providedIn: 'root'
})
export class Auth {

  private readonly API_URL = `${environment.apiUrl}Auth`;

  private userSubject = new BehaviorSubject<User | null>(null);
  public user$ = this.userSubject.asObservable();

  constructor(private http: HttpClient) {
    
  const savedUser = localStorage.getItem('pato-user-meta');
    if (savedUser) {
      try {
        this.userSubject.next(JSON.parse(savedUser));
      } catch {
        this.logout();
      }
    }
  }

  public get currentUser(): User | null {
    return this.userSubject.value;
  }

  login(credentials: { username: string; password: string }): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.API_URL}/login`, credentials, { withCredentials: true })
      .pipe(
        tap(response => {
          if (response.succeeded && response.data) {
            const userMeta: User = {
              id: response.data.id,
              username: response.data.username,
              role: response.data.role,
              photoUrl: response.data.photoUrl
            };
            this.userSubject.next(userMeta);
            localStorage.setItem('pato-user-meta', JSON.stringify(userMeta));
          }
        })
      );
  }

  logout(): void {
    this.http.post(`${this.API_URL}/logout`, {}, { withCredentials: true }).subscribe();
    localStorage.removeItem('pato-user-meta');
    this.userSubject.next(null);
  }

  forceLogout(): void {
    localStorage.removeItem('pato-user-meta');
    this.userSubject.next(null);
  }

  isLoggedIn(): boolean {
    const user = localStorage.getItem('pato-user-meta');
    return !!user;
  }

  
}