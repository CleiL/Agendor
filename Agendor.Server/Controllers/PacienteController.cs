using Agendor.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Agendor.Application.Dto.Pacientes.PacienteDto;

namespace Agendor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacienteController(
            IPacienteService service,
            ILogger<PacienteController> logger
        )
        : ControllerBase
    {
        private readonly IPacienteService _service = service;
        private readonly ILogger<PacienteController> _logger = logger;

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PacienteResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<PacienteResponseDto>> Create([FromBody] PacienteCreateDto dto)
        {
            var ct = HttpContext.RequestAborted;

            try
            {
                var created = await _service.CreateAsync(dto, ct);
                _logger.LogInformation("Criado paciente {PacienteId}", created.PacienteId);
                return CreatedAtAction(nameof(GetById), new { id = created.PacienteId }, created);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("CPF", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(Problem(title: "Conflito de dados", detail: ex.Message, statusCode: StatusCodes.Status409Conflict));
            }

        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PacienteResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<PacienteResponseDto>> GetById(Guid id)
        {
            var ct = HttpContext.RequestAborted;
            var item = await _service.GetByIdAsync(id, ct);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PacienteResponseDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<IEnumerable<PacienteResponseDto>>> List()
        {
            var ct = HttpContext.RequestAborted;
            var itens = await _service.GetAllAsync(ct);
            _logger.LogInformation("Listando {count} pacientes", itens.Count());
            return Ok(itens);
        }

        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> Update(Guid id, [FromBody] PacienteUpdateDto dto)
        {
            var ct = HttpContext.RequestAborted;
            dto.PacienteId = id;
            try
            {
                await _service.UpdateAsync(dto, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("CPF", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(Problem(title: "Conflito de dados", detail: ex.Message, statusCode: StatusCodes.Status409Conflict));
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ct = HttpContext.RequestAborted;
            var ok = await _service.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }
    }
}
