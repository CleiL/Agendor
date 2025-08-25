namespace Agendor.Core.Entities
{
    public class Consulta
    {
        public Guid ConsultaId { get; set; }
        public DateTime DataHora { get; set; }
        public Guid PacienteId { get; set; }
        public Guid MedicoId { get; set; }
    }
}
