using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Agendor.Application.Dto.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        [DefaultValue("clei.lisboa@gmail.com")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória.")]
        [PasswordPropertyText]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$",
          ErrorMessage = "A senha deve ter no mínimo 8 caracteres, incluindo letras e números.")]
        [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
        public string? Password { get; set; }
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public Guid UsuarioId { get; set; }
        public Guid? PacienteId { get; set; }
        public Guid? MedicoId { get; set; }
    }
}
