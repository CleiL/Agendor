using Agendor.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Agendor.Application.Dto.Medicos.MedicosDto;

namespace Agendor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicoController(
            IMedicoService service,
            ILogger<MedicoController> logger
        )
        : ControllerBase
    {
        private readonly IMedicoService _service = service;
        private readonly ILogger<MedicoController> _logger = logger;

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MedicoResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<MedicoResponseDto>> Create([FromBody] MedicoCreateDto dto)
        {
            var ct = HttpContext.RequestAborted;

            try
            {
                var created = await _service.CreateAsync(dto, ct);
                _logger.LogInformation("Criado medico {MedicoId}", created.MedicoId);
                return CreatedAtAction(nameof(GetById), new { id = created.MedicoId }, created);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("CRM", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(Problem(title: "Conflito de dados", detail: ex.Message, statusCode: StatusCodes.Status409Conflict));
            }

        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MedicoResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<MedicoResponseDto>> GetById(Guid id)
        {
            var ct = HttpContext.RequestAborted;
            var item = await _service.GetByIdAsync(id, ct);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MedicoResponseDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<IEnumerable<MedicoResponseDto>>> List()
        {
            var ct = HttpContext.RequestAborted;
            var itens = await _service.GetAllAsync(ct);
            _logger.LogInformation("Listando {count} medicos", itens.Count());
            return Ok(itens);
        }

        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> Update(Guid id, [FromBody] MedicoUpdateDto dto)
        {
            var ct = HttpContext.RequestAborted;
            dto.MedicoId = id;
            try
            {
                await _service.UpdateAsync(dto, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("CRM", StringComparison.OrdinalIgnoreCase))
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