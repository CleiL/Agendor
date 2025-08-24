import { Injectable } from '@angular/core';
import { IProfileService, ProfileDto } from '../contracts/profile.tokens';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class DoctorService implements IProfileService {
  constructor(private http: HttpClient) { }
  getProfile(): Observable<ProfileDto> {
    return this.http.get<ProfileDto>('/api/medicos/me');
  }
  updateProfile(payload: Partial<ProfileDto>): Observable<void> {
    return this.http.put<void>('/api/medicos/me', payload);
  }
  updatePassword(payload: { password: string }): Observable<void> {
    return this.http.post<void>('/api/medicos/me/password', payload);
  }
}
