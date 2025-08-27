namespace Agendor.Application.Dto.Agenda
{
    public class AgendaDto
    {
        public class EspecialidadeDto(string Nome);
        public class MedicoListItemDto(Guid MedicoId, string Nome, string Especialidade);
        public class SlotsRequestDto(Guid MedicoId, DateOnly Dia);
        public class SlotsResponseDto(DateOnly Dia, Guid MedicoId, IReadOnlyList<string> Slots);
    }
}
