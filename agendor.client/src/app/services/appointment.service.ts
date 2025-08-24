// appointment.service.ts
import { Injectable } from "@angular/core";

export type Appointment = {
  id: string;
  patientId: string;
  date: Date;
  specialty: string;
  doctor: string;
};

@Injectable({ providedIn: "root" })
export class AppointmentService {
  private appointments: Appointment[] = [];

  getAll(): Appointment[] {
    return this.appointments;
  }

  add(appt: Appointment) {
    this.appointments.unshift(appt);
  }

  getByDoctor(doctor: string): Appointment[] {
    return this.appointments.filter(a => a.doctor === doctor);
  }

  getByPatient(patientId: string): Appointment[] {
    return this.appointments.filter(a => a.patientId === patientId);
  }
}
