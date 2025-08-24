import { CommonModule } from "@angular/common";
import { Component } from "@angular/core";
import { MatCardModule } from "@angular/material/card";
import { AppointmentService, Appointment } from "../services/appointment.service";

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
      <mat-card class="appt" *ngFor="let appt of myAppointments">
        <mat-card-title>{{ appt.specialty }} — {{ appt.patientId }}</mat-card-title>
        <mat-card-subtitle>
          {{ appt.date | date:'fullDate' }} — {{ appt.date | date:'HH:mm' }}
        </mat-card-subtitle>
      </mat-card>

      <p *ngIf="myAppointments.length === 0" class="empty-hint">
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
export class ConsultaComponent {
  private readonly doctorName = "Dr. Marcos Silva"; // simula médico logado

  constructor(private apptService: AppointmentService) { }

  get myAppointments(): Appointment[] {
    return this.apptService.getByDoctor(this.doctorName);
  }
}
