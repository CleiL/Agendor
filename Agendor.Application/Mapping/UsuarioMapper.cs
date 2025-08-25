using Agendor.Core.Entities;
using static Agendor.Application.Dto.Usuarios.UsuarioDto;

namespace Agendor.Application.Mapping
{
    public static class UsuarioMapper
    {
        public static UsuarioResponseDto ToDto(this Usuario e)
           => new UsuarioResponseDto
           {
               Email = e.Email,
               Role = e.Role
           };

        public static Usuario ToEntity(this UsuarioCreateDto dto)
            => new Usuario
            {
                UsuarioId = Guid.NewGuid(),
                Email = dto.Email?.Trim() ?? string.Empty,
                PasswordHash = dto.Password?.Trim() ?? string.Empty,
                Role = dto.Role?.Trim() ?? string.Empty
            };
    }
}
