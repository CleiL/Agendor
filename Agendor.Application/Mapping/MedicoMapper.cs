using Agendor.Core.Entities;
using static Agendor.Application.Dto.Medicos.MedicosDto;

namespace Agendor.Application.Mapping
{
    public static class MedicoMapper
    {
        public static MedicoResponseDto ToDto(this Medico e)
           => new MedicoResponseDto
           {
               MedicoId = e.MedicoId,
               Nome = e.Nome,
               CRM = e.CRM,
               Email = e.Email,
               Phone = e.Phone,
               Especialidade = e.Especialidade
           };

        public static Medico ToEntity(this MedicoCreateDto dto)
            => new Medico
            {
                MedicoId = Guid.NewGuid(),
                Nome = dto.Nome?.Trim() ?? string.Empty,
                CRM = dto.CRM?.Trim() ?? string.Empty,
                Email = dto.Email?.Trim() ?? string.Empty,
                Phone = dto.Phone?.Trim() ?? string.Empty,
                Especialidade = dto.Especialidade?.Trim() ?? string.Empty
            };
    }
}
