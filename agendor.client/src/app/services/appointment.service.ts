import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { AgendaSlotDto } from "../interfaces/agenda";
import { ConsultaCreateDto, ConsultaResponseDto } from "../interfaces/consulta";
import { environment } from "../../environments/environment";

@Injectable({ providedIn: "root" })
export class AppointmentService {

  private apiUrl = environment.apiUrls[0];

  constructor(private http: HttpClient) { }

  /** Lista os slots disponíveis (30min) para o médico no dia */
  getAgenda(medicoId: string, dia: Date): Observable<AgendaSlotDto[]> {
    const diaParam = dia.toISOString().split("T")[0]; // apenas YYYY-MM-DD
    return this.http.get<AgendaSlotDto[]>(`${this.apiUrl}/consultas/profissionais/${medicoId}/agenda`, {
      params: { dia: diaParam }
    });
  }

  /** Cria/agenda uma nova consulta */
  agendarConsulta(dto: ConsultaCreateDto): Observable<ConsultaResponseDto> {
    return this.http.post<ConsultaResponseDto>(`${this.apiUrl}/consultas`, dto);
  }

  /** Lista todas as consultas do médico */
  getByDoctor(medicoId: string): Observable<ConsultaResponseDto[]> {
    return this.http.get<ConsultaResponseDto[]>(`${this.apiUrl}/consultas/profissionais/${medicoId}/consultas`);
  }

  /** Lista todas as consultas do paciente */
  getByPatient(pacienteId: string): Observable<ConsultaResponseDto[]> {
    return this.http.get<ConsultaResponseDto[]>(`${this.apiUrl}/consultas/pacientes/${pacienteId}/consultas`);
  }
}
