import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Medico } from '../interfaces/medico';

@Injectable({
  providedIn: 'root'
})
export class MedicoService {

  private apiUrl = environment.apiUrls[0];

  constructor(private http: HttpClient) { }

  getAll(): Observable<Medico[]> {
    return this.http.get<Medico[]>(`${this.apiUrl}/medico`);
  }
}
