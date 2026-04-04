using Application.Contracts;
using Application.Dto;
using Application.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(UserManager<IdentityUser> userManager, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<string> RegisterAsync(RegisterDto registerDto)
        {
            var user = new IdentityUser
            {
                Email = registerDto.Email,
                UserName = registerDto.Email
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }

            // Generate email confirmation token
            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedConfirmationToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));

            // Build confirmation link
            var confirmationLink = $"https://localhost:7216/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(encodedConfirmationToken)}";

            // Send confirmation email
            var emailBody = $@"
                <h2>Welcome to Clean Architecture Demo!</h2>
                <p>Please confirm your email by clicking the link below:</p>
                <a href='{confirmationLink}'>Confirm Email</a>
                <p>This link will expire after use.</p>";

            await _emailService.SendEmailAsync(user.Email!, "Confirm Your Email", emailBody);

            return user.Id;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                throw new Exception("Invalid email or password.");

            // Check if account is locked out
            if (await _userManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var minutesLeft = Math.Ceiling((lockoutEnd!.Value - DateTimeOffset.UtcNow).TotalMinutes);
                throw new Exception($"Account is locked. Please try again in {minutesLeft} minute(s).");
            }

            // Block login if email is not confirmed
            if (!user.EmailConfirmed)
                throw new Exception("Please confirm your email before logging in.");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!isPasswordValid)
            {
                // Increment failed access count
                await _userManager.AccessFailedAsync(user);

                var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
                var attemptsLeft = 5 - failedAttempts;

                if (await _userManager.IsLockedOutAsync(user))
                    throw new Exception("Too many failed attempts. Account locked for 5 minutes.");

                throw new Exception($"Invalid email or password. {attemptsLeft} attempt(s) remaining.");
            }

            // Reset failed count on successful login
            await _userManager.ResetAccessFailedCountAsync(user);

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email!,
                UserId = user.Id
            };
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            // Decode URL encoding first, then Base64Url decode
            var urlDecodedToken = Uri.UnescapeDataString(token);
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(urlDecodedToken));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }

            return true;
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !user.EmailConfirmed)
                return; // Silently return to prevent email enumeration

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));
            var resetLink = $"https://localhost:7216/api/auth/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(encodedToken)}";

            var emailBody = $@"
                <h2>Reset Your Password</h2>
                <p>You requested a password reset. Click the link below to set a new password:</p>
                <a href='{resetLink}'>Reset Password</a>
                <p>If you did not request this, please ignore this email.</p>
                <p>This link will expire after use.</p>";

            await _emailService.SendEmailAsync(user.Email!, "Reset Your Password", emailBody);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
                throw new Exception("User not found.");

            var urlDecodedToken = Uri.UnescapeDataString(resetPasswordDto.Token);
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(urlDecodedToken));

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, resetPasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }
        }

        public async Task ResendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found.");

            if (user.EmailConfirmed)
                throw new Exception("Email is already confirmed.");

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedConfirmationToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));
            var confirmationLink = $"https://localhost:7216/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(encodedConfirmationToken)}";

            var emailBody = $@"
                <h2>Confirm Your Email</h2>
                <p>You requested a new confirmation link. Please confirm your email by clicking below:</p>
                <a href='{confirmationLink}'>Confirm Email</a>
                <p>This link will expire after use.</p>";

            await _emailService.SendEmailAsync(user.Email!, "Confirm Your Email", emailBody);
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtSettings:Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}