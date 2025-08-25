using Agendor.Core.Entities;
using static Agendor.Application.Dto.Pacientes.PacienteDto;

namespace Agendor.Application.Mapping
{
    public static class PacienteMapper
    {
        public static PacienteResponseDto ToDto(this Paciente e)
            => new PacienteResponseDto
            {
                PacienteId = e.PacienteId,
                Nome = e.Nome,
                CPF = e.CPF,
                Email = e.Email,
                Phone = e.Phone
            };

        public static Paciente ToEntity(this PacienteCreateDto dto)
            => new Paciente
            {
                PacienteId = Guid.NewGuid(),
                Nome = dto.Nome.Trim() ?? string.Empty,
                CPF = dto.CPF.Trim() ?? string.Empty,
                Email = dto.Email.Trim() ?? string.Empty,
                Phone = dto.Phone.Trim() ?? string.Empty
            };

        public static void Apply(this Paciente entity, PacienteUpdateDto dto)
        {
            entity.Nome = dto.Nome?.Trim() ?? string.Empty;
            entity.CPF = dto.CPF?.Trim() ?? string.Empty;
            entity.Email = dto.Email?.Trim() ?? string.Empty;
            entity.Phone = dto.Phone?.Trim() ?? string.Empty;
        }
    }
}
