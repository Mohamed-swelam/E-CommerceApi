using Core.DTOs.Account;
using Core.DTOs.Auth;
using Core.DTOs.GeneralDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Interfaces
{
    public interface IAuthService
    {
        Task<GeneralResponse> RegisterAsync(RegisterDto dto);
        Task<GeneralResponse> LoginAsync(LoginDto dto);
        Task<GeneralResponse> LogoutAsync(string userId);
        Task<GeneralResponse> ConfirmEmailAsync(ConfirmEmailDto dto);
        Task<GeneralResponse> ResendConfirmationCodeAsync(string email);
        Task<GeneralResponse> ForgetPasswordAsync(string email);
        Task<GeneralResponse> VerifyResetCodeAsync(VerifyCodeDto dto);
        Task<GeneralResponse> ResetPasswordAsync(ResetPasswordDto dto);
        Task<GeneralResponse> ChangePasswordAsync(ChangePasswordDto dto, string userId);
        Task<GeneralResponse> RefreshTokenAsync(TokenRequestDto dto);
        Task<GeneralResponse> RevokeAllTokensAsync(string userId);
    }
}
