using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using AppPickleball.Application.Features.Users.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;
using System.Security.Cryptography;
using Shared.Kernel.Resources;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ApiResponse<ForgotPasswordResponseDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _emailService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ForgotPasswordCommandHandler(IUserRepository userRepo, IUnitOfWork uow, IEmailService emailService, IStringLocalizer<SharedResource> localizer)
    {
        _userRepo = userRepo; _uow = uow; _emailService = emailService;
        _localizer = localizer;
    }

    public async Task<ApiResponse<ForgotPasswordResponseDto>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email, cancellationToken);

        // Không tiết lộ email có tồn tại hay không (security best practice)
        if (user == null)
            return ApiResponse<ForgotPasswordResponseDto>.SuccessResponse(
                new ForgotPasswordResponseDto(_localizer["ForgotPassword_Success"], 600));

        // Generate 6-digit OTP
        var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var otpHash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(otp))).ToLower();

        user.PasswordResetToken = otpHash;
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(10);
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(cancellationToken);

        await _emailService.SendEmailAsync(user.Email,
            "Đặt lại mật khẩu AppPickleball",
            $"Mã OTP đặt lại mật khẩu của bạn là: <strong>{otp}</strong>. Có hiệu lực trong 10 phút.<br>Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.",
            cancellationToken);

        return ApiResponse<ForgotPasswordResponseDto>.SuccessResponse(
            new ForgotPasswordResponseDto(_localizer["ForgotPassword_Success"], 600));
    }
}
