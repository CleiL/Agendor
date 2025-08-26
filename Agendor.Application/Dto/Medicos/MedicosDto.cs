using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Agendor.Application.Dto.Medicos
{
    public class MedicosDto
    {
        public class MedicoCreateDto
        {
            [Required(ErrorMessage = "Nome é obrigatório.")]
            [StringLength(150, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 a 150 caracteres.")]
            [DefaultValue("Dr. Clei Lisboa Santos")]
            public string? Nome { get; set; }

            [Required(ErrorMessage = "Email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
            [DefaultValue("clei.lisboa@gmail.com")]
            public string? Email { get; set; }

            [Required(ErrorMessage = "CRM é obrigatório.")]
            [RegularExpression(@"^CRM/[A-Z]{2}\s\d{1,6}$", ErrorMessage = "Informe no formato: CRM/UF 123456")]
            [DefaultValue("CRM/SP 123456")]
            public string? CRM { get; set; }

            [Required(ErrorMessage = "Especialidade é obrigatório.")]
            [StringLength(150, MinimumLength = 3, ErrorMessage = "Especialidade deve ter entre 3 a 150 caracteres.")]
            [DefaultValue("Cardiologista")]
            public string? Especialidade { get; set; }
        }

        public class MedicoUpdateDto
        {
            public Guid MedicoId { get; set; }

            [StringLength(150, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 a 150 caracteres.")]
            public string? Nome { get; set; }
            [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
            public string? Email { get; set; }

            [RegularExpression(@"^\+?[1-9]\d{1,14}$|^\d{10,11}$", ErrorMessage = "Telefone inválido.")]
            public string? Phone { get; set; }

            [RegularExpression(@"^CRM/[A-Z]{2}\s\d{1,6}$", ErrorMessage = "Informe no formato: CRM/UF 123456")]
            public string? CRM { get; set; }

            [StringLength(150, MinimumLength = 3, ErrorMessage = "Especialidade deve ter entre 3 a 150 caracteres.")]
            public string? Especialidade { get; set; }
        }

        public class MedicoResponseDto
        {
            public Guid MedicoId { get; set; }
            public string Nome { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string Phone { get; set; } = default!;
            public string CRM { get; set; } = default!;
            public string Especialidade { get; set; } = default!;
        }
    }
}
