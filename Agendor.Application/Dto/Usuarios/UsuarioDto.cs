namespace Agendor.Application.Dto.Usuarios
{
    public class UsuarioDto
    {
        public class UsuarioCreateDto
        {
            public Guid UsuarioId { get; set; }
            public string Email { get; set; } = default!;
            public string Password { get; set; } = default!;
            public string Role { get; set; } = default!;
        }

        public class UsuarioUpdateDto
        {
            public Guid UsuarioId { get; set; }
            public string Email { get; set; } = default!;
            public string Password { get; set; } = default!;
            public string Role { get; set; } = default!;
        }

        public class UsuarioResponseDto
        {
            public string Email { get; set; } = default!;
            public string Role { get; set; } = default!;
        }
    }
}
