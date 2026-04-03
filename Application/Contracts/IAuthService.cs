using Application.Dto;
using Application.DTOs;

namespace Application.Contracts
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<string> RegisterAsync(RegisterDto registerDto);
        Task<bool> ConfirmEmailAsync(string userId, string token);
    }
}