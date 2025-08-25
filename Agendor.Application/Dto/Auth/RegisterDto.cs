using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agendor.Application.Dto.Auth
{
    public class RegisterDto
    {
        public class RegisterPacienteDto
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

            [Required(ErrorMessage = "Senha é obrigatória.")]
            [PasswordPropertyText]
            [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$",
            ErrorMessage = "A senha deve ter no mínimo 8 caracteres, incluindo letras e números.")]
            [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
            public string? Password { get; set; }
        }

        public class RegisterMedicoDto
        {
            [Required(ErrorMessage = "Nome é obrigatório.")]
            [StringLength(150, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 a 150 caracteres.")]
            [DefaultValue("Dr. Clei Lisboa Santos")]
            public string? Nome { get; set; }

            [Required(ErrorMessage = "Email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
            [DefaultValue("clei.lisboa@gmail.com")]
            public string? Email { get; set; }

            [Required(ErrorMessage = "Telefone é obrigatório.")]
            [RegularExpression(@"^\+?[1-9]\d{1,14}$|^\d{10,11}$", ErrorMessage = "Telefone inválido.")]
            [DefaultValue("+5511999990000")]
            public string? Phone { get; set; }

            [Required(ErrorMessage = "CRM é obrigatório.")]
            [RegularExpression(@"^CRM/[A-Z]{2}\s\d{1,6}$", ErrorMessage = "Informe no formato: CRM/UF 123456")]
            [DefaultValue("CRM/SP 123456")]
            public string? CRM { get; set; }

            [Required(ErrorMessage = "Especialidade é obrigatório.")]
            [StringLength(150, MinimumLength = 3, ErrorMessage = "Especialidade deve ter entre 3 a 150 caracteres.")]
            [DefaultValue("Cardiologista")]
            public string? Especialidade { get; set; }

            [Required(ErrorMessage = "Senha é obrigatória.")]
            [PasswordPropertyText]
            [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$",
            ErrorMessage = "A senha deve ter no mínimo 8 caracteres, incluindo letras e números.")]
            [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
            public string? Password { get; set; }
        }

        public class RegisterResponseDto
        {
            public string? Nome { get; set; }
        }
    }
}
