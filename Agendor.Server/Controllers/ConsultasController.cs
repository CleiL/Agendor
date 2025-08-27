using Agendor.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Agendor.Application.Dto.Consultas.ConsultaDto;

namespace Agendor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultasController
        (
            IConsultaService service,
            ILogger<ConsultasController> logger
        )
        : ControllerBase
    {
        private readonly IConsultaService _service = service;
        private readonly ILogger<ConsultasController> _logger = logger;

        /// <summary>Agenda uma nova consulta (regras: 30min, 08–18, seg–sex, sem conflitos).</summary>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ConsultaResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<ConsultaResponseDto>> Agendar([FromBody] ConsultaCreateDto dto)
        {
            var ct = HttpContext.RequestAborted;

            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "ConsultaController.Agendar",
                ["MedicoId"] = dto.MedicoId,
                ["PacienteId"] = dto.PacienteId,
                ["DataHora"] = dto.DataHora
            }))
            {
                try
                {
                    var created = await _service.AgendarAsync(dto, ct);
                    _logger.LogInformation("Consulta {ConsultaId} criada (MedicoId={MedicoId}, PacienteId={PacienteId}, DataHora={DataHora})",
                        created.ConsultaId, created.MedicoId, created.PacienteId, created.DataHora);
                    return Created($"/api/consulta/{created.ConsultaId}", created);
                
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(Problem(title: "Dados inválidos", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest));
                }
                catch (InvalidOperationException ex)
                {
                    // conflitos de regra (horário ocupado, paciente já tem no dia, fora da janela, etc.)
                    return Conflict(Problem(title: "Conflito de agenda", detail: ex.Message, statusCode: StatusCodes.Status409Conflict));
                }
            }
        }

        /// <summary>Retorna a agenda de slots (30min) do profissional para o dia informado.</summary>
        [HttpGet("profissionais/{profissionalId:guid}/agenda")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AgendaSlotDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<IEnumerable<AgendaSlotDto>>> Agenda(Guid profissionalId, [FromQuery] DateTime dia)
        {
            var ct = HttpContext.RequestAborted;

            if (dia == default)
                return BadRequest(Problem(title: "Parâmetro obrigatório", detail: "Informe o parâmetro de query 'dia' (YYYY-MM-DD).", statusCode: StatusCodes.Status400BadRequest));

            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "ConsultaController.Agenda",
                ["ProfissionalId"] = profissionalId,
                ["Dia"] = dia.Date
            }))
            {
                var slots = await _service.AgendaDoProfissionalAsync(profissionalId, dia, ct);
                _logger.LogInformation("Agenda consultada: {Qtde} slots para o profissional {ProfissionalId} em {Dia}",
                    slots.Count(), profissionalId, dia.Date);
                return Ok(slots);
            }
        }

        /// <summary>Lista todas as consultas agendadas de um profissional.</summary>
        [HttpGet("profissionais/{profissionalId:guid}/consultas")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ConsultaResponseDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<IEnumerable<ConsultaResponseDto>>> ConsultasDoProfissional(Guid profissionalId)
        {
            var ct = HttpContext.RequestAborted;

            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "ConsultaController.ListarPorProfissional",
                ["ProfissionalId"] = profissionalId
            }))
            {
                try
                {
                    var consultas = await _service.GetConsultasPorProfissionalAsync(profissionalId, ct);
                    return Ok(consultas);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao listar consultas do profissional");
                    return StatusCode(500, Problem(title: "Erro interno", detail: ex.Message));
                }
            }
        }

    }
}
