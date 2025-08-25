using Agendor.Application.Dto.Auth;
using Agendor.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Agendor.Application.Dto.Auth.RegisterDto;

namespace Agendor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController
        (
            IAuthService service,
            ILogger<AuthController> logger
        )
        : ControllerBase
    {
        private readonly IAuthService _service = service;
        private readonly ILogger<AuthController> _logger = logger;

        [HttpPost("login")]
        [AllowAnonymous]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
        {
            var ct = HttpContext.RequestAborted;
            try
            {
                var result = await _service.AuthenticateAsync(dto);
                _logger.LogInformation("Login efetuado para {Email}", dto.Email);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Falha de login para {Email}", dto.Email);
                return Unauthorized(Problem(title: "Credenciais inválidas", detail: ex.Message, statusCode: StatusCodes.Status401Unauthorized));
            }
        }

        [HttpPost("register/paciente")]
        [AllowAnonymous]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> RegisterPaciente([FromBody] RegisterPacienteDto dto)
        {
            var ct = HttpContext.RequestAborted;
            try
            {
                var ok = await _service.RegisterPacienteAsync(dto);
                return StatusCode(StatusCodes.Status201Created, new { success = ok });
            }
            catch (InvalidOperationException ex)
            {
                // conflitos de e-mail/CPF
                return Conflict(Problem(title: "Conflito de dados", detail: ex.Message, statusCode: StatusCodes.Status409Conflict));
            }
        }

        [HttpPost("register/medico")]
        [AllowAnonymous]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> RegisterMedico([FromBody] RegisterMedicoDto dto)
        {
            var ct = HttpContext.RequestAborted;
            try
            {
                var ok = await _service.RegisterMedicoAsync(dto);
                return StatusCode(StatusCodes.Status201Created, new { success = ok });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(Problem(title: "Conflito de dados", detail: ex.Message, statusCode: StatusCodes.Status409Conflict));
            }
        }

        [HttpPost("confirm")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Confirm([FromBody] RegisterResponseDto dto)
        {
            await _service.ConfirmRegisterAsync(dto);
            return NoContent();
        }
    }
}