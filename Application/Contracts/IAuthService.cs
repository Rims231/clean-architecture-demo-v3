using Application.Dto;
using Application.DTOs;

namespace Application.Contracts
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<string> RegisterAsync(RegisterDto registerDto);
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task ResendConfirmationEmailAsync(string email);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }
}