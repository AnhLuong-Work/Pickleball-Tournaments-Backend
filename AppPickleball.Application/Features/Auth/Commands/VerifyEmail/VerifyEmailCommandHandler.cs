using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using MediatR;
using Microsoft.Extensions.Localization;
using Shared.Kernel.Resources;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, ApiResponse<VerifyEmailResponseDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public VerifyEmailCommandHandler(IUserRepository userRepo, IUnitOfWork uow, ICurrentUserService currentUser, IStringLocalizer<SharedResource> localizer)
    {
        _userRepo = userRepo; _uow = uow; _currentUser = currentUser;
        _localizer = localizer;
    }

    public async Task<ApiResponse<VerifyEmailResponseDto>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("User không tồn tại");

        if (user.EmailVerified)
            throw new DomainException(_localizer["Email_AlreadyVerified"]);

        if (user.EmailVerificationToken == null || user.EmailVerificationTokenExpiresAt == null)
            throw new DomainException(_localizer["OTP_NotFound"]);

        if (DateTime.UtcNow > user.EmailVerificationTokenExpiresAt)
            throw new DomainException(_localizer["OTP_Expired"]);

        var otpHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(request.Otp))).ToLower();
        if (user.EmailVerificationToken != otpHash)
            throw new DomainException(_localizer["OTP_Invalid"]);

        user.EmailVerified = true;
        user.EmailVerifiedAt = DateTime.UtcNow;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiresAt = null;
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse<VerifyEmailResponseDto>.SuccessResponse(
            new VerifyEmailResponseDto(true, user.EmailVerifiedAt!.Value));
    }
}
