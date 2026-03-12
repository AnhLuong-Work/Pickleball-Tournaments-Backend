using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using MediatR;
using System.Security.Cryptography;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ApiResponse<ForgotPasswordResponseDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IUserRepository userRepo, IUnitOfWork uow, IEmailService emailService)
    {
        _userRepo = userRepo; _uow = uow; _emailService = emailService;
    }

    public async Task<ApiResponse<ForgotPasswordResponseDto>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email, cancellationToken);

        // Không tiết lộ email có tồn tại hay không (security best practice)
        if (user == null)
            return ApiResponse<ForgotPasswordResponseDto>.SuccessResponse(
                new ForgotPasswordResponseDto("Nếu email tồn tại, OTP đã được gửi đến hộp thư của bạn", 600));

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
            new ForgotPasswordResponseDto("Nếu email tồn tại, OTP đã được gửi đến hộp thư của bạn", 600));
    }
}
