using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Agendor.Application.Dto.Pacientes
{
    public class PacienteDto
    {
        public class PacienteCreateDto
        {
            [Required(ErrorMessage = "Nome é obrigatório.")]
            [StringLength(150, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 a 150 caracteres.")]
            [DefaultValue("Clei Lisboa Santos")]
            public string Nome { get; set; } = default!;

            [Required(ErrorMessage = "CPF é obrigatório.")]
            [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter exatamente 11 dígitos.")]
            [DefaultValue("52224629001")]
            public string CPF { get; set; } = default!;

            [Required(ErrorMessage = "Email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
            [DefaultValue("clei.lisboa@email.com")]
            public string Email { get; set; } = default!;

            [Required(ErrorMessage = "Telefone é obrigatório.")]
            // E.164 simples (ex.: +5511999990000) OU só dígitos BR (10..11 dígitos).
            [RegularExpression(@"^\+?[1-9]\d{1,14}$|^\d{10,11}$", ErrorMessage = "Telefone inválido.")]
            [DefaultValue("+5511999990000")]
            public string Phone { get; set; } = default!;
        }

        public class PacienteUpdateDto
        {
            [Required]
            public Guid PacienteId { get; set; }

            [StringLength(150, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 a 150 caracteres.")]
            public string? Nome { get; set; }

            [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter exatamente 11 dígitos.")]
            public string? CPF { get; set; }

            [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
            public string? Email { get; set; }

            [RegularExpression(@"^\+?[1-9]\d{1,14}$|^\d{10,11}$", ErrorMessage = "Telefone inválido.")]
            public string? Phone { get; set; }
        }

        public class PacienteResponseDto
        {
            public Guid PacienteId { get; set; }
            public string Nome { get; set; } = default!;
            public string CPF { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string Phone { get; set; } = default!;
        }
    }
}
