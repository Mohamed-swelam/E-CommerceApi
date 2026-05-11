using Core.DTOs.Account;
using Core.DTOs.Auth;
using Core.DTOs.GeneralDto;
using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace Services
{


    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            AppDbContext context,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _context = context;
            _config = config;
        }

        public async Task<GeneralResponse> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return Fail("This email is already taken, please choose another.");

            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return Fail(result.Errors.Select(e => e.Description).ToList());

            await _userManager.AddToRoleAsync(user, "Customer");

            await SendConfirmationEmailAsync(user);

            return Ok("Registration successful. Check your email for the verification code.");
        }

        
        public async Task<GeneralResponse> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || user.IsDeleted)
                return Fail("Invalid email or password.");

            if (!user.EmailConfirmed)
            {
                await SendConfirmationEmailAsync(user);
                return Fail("Please check your email to confirm your account.");
            }

            var isLocked = await _userManager.IsLockedOutAsync(user);
            if (isLocked)
                return Fail("Account is locked. Try again later.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
                return Fail("Invalid email or password.");

            var jwtToken = await GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                Token = jwtToken,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(30),
                FullName = user.FullName,
                Email = user.Email,
                Role = roles.FirstOrDefault()
            });
        }

        
        public async Task<GeneralResponse> LogoutAsync(string userId)
        {
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return Fail("User not found.");

            foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
                token.Revoked = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _signInManager.SignOutAsync();

            return Ok("Signed out successfully.");
        }

        
        public async Task<GeneralResponse> ConfirmEmailAsync(ConfirmEmailDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Fail("User not found.");

            var storedCode = await _userManager.GetAuthenticationTokenAsync(
                user, "EmailVerification", "VerificationCode");

            if (storedCode != dto.VerificationCode)
                return Fail("Invalid verification code.");

            user.EmailConfirmed = true;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return Fail("Email confirmation failed.");

            return Ok("Email successfully confirmed.");
        }

        
        public async Task<GeneralResponse> ResendConfirmationCodeAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Fail("User not found.");

            if (user.EmailConfirmed)
                return Fail("Email is already confirmed.");

            await SendConfirmationEmailAsync(user);

            return Ok("Verification code sent.");
        }

       
        public async Task<GeneralResponse> ForgetPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Ok("If this email is registered, a verification code has been sent.");

            try
            {
                var code = _emailService.GenerateVerificationCode();
                await _userManager.SetAuthenticationTokenAsync(user, "ResetPassword", "ResetPasswordCode", code);

                var body = BuildEmailTemplate("Password Reset Request",
                    "You requested to reset your password. Use the code below:",
                    code, "#e74c3c");

                await _emailService.SendEmailAsync(email, "Password Reset Code", body);

                return Ok("If this email is registered, a verification code has been sent.");
            }
            catch
            {
                return Fail("Failed to send email. Please try again later.");
            }
        }

        
        public async Task<GeneralResponse> VerifyResetCodeAsync(VerifyCodeDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Fail("User not found.");

            var savedCode = await _userManager.GetAuthenticationTokenAsync(
                user, "ResetPassword", "ResetPasswordCode");

            if (savedCode != dto.VerificationCode)
                return Fail("Invalid verification code.");

            await _userManager.RemoveAuthenticationTokenAsync(user, "ResetPassword", "ResetPasswordCode");

            var token = await GenerateJwtToken(user, isResetPassword: true);

            return Ok(token);
        }

       
        public async Task<GeneralResponse> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Fail("User not found.");

            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
                return Fail("Failed to reset password.");

            var addResult = await _userManager.AddPasswordAsync(user, dto.NewPassword);
            if (!addResult.Succeeded)
                return Fail("Invalid password format.");

            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Ok("Password has been changed successfully.");
        }

        
        public async Task<GeneralResponse> ChangePasswordAsync(ChangePasswordDto dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Fail("User not found.");

            var isCorrect = await _userManager.CheckPasswordAsync(user, dto.OldPassword);
            if (!isCorrect)
                return Fail("The old password is incorrect.");

            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
            if (!result.Succeeded)
                return Fail(result.Errors.Select(e => e.Description).ToList());

            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Ok("Password changed successfully.");
        }

        
        public async Task<GeneralResponse> RefreshTokenAsync(TokenRequestDto dto)
        {
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == dto.RefreshToken));

            if (user == null)
                return Fail("Invalid refresh token.");

            var refreshToken = user.RefreshTokens.Single(t => t.Token == dto.RefreshToken);

            if (!refreshToken.IsActive)
                return Fail("Refresh token has expired.");

            refreshToken.Revoked = DateTime.UtcNow;

            var newJwt = await GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            await _context.SaveChangesAsync();

            return Ok(new TokenRequestDto
            {
                Token = newJwt,
                RefreshToken = newRefreshToken
            });
        }

        
        public async Task<GeneralResponse> RevokeAllTokensAsync(string userId)
        {
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return Fail("User not found.");

            foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
                token.Revoked = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("All tokens have been revoked.");
        }

        
        private async Task SendConfirmationEmailAsync(ApplicationUser user)
        {
            var code = _emailService.GenerateVerificationCode();
            user.EmailConfirmed = false;
            await _userManager.SetAuthenticationTokenAsync(user, "EmailVerification", "VerificationCode", code);

            var body = BuildEmailTemplate("Email Verification",
                "Thank you for registering. Use the code below to verify your email:",
                code, "#3498db");

            await _emailService.SendEmailAsync(user.Email!, "Verify Your Email", body);
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user, bool isResetPassword = false)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name,           user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email,          user.Email    ?? string.Empty),
            new Claim("FullName",                user.FullName ?? string.Empty)
        };

            if (isResetPassword)
            {
                claims.Add(new Claim("Purpose", "ResetPassword"));
            }
            else
            {
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private string BuildEmailTemplate(string title, string message, string code, string color)
        {
            return $@"
        <div style='font-family:Arial,sans-serif;background:#f4f4f4;padding:20px;'>
          <div style='max-width:600px;margin:auto;background:#fff;padding:30px;border-radius:10px;box-shadow:0 2px 8px rgba(0,0,0,0.1);'>
            <h2 style='color:#2c3e50;'>{title}</h2>
            <p style='font-size:16px;color:#555;'>{message}</p>
            <div style='text-align:center;margin:30px 0;'>
              <span style='display:inline-block;padding:15px 30px;font-size:24px;
                           background:{color};color:#fff;border-radius:6px;font-weight:bold;letter-spacing:2px;'>
                {code}
              </span>
            </div>
            <p style='font-size:13px;color:#999;'>If you didn't request this, please ignore this email.<br/>
            <strong>E-Commerce Team</strong></p>
          </div>
        </div>";
        }

        private GeneralResponse Ok(object data) =>
            new GeneralResponse { IsSuccess = true, Data = data };

        private GeneralResponse Fail(object data) =>
            new GeneralResponse { IsSuccess = false, Data = data };
    }
}
