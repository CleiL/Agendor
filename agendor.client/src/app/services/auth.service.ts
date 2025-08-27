import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable, tap } from 'rxjs';
import { LoginResponse } from '../interfaces/login';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(
    private http: HttpClient,
  ) { }

  private apiUrl = environment.apiUrls[0];

  login(data: { email: string, password: string }): Observable<any> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/Auth/login`, data).pipe(
      tap((res: any) => {
        localStorage.setItem('access_token', res.token);
        if (res.nome) localStorage.setItem('nome', res.nome);
      })
    );
  }

  logout() {
    localStorage.removeItem('access_token');
    localStorage.removeItem('nome');
  }

  register(data: { nome: string, cpf: string, email: string, password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/register/paciente`, data);
  }

  registerMedico(data: { nome: string, especialidade: string, crm: string, email: string, password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/register/medico`, data);
  }

  isAuthenticated(): boolean {
    const t = this.token;
    if (!t) return false;
    const exp = this.jwtExp(t);
    return exp ? Date.now() / 1000 < exp : true; // se não tiver exp, assume válido
  }

  get token(): string | null {
    return localStorage.getItem('access_token');
  }

  // ====== Derivado do JWT ======

  /** Role em minúsculas (ex.: 'paciente' | 'medico' | 'admin') */
  get role(): string {
    const r = this.jwtRole(this.token);
    return (r ?? '').toLowerCase();
  }

  get isPaciente(): boolean {
    const r = this.role;
    return r === 'paciente' || r === 'patient';
  }

  get isMedico(): boolean {
    const r = this.role;
    return r === 'médico' || r === 'medico' || r === 'doctor';
  }

  get userId(): string | null {
    const payload = this.decodePayload<any>(this.token);
    return payload?.sub ?? null;
  }

  // ====== Helpers de JWT ======

  private decodePayload<T = any>(token: string | null): T | null {
    if (!token) return null;
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    try {
      // corrige base64url
      const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const json = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(json) as T;
    } catch {
      return null;
    }
  }

  /** Tenta extrair a role das claims comuns (role, roles, claim URI da Microsoft) */
  private jwtRole(token: string | null): string | null {
    const p = this.decodePayload<any>(token);
    if (!p) return null;
    return (
      p.role ??
      p.roles ??
      p['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
      null
    );
  }

  /** Epoch seconds do exp, se houver */
  private jwtExp(token: string | null): number | null {
    const p = this.decodePayload<any>(token);
    return p?.exp ?? null;
  }
}
