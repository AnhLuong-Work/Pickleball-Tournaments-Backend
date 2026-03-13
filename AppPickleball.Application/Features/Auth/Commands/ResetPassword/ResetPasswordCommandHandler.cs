using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using MediatR;
using Microsoft.Extensions.Localization;
using Shared.Kernel.Resources;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ApiResponse<ResetPasswordResponseDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _uow;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ResetPasswordCommandHandler(IUserRepository userRepo, IPasswordHasher passwordHasher, IUnitOfWork uow, IStringLocalizer<SharedResource> localizer)
    {
        _userRepo = userRepo; _passwordHasher = passwordHasher; _uow = uow;
        _localizer = localizer;
    }

    public async Task<ApiResponse<ResetPasswordResponseDto>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new DomainException(_localizer["OTP_Invalid"]);

        if (user.PasswordResetToken == null || user.PasswordResetTokenExpiresAt == null)
            throw new DomainException(_localizer["NoReset_Found"]);

        if (DateTime.UtcNow > user.PasswordResetTokenExpiresAt)
            throw new DomainException(_localizer["OTP_Expired"]);

        var otpHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(request.Otp))).ToLower();

        if (user.PasswordResetToken != otpHash)
            throw new DomainException(_localizer["OTP_Invalid"]);

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse<ResetPasswordResponseDto>.SuccessResponse(
            new ResetPasswordResponseDto(_localizer["ResetPassword_Success"]));
    }
}
