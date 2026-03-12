using AppPickleball.Application.Features.Auth.Commands.ChangePassword;
using AppPickleball.Application.Features.Auth.Commands.FacebookLogin;
using AppPickleball.Application.Features.Auth.Commands.ForgotPassword;
using AppPickleball.Application.Features.Auth.Commands.GoogleLogin;
using AppPickleball.Application.Features.Auth.Commands.Login;
using AppPickleball.Application.Features.Auth.Commands.RefreshToken;
using AppPickleball.Application.Features.Auth.Commands.Register;
using AppPickleball.Application.Features.Auth.Commands.ResetPassword;
using AppPickleball.Application.Features.Auth.Commands.SendEmailVerification;
using AppPickleball.Application.Features.Auth.Commands.VerifyEmail;
using AppPickleball.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernel.Wrappers;
using Swashbuckle.AspNetCore.Annotations;

namespace AppPickleball.Api.Controllers;

/// <summary>Xác thực và quản lý phiên đăng nhập</summary>
[Route("api/auth")]
[SwaggerTag("Authentication — đăng ký, đăng nhập, refresh token, xác thực email, quên mật khẩu")]
[Produces("application/json")]
public class AuthController : BaseApi.BaseApiController
{
    public AuthController(IMediator mediator, ILogger<BaseApi.BaseApiController> logger)
        : base(mediator, logger) { }

    /// <summary>1.1 POST /auth/register — Đăng ký tài khoản</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Đăng ký tài khoản mới",
        Description = "Tạo tài khoản mới bằng email và mật khẩu. Sau khi đăng ký thành công, gửi OTP xác thực email qua `/auth/send-verification`.")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegisterCommand(request.Email, request.Password, request.Name), ct);
        return StatusCode(201, result);
    }

    /// <summary>1.2 POST /auth/login — Đăng nhập</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Đăng nhập",
        Description = "Xác thực email/password, trả về access token (15 phút) và refresh token (7 ngày). " +
                      "Access token dùng trong header `Authorization: Bearer {token}`.")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginCommand(request.Email, request.Password), ct);
        return Ok(result);
    }

    /// <summary>1.4 POST /auth/refresh — Làm mới access token</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Làm mới access token",
        Description = "Dùng refresh token để lấy access token mới. Refresh token được xoay vòng (rotation) — " +
                      "token cũ bị thu hồi ngay sau khi dùng. Nếu dùng lại token đã revoke → toàn bộ session bị đăng xuất.")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), ct);
        return Ok(result);
    }

    /// <summary>1.5 PUT /auth/password — Đổi mật khẩu</summary>
    [HttpPut("password")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Đổi mật khẩu",
        Description = "Đổi mật khẩu khi đã đăng nhập. Yêu cầu nhập mật khẩu hiện tại để xác nhận. " +
                      "Nếu quên mật khẩu, dùng `/auth/forgot-password` thay thế.")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ChangePasswordCommand(request.CurrentPassword, request.NewPassword), ct);
        return Ok(result);
    }

    /// <summary>1.6 POST /auth/send-verification — Gửi OTP xác thực email</summary>
    [HttpPost("send-verification")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Gửi OTP xác thực email",
        Description = "Gửi mã OTP 6 số đến email của tài khoản hiện tại. OTP có hiệu lực 10 phút. " +
                      "Chỉ dùng được khi email chưa được xác thực.")]
    [ProducesResponseType(typeof(ApiResponse<SendVerificationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendVerification(CancellationToken ct)
    {
        var result = await _mediator.Send(new SendEmailVerificationCommand(), ct);
        return Ok(result);
    }

    /// <summary>1.7 POST /auth/verify-email — Xác thực email bằng OTP</summary>
    [HttpPost("verify-email")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Xác thực email",
        Description = "Xác thực email bằng OTP nhận từ `/auth/send-verification`. " +
                      "Sau khi xác thực thành công, tài khoản mới được phép tạo giải đấu.")]
    [ProducesResponseType(typeof(ApiResponse<VerifyEmailResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new VerifyEmailCommand(request.Otp), ct);
        return Ok(result);
    }

    /// <summary>1.8 POST /auth/forgot-password — Quên mật khẩu</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Quên mật khẩu — Gửi OTP đặt lại",
        Description = "Gửi mã OTP 6 số đến email để đặt lại mật khẩu. OTP có hiệu lực 10 phút. " +
                      "Response luôn trả 200 dù email có tồn tại hay không (bảo mật — tránh email enumeration). " +
                      "Dùng OTP nhận được để gọi `/auth/reset-password`.")]
    [ProducesResponseType(typeof(ApiResponse<ForgotPasswordResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ForgotPasswordCommand(request.Email), ct);
        return Ok(result);
    }

    /// <summary>1.9 POST /auth/reset-password — Đặt lại mật khẩu bằng OTP</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Đặt lại mật khẩu",
        Description = "Đặt lại mật khẩu mới bằng OTP nhận từ `/auth/forgot-password`. " +
                      "Sau khi đặt lại thành công, OTP bị xóa và cần đăng nhập lại.")]
    [ProducesResponseType(typeof(ApiResponse<ResetPasswordResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ResetPasswordCommand(request.Email, request.Otp, request.NewPassword), ct);
        return Ok(result);
    }

    /// <summary>1.10 POST /auth/google-login — Đăng nhập bằng Google</summary>
    [HttpPost("google-login")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Đăng nhập bằng Google SSO",
        Description = "Xác thực ID Token từ Google SDK. Backend verify với Google, " +
                      "tự động tạo tài khoản nếu chưa tồn tại. Trả về `isNewUser = true` nếu tài khoản vừa được tạo — " +
                      "client nên redirect đến trang hoàn thiện hồ sơ. " +
                      "Không cần đăng ký trước — luồng register + login gộp làm một.")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new GoogleLoginCommand(request.IdToken), ct);
        return Ok(result);
    }

    /// <summary>1.11 POST /auth/facebook-login — Đăng nhập bằng Facebook</summary>
    [HttpPost("facebook-login")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Đăng nhập bằng Facebook SSO",
        Description = "Xác thực Access Token từ Facebook SDK bằng Graph API. " +
                      "Backend tự động tạo tài khoản nếu chưa tồn tại. " +
                      "Nếu Facebook không cung cấp email (user từ chối cấp quyền), " +
                      "tài khoản được tạo với placeholder email `fb_{id}@facebook.placeholder`. " +
                      "Trả về `isNewUser = true` nếu tài khoản vừa được tạo.")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new FacebookLoginCommand(request.AccessToken), ct);
        return Ok(result);
    }
}

// Request DTOs
public record RegisterRequest(string Email, string Password, string Name);
public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record VerifyEmailRequest(string Otp);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Otp, string NewPassword);
public record GoogleLoginRequest(string IdToken);
public record FacebookLoginRequest(string AccessToken);
