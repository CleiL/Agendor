export interface ConsultaCreateDto {
  medicoId: string;
  pacienteId: string;
  dataHora: string; // ISO string
}

export interface ConsultaResponseDto {
  consultaId: string;
  medicoId: string;
  pacienteId: string;
  dataHora: string;
  especialidade?: string; 
}
