import { CommonModule } from "@angular/common";
import { Component, OnInit } from "@angular/core";
import { MatCardModule } from "@angular/material/card";
import { AppointmentService } from "../services/appointment.service";
import { ConsultaResponseDto } from "../interfaces/consulta";
import { AuthService } from "../services/auth.service";

@Component({
  selector: "app-consultas",
  standalone: true,
  imports: [CommonModule, MatCardModule],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>Minhas Consultas</mat-card-title>
        <mat-card-subtitle>Agenda do médico</mat-card-subtitle>
      </mat-card-header>
    </mat-card>

    <section class="list">
      <mat-card class="appt" *ngFor="let c of consultas">
        <mat-card-title>{{ c.especialidade || '—' }} — {{ c.pacienteId }}</mat-card-title>
        <mat-card-subtitle>
          {{ c.dataHora | date:'fullDate' }} — {{ c.dataHora | date:'HH:mm' }}
        </mat-card-subtitle>
      </mat-card>

      <p *ngIf="consultas.length === 0" class="empty-hint">
        Nenhuma consulta marcada para você.
      </p>
    </section>

  `,
  styles: [`
    :host { display: block; width: 100%; }
    mat-card { margin: 1rem; padding: 1rem; }
    .list {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
      gap: 1rem;
      padding: 1rem;
    }
    .appt { border-left: 4px solid #e91e63; }
    .empty-hint { opacity: 0.7; }
  `]
})
export class ConsultaComponent implements OnInit {
  consultas: ConsultaResponseDto[] = [];

  constructor(
    private apptService: AppointmentService,
    private auth: AuthService
  ) { }

  ngOnInit(): void {
    const medicoId = this.auth.userId;

    if (!medicoId) {
      console.error("Usuário não autenticado ou token inválido.");
      return;
    }

    this.apptService.getByDoctor(medicoId).subscribe({
      next: res => this.consultas = res,
      error: err => console.error("Erro ao carregar consultas", err)
    });
  }
}
