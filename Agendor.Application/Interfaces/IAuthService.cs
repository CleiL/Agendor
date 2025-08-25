using Agendor.Application.Dto.Auth;
using static Agendor.Application.Dto.Auth.RegisterDto;

namespace Agendor.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginDto dto);
        Task<bool> RegisterPacienteAsync(RegisterPacienteDto dto);
        Task<bool> RegisterMedicoAsync(RegisterMedicoDto dto);
        Task ConfirmRegisterAsync(RegisterResponseDto dto);
    }
}
