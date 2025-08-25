namespace Agendor.Core.Entities
{
    public class Usuario
    {
        public Guid UsuarioId { get; set; }
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string Role { get; set; } = default!;
    }
}
