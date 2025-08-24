import { Injectable } from '@angular/core';
import { IProfileService, ProfileDto } from '../contracts/profile.tokens';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class PatientService implements IProfileService {
  constructor(private http: HttpClient) { }
  getProfile(): Observable<ProfileDto> {
    return this.http.get<ProfileDto>('/api/pacientes/me');
  }
  updateProfile(payload: Partial<ProfileDto>): Observable<void> {
    return this.http.put<void>('/api/pacientes/me', payload);
  }
  updatePassword(payload: { password: string }): Observable<void> {
    return this.http.post<void>('/api/pacientes/me/password', payload);
  }
}
