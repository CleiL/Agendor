import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(
    private http: HttpClient,
  ) { }

  private apiUrl = environment.apiUrls[0];

  login(data: { email: string, password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/login/`, data).pipe(
      tap((res: any) => {
        localStorage.setItem('access_token', res.access);
        localStorage.setItem('refresh_token', res.refresh);
      })
    );
  }

  logout() {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
  }

  register(data: { name: string, cpf: string, email: string, password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/register/`, data).pipe(
      tap((res: any) => {
        localStorage.setItem('access_token', res.access);
        localStorage.setItem('refresh_token', res.refresh);
      })
    );
  }

  registerMedico(data: { name: string, crm: string, email: string, password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/register-medico/`, data).pipe(
      tap((res: any) => {
        localStorage.setItem('access_token', res.access);
        localStorage.setItem('refresh_token', res.refresh);
      })
    );
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('access_token');
  }

  getToken(): string | null {
    return localStorage.getItem('access_token');
  }
}
