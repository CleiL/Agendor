namespace Agendor.Core.Entities
{
    public class Medico
    {
        public Guid MedicoId { get; set; }
        public string Nome { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? Phone { get; set; }
        public string CRM { get; set; } = default!;
        public string Especialidade { get; set; } = default!;
    }
}
