using Agendor.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Agendor.Application.Dto.Usuarios.UsuarioDto;

namespace Agendor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController
        (
            IUsuarioService service,
            ILogger<UsuarioController> logger
        )
        : ControllerBase
    {
        private readonly IUsuarioService _service = service;
        private readonly ILogger<UsuarioController> _logger = logger;

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UsuarioResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<UsuarioResponseDto>> Create([FromBody] UsuarioCreateDto dto)
        {
            var ct = HttpContext.RequestAborted;

            try
            {
                var created = await _service.CreateAsync(dto, ct);
                _logger.LogInformation("Criado usuário");
                return CreatedAtAction(nameof(GetById), created);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Email", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(Problem(title: "Conflito de dados", detail: ex.Message, statusCode: StatusCodes.Status409Conflict));
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UsuarioResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UsuarioResponseDto>> GetById(Guid id)
        {
            var ct = HttpContext.RequestAborted;
            var item = await _service.GetByIdAsync(id, ct);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UsuarioResponseDto>))]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> List()
        {
            var ct = HttpContext.RequestAborted;
            var itens = await _service.GetAllAsync(ct);
            _logger.LogInformation("Listando {count} usuários", itens.Count());
            return Ok(itens);
        }

        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> Update(Guid id, [FromBody] UsuarioUpdateDto dto)
        {
            var ct = HttpContext.RequestAborted;
            dto.UsuarioId = id;

            try
            {
                await _service.UpdateAsync(dto, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Email", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(Problem(title: "Conflito de dados", detail: ex.Message, statusCode: StatusCodes.Status409Conflict));
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ct = HttpContext.RequestAborted;
            var ok = await _service.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }
    }
}