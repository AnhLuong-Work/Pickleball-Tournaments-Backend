using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ApiResponse<ResetPasswordResponseDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _uow;

    public ResetPasswordCommandHandler(IUserRepository userRepo, IPasswordHasher passwordHasher, IUnitOfWork uow)
    {
        _userRepo = userRepo; _passwordHasher = passwordHasher; _uow = uow;
    }

    public async Task<ApiResponse<ResetPasswordResponseDto>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new DomainException("Email hoặc OTP không hợp lệ");

        if (user.PasswordResetToken == null || user.PasswordResetTokenExpiresAt == null)
            throw new DomainException("Chưa có yêu cầu đặt lại mật khẩu. Vui lòng gửi lại OTP");

        if (DateTime.UtcNow > user.PasswordResetTokenExpiresAt)
            throw new DomainException("OTP đã hết hạn. Vui lòng gửi lại OTP");

        var otpHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(request.Otp))).ToLower();

        if (user.PasswordResetToken != otpHash)
            throw new DomainException("Email hoặc OTP không hợp lệ");

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse<ResetPasswordResponseDto>.SuccessResponse(
            new ResetPasswordResponseDto("Mật khẩu đã được đặt lại thành công. Vui lòng đăng nhập lại"));
    }
}
