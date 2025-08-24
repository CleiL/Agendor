import { CommonModule } from "@angular/common";
import { Component } from "@angular/core";
import { RouterModule } from "@angular/router";
import { FormsModule } from "@angular/forms";

import { MatCardModule } from "@angular/material/card";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { MatDatepickerModule } from "@angular/material/datepicker";
import { MatNativeDateModule } from "@angular/material/core";
import { MatButtonModule } from "@angular/material/button";
import { MatTooltipModule } from "@angular/material/tooltip";
import { MatSnackBar, MatSnackBarModule } from "@angular/material/snack-bar";
import { AppointmentService } from "../services/appointment.service";

type Appointment = {
  id: string;
  patientId: string;
  date: Date;         
  specialty: string;
  doctor: string;
};

@Component({
  selector: "app-agenda",
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
    MatTooltipModule,
    MatSnackBarModule
  ],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>Minha Agenda</mat-card-title>
        <mat-card-subtitle>Agendo do paciente</mat-card-subtitle>
      </mat-card-header>

      <mat-card-content class="form-grid">
        <mat-form-field appearance="outline">
          <mat-label>Especialidade</mat-label>
          <mat-select [(ngModel)]="selectedSpecialty" (selectionChange)="onSpecialtyChange()">
            <mat-option *ngFor="let esp of specialties" [value]="esp">{{ esp }}</mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Médico</mat-label>
          <mat-select [(ngModel)]="selectedDoctor" [disabled]="!selectedSpecialty" (selectionChange)="refreshSlots()">
            <mat-option *ngFor="let med of doctorsForSelected()" [value]="med">{{ med }}</mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Data de Agendamento</mat-label>
          <input matInput [matDatepicker]="picker"
                 [(ngModel)]="selectedDate"
                 [matDatepickerFilter]="weekdaysOnly"
                 (dateChange)="refreshSlots()">
          <mat-datepicker-toggle matIconSuffix [for]="picker"></mat-datepicker-toggle>
          <mat-datepicker #picker></mat-datepicker>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Horário</mat-label>
          <mat-select [(ngModel)]="selectedSlot" [disabled]="!selectedDate || !selectedDoctor">
            <mat-option *ngFor="let s of slotOptions" [value]="s">
              {{ s }}
            </mat-option>
          </mat-select>
        </mat-form-field>

        <button mat-stroked-button class="agendar-btn"
                [disabled]="!canBook"
                (click)="addAppointment()"
                matTooltip="Agendar">
          <span>Agendar</span>
        </button>
      </mat-card-content>
    </mat-card>

    <!-- Lista de consultas agendadas -->
    <section class="list">
      <mat-card class="appt" *ngFor="let appt of appointments">
        <mat-card-title>{{ appt.specialty }} • {{ appt.doctor }}</mat-card-title>
        <mat-card-subtitle>
          {{ appt.date | date:'fullDate' }} — {{ appt.date | date:'HH:mm' }}
        </mat-card-subtitle>
      </mat-card>
      <p *ngIf="appointments.length === 0" class="empty-hint">
        Nenhum agendamento ainda. Preencha os campos e clique em <strong>Agendar</strong>.
      </p>
    </section>
  `,
  styles: [`
    :host { display: block; width: 100%; }
    mat-card { margin: 1rem; padding: 1rem; }

    .form-grid {
      display: grid;
      grid-template-columns: repeat(5, minmax(200px, 1fr));
      gap: 1rem;
      align-items: center;
    }

    .agendar-btn { height: 56px; }

    .list {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
      gap: 1rem;
      padding: 0 1rem 1rem;
    }

    .appt { border-left: 4px solid #3f51b5; }

    .empty-hint {
      grid-column: 1 / -1;
      opacity: 0.7;
      margin: 0 1rem 1rem;
    }

    @media (max-width: 1100px) {
      .form-grid { grid-template-columns: 1fr 1fr; }
      .agendar-btn { width: 100%; }
    }

    @media (max-width: 700px) {
      .form-grid { grid-template-columns: 1fr; }
    }
  `]
})
export class AgendaComponent {
  constructor(
    private snack: MatSnackBar,
    private apptService: AppointmentService
  ) { }

  // Simulação: paciente logado (id fixo para validar regra "1 por dia por profissional")
  private readonly patientId = "meu-paciente";

  isLoadingSearch = false;

  // Estado do formulário
  selectedSpecialty: string | null = null;
  selectedDoctor: string | null = null;
  selectedDate: Date | null = null;
  selectedSlot: string | null = null; // "HH:mm"

  // Slots disponíveis para a data e médico selecionados
  slotOptions: string[] = [];

  // Catálogo simples
  specialties: string[] = ["Clínico Geral", "Cardiologia", "Dermatologia"];
  doctorsBySpecialty: Record<string, string[]> = {
    "Clínico Geral": ["Dra. Ana Lima", "Dr. Paulo Souza"],
    "Cardiologia": ["Dr. Marcos Silva", "Dra. Beatriz Figueiredo"],
    "Dermatologia": ["Dra. Camila Rocha"]
  };

  // Agenda local (mock)
  get appointments(): Appointment[] {
    return this.apptService.getByPatient(this.patientId);
  }

  // --------- UI helpers ---------
  weekdaysOnly = (d: Date | null) => {
    if (!d) return false;
    const day = d.getDay(); // 0=Dom, 6=Sáb
    return day !== 0 && day !== 6;
  };

  get canBook(): boolean {
    return !!(this.selectedSpecialty && this.selectedDoctor && this.selectedDate && this.selectedSlot);
  }

  onSpecialtyChange() {
    this.selectedDoctor = null;
    this.refreshSlots();
  }

  doctorsForSelected(): string[] {
    return this.selectedSpecialty ? (this.doctorsBySpecialty[this.selectedSpecialty] ?? []) : [];
  }

  refreshSlots() {
    this.selectedSlot = null;
    if (!this.selectedDate || !this.selectedDoctor) {
      this.slotOptions = [];
      return;
    }
    this.slotOptions = this.generateDaySlots(this.selectedDate)
      .filter(s => this.isSlotAvailableForDoctor(this.selectedDoctor!, this.selectedDate!, s));
  }

  // --------- Regras de negócio (front) ---------
  private generateDaySlots(date: Date): string[] {
    // 08:00 até 18:00, duração 30 min => último slot inicia 17:30
    const start = new Date(date); start.setHours(8, 0, 0, 0);
    const endExcl = new Date(date); endExcl.setHours(18, 0, 0, 0);
    const slots: string[] = [];
    const cur = new Date(start);
    while (cur < endExcl) {
      const hh = String(cur.getHours()).padStart(2, "0");
      const mm = String(cur.getMinutes()).padStart(2, "0");
      slots.push(`${hh}:${mm}`);
      cur.setMinutes(cur.getMinutes() + 30);
    }
    return slots;
  }

  private isSlotAvailableForDoctor(doctor: string, date: Date, slot: string): boolean {
    // "um profissional só pode atender uma consulta por horário"
    return !this.appointments.some(a =>
      a.doctor === doctor &&
      this.isSameDay(a.date, date) &&
      this.timeToStr(a.date) === slot
    );
  }

  private patientHasConsultationWithDoctorOnDay(patientId: string, doctor: string, date: Date): boolean {
    // "um paciente só pode ter 1 consulta por profissional por dia"
    return this.appointments.some(a =>
      a.patientId === patientId &&
      a.doctor === doctor &&
      this.isSameDay(a.date, date)
    );
  }

  private mergeDateAndTime(baseDate: Date, hhmm: string): Date {
    const [hh, mm] = hhmm.split(":").map(v => parseInt(v, 10));
    const d = new Date(baseDate);
    d.setHours(hh, mm, 0, 0);
    return d;
  }

  private isSameDay(a: Date, b: Date): boolean {
    return a.getFullYear() === b.getFullYear()
      && a.getMonth() === b.getMonth()
      && a.getDate() === b.getDate();
  }

  private timeToStr(d: Date): string {
    return `${String(d.getHours()).padStart(2, "0")}:${String(d.getMinutes()).padStart(2, "0")}`;
  }

  addAppointment(): void {
    if (!this.canBook) return;

    // 1) Segunda a sexta
    if (!this.weekdaysOnly(this.selectedDate!)) {
      this.snack.open("Agendamentos apenas de segunda a sexta.", "Fechar", { duration: 3500 });
      return;
    }

    // 2) Slot válido (08:00–18:00, 30min)
    if (!this.slotOptions.includes(this.selectedSlot!)) {
      this.snack.open("Horário inválido para o dia selecionado.", "Fechar", { duration: 3500 });
      return;
    }

    // 3) Profissional não pode ter 2 no mesmo horário
    if (!this.isSlotAvailableForDoctor(this.selectedDoctor!, this.selectedDate!, this.selectedSlot!)) {
      this.snack.open("Médico já possui consulta neste horário.", "Fechar", { duration: 3500 });
      return;
    }

    // 4) Paciente só 1 consulta com o mesmo profissional no dia
    if (this.patientHasConsultationWithDoctorOnDay(this.patientId, this.selectedDoctor!, this.selectedDate!)) {
      this.snack.open("Você já possui consulta com este profissional neste dia.", "Fechar", { duration: 3500 });
      return;
    }

    // Passou nas regras → cria a consulta local
    const dateWithTime = this.mergeDateAndTime(this.selectedDate!, this.selectedSlot!);
    this.apptService.add({
      id: crypto.randomUUID(),
      patientId: this.patientId,
      date: dateWithTime,
      specialty: this.selectedSpecialty!,
      doctor: this.selectedDoctor!
    });

    this.snack.open("Consulta agendada!", "Fechar", { duration: 2500 });

    // Recalcular slots para refletir o horário agora ocupado
    this.refreshSlots();
  }
}
