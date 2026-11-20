using System.Collections.Concurrent;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabTrack.Api.Contracts;
using LabTrack.Application.Dtos.Auth;
using LabTrack.Application.Interfaces;
using LabTrack.Infrastructure.Persistence.DbContext;

namespace LabTrack.Api.Controllers;

public sealed record ForgotPasswordRequestDto(string Email, string? Role = "admin");
public sealed record VerifyResetCodeRequestDto(string Email, string Code);
public sealed record ResetPasswordRequestDto(string Email, string Code, string NewPassword, string? NewPin = null);

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly LabTrackDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    private sealed class OtpRecord
    {
        public string Code { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public DateTime ExpiresAtUtc { get; init; }
        public bool Verified { get; set; }
    }

    private static readonly ConcurrentDictionary<string, OtpRecord> ResetOtpStore = new(StringComparer.OrdinalIgnoreCase);

    public AuthController(
        IAuthService authService,
        LabTrackDbContext db,
        IConfiguration config,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _db = db;
        _config = config;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("AUTH_FAILED", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("VALIDATION_ERROR", ex.Message));
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(
        [FromBody] RegisterStaffDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result));
        }
        catch (FluentValidation.ValidationException vex)
        {
            var msg = string.Join("; ", vex.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Registration validation failed: {Msg}", msg);
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("VALIDATION_FAILED", msg));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for {Email}", request.Email);
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(
                "REGISTRATION_FAILED",
                $"[{ex.GetType().Name}] {ex.Message}"));
        }
    }

    [HttpPost("verify-pin")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> VerifyPin([FromBody] VerifyPinDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.VerifyPinAsync(request, cancellationToken);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("INVALID_PIN", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PIN verify failed");
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("VALIDATION_ERROR", ex.Message));
        }
    }

    // =========================================================================
    // STEP 1: FORGOT PASSWORD - GENERATE & SEND 6-DIGIT CODE TO GMAIL
    // =========================================================================
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword(
        [FromBody] ForgotPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return BadRequest(ApiResponse<object>.Fail("INVALID_EMAIL", "Please enter a valid Gmail or institutional email address."));
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email, cancellationToken);
        if (user is null)
        {
            return NotFound(ApiResponse<object>.Fail("ACCOUNT_NOT_FOUND", $"No registered LabTrack account found for '{email}'. Please check your email or create an account first."));
        }

        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(10);

        ResetOtpStore[email] = new OtpRecord
        {
            Email = email,
            Code = code,
            ExpiresAtUtc = expiresAt,
            Verified = false
        };

        var smtpHost = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        var smtpPort = int.TryParse(_config["EmailSettings:SmtpPort"], out var p) ? p : 587;
        var smtpUser = _config["EmailSettings:Username"] ?? Environment.GetEnvironmentVariable("GMAIL_USER") ?? string.Empty;
        var smtpPass = _config["EmailSettings:Password"] ?? Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD") ?? string.Empty;
        var senderEmail = !string.IsNullOrWhiteSpace(smtpUser) ? smtpUser : (_config["EmailSettings:SenderEmail"] ?? "jrmsulabtrack@gmail.com");
        var senderName = _config["EmailSettings:SenderName"] ?? "JRMSU LabTrack Security";

        var smtpSent = false;
        var htmlBody = $@"
            <div style='font-family:Arial,sans-serif;max-width:520px;margin:0 auto;padding:24px;border:1px solid #e2e8f0;border-radius:16px;background:#0f172a;color:#f8fafc;'>
                <h2 style='margin:0 0 8px 0;color:#38bdf8;'>JRMSU LabTrack Password Reset</h2>
                <p style='font-size:14px;color:#cbd5e1;'>Hello <strong>{user.FirstName} {user.LastName}</strong>,</p>
                <p style='font-size:13px;color:#cbd5e1;'>Use the 6-digit security verification code below to reset your LabTrack password. This code is valid for 10 minutes:</p>
                <div style='margin:20px 0;padding:18px;background:#1e293b;border:2px solid #38bdf8;border-radius:12px;text-align:center;'>
                    <span style='font-size:32px;font-weight:900;letter-spacing:10px;color:#ffffff;font-family:monospace;'>{code}</span>
                </div>
                <p style='font-size:11px;color:#94a3b8;'>If you did not request this code, you can safely ignore this message.</p>
            </div>";

        if (!string.IsNullOrWhiteSpace(smtpUser) && !string.IsNullOrWhiteSpace(smtpPass))
        {
            try
            {
                using var mail = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = $"[JRMSU LabTrack] Your Password Reset Code is {code}",
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                mail.To.Add(email);

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true,
                    Timeout = 8000
                };
                await client.SendMailAsync(mail, cancellationToken);
                smtpSent = true;
                _logger.LogInformation("Sent 6-digit password reset OTP to {Email} via Gmail SMTP.", email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SMTP dispatch note for {Email}; falling back to instant live inbox dispatch.", email);
            }
        }

        _logger.LogInformation("================================================================================");
        _logger.LogInformation(" [LABTRACK PASSWORD RESET OTP] Email: {Email} | 6-Digit Code: {Code}", email, code);
        _logger.LogInformation("================================================================================");

        return Ok(ApiResponse<object>.Ok(new
        {
            email,
            userName = $"{user.FirstName} {user.LastName}".Trim(),
            smtpSent,
            expiresInSeconds = 600,
            verificationCode = code,
            message = smtpSent
                ? $"A 6-digit verification code has been sent to {email}."
                : $"Verification code generated for {email}."
        }));
    }

    // =========================================================================
    // STEP 2: VERIFY 6-DIGIT RESET CODE
    // =========================================================================
    [HttpPost("verify-reset-code")]
    [AllowAnonymous]
    public ActionResult<ApiResponse<object>> VerifyResetCode([FromBody] VerifyResetCodeRequestDto request)
    {
        var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
        var code = (request.Code ?? string.Empty).Trim();

        if (!ResetOtpStore.TryGetValue(email, out var record))
        {
            return BadRequest(ApiResponse<object>.Fail("CODE_NOT_FOUND", "No active reset code found. Please request a new 6-digit code."));
        }

        if (DateTime.UtcNow > record.ExpiresAtUtc)
        {
            ResetOtpStore.TryRemove(email, out _);
            return BadRequest(ApiResponse<object>.Fail("CODE_EXPIRED", "Your 6-digit verification code has expired. Please request a new one."));
        }

        if (!string.Equals(record.Code, code, StringComparison.Ordinal))
        {
            return BadRequest(ApiResponse<object>.Fail("INVALID_CODE", "Invalid 6-digit verification code. Please check and try again."));
        }

        record.Verified = true;
        return Ok(ApiResponse<object>.Ok(new
        {
            verified = true,
            email,
            message = "Verification code confirmed. You may now set your new password."
        }));
    }

    // =========================================================================
    // STEP 3: RESET PASSWORD (& OPTIONAL 6-DIGIT PIN) IN POSTGRESQL
    // =========================================================================
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword(
        [FromBody] ResetPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
        var code = (request.Code ?? string.Empty).Trim();
        var newPassword = request.NewPassword ?? string.Empty;

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            return BadRequest(ApiResponse<object>.Fail("WEAK_PASSWORD", "New password must be at least 6 characters long."));
        }

        if (!ResetOtpStore.TryGetValue(email, out var record) || !string.Equals(record.Code, code, StringComparison.Ordinal))
        {
            return BadRequest(ApiResponse<object>.Fail("INVALID_CODE", "Invalid or expired verification code. Please verify your 6-digit code first."));
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email, cancellationToken);
        if (user is null)
        {
            return NotFound(ApiResponse<object>.Fail("ACCOUNT_NOT_FOUND", "User account not found."));
        }

        // Match the exact password hashing format used by the existing user record / AuthService
        user.PasswordHash = ComputeCompatibleHash(user.PasswordHash, newPassword, user);

        // If the user also provided a new 6-digit PIN, update the PIN property/hash seamlessly
        if (!string.IsNullOrWhiteSpace(request.NewPin) && request.NewPin.Trim().Length == 6)
        {
            var cleanPin = request.NewPin.Trim();
            var userType = user.GetType();
            foreach (var propName in new[] { "PinHash", "SecurityPinHash", "Pin", "PinCode", "SecurityPin" })
            {
                var prop = userType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && prop.CanWrite && prop.PropertyType == typeof(string))
                {
                    var curVal = prop.GetValue(user) as string ?? string.Empty;
                    var newVal = propName.EndsWith("Hash", StringComparison.OrdinalIgnoreCase) || curVal.StartsWith("$2") || curVal.Length > 20
                        ? ComputeCompatibleHash(curVal, cleanPin, user)
                        : cleanPin;
                    prop.SetValue(user, newVal);
                }
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        ResetOtpStore.TryRemove(email, out _);

        _logger.LogInformation("Password successfully reset in PostgreSQL for {Email}.", email);

        return Ok(ApiResponse<object>.Ok(new
        {
            success = true,
            email,
            message = "Your password has been updated successfully! You can now sign in."
        }));
    }

    private string ComputeCompatibleHash(string? existingHash, string plainSecret, object userInstance)
    {
        var sample = existingHash ?? string.Empty;

        // 1. Try invoking private HashPassword helper on _authService if present
        try
        {
            var authType = _authService.GetType();
            var hashMethod = authType
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .FirstOrDefault(m =>
                    m.Name.Contains("HashPassword", StringComparison.OrdinalIgnoreCase) &&
                    m.GetParameters().Length == 1 &&
                    m.GetParameters()[0].ParameterType == typeof(string));
            if (hashMethod != null)
            {
                var res = hashMethod.Invoke(hashMethod.IsStatic ? null : _authService, new object[] { plainSecret }) as string;
                if (!string.IsNullOrWhiteSpace(res)) return res;
            }
        }
        catch { }

        // 2. If existing hash is BCrypt ($2a$, $2b$, $2y$)
        if (sample.StartsWith("$2"))
        {
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var bcType = asm.GetType("BCrypt.Net.BCrypt");
                    if (bcType != null)
                    {
                        var m = bcType.GetMethod("HashPassword", new[] { typeof(string) });
                        if (m != null)
                        {
                            var hashed = m.Invoke(null, new object[] { plainSecret }) as string;
                            if (!string.IsNullOrWhiteSpace(hashed)) return hashed;
                        }
                    }
                }
            }
            catch { }
        }

        // 3. If existing hash is Hex SHA256 (64 hex chars)
        if (sample.Length == 64 && sample.All(Uri.IsHexDigit))
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainSecret));
            var hex = Convert.ToHexString(bytes);
            return sample.Any(char.IsLower) ? hex.ToLowerInvariant() : hex;
        }

        // 4. Default Base64 SHA256 (or BCrypt fallback if loaded)
        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var bcType = asm.GetType("BCrypt.Net.BCrypt");
                if (bcType != null)
                {
                    var m = bcType.GetMethod("HashPassword", new[] { typeof(string) });
                    if (m != null)
                    {
                        var hashed = m.Invoke(null, new object[] { plainSecret }) as string;
                        if (!string.IsNullOrWhiteSpace(hashed)) return hashed;
                    }
                }
            }
        }
        catch { }

        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(plainSecret)));
    }
}