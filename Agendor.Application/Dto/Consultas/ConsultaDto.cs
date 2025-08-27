namespace Agendor.Application.Dto.Consultas
{
    public class ConsultaDto
    {
        public class AgendarDto
        {
            public DateTime Dia { get; set; }
        }

        public class ConsultaCreateDto
        {
            public Guid MedicoId { get; set; }
            public Guid PacienteId { get; set; }
            public DateTime DataHora { get; set; }
            public string? Slot { get; set; }
            public DateOnly Dia { get; set; }
        }

        public class ConsultaResponseDto
        {
            public Guid ConsultaId { get; set; }
            public Guid MedicoId { get; set; }
            public Guid PacienteId { get; set; }
            public DateTime DataHora { get; set; }
            public string? Especialidade { get; set; }
            public string? MedicoNome { get; set; }
        }

        public class AgendaSlotDto
        {
            public DateTime Horario { get; set; }
            public bool Disponivel { get; set; }
        }
    }
}
