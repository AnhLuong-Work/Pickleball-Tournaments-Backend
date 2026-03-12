using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using MediatR;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, ApiResponse<VerifyEmailResponseDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public VerifyEmailCommandHandler(IUserRepository userRepo, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _userRepo = userRepo; _uow = uow; _currentUser = currentUser;
    }

    public async Task<ApiResponse<VerifyEmailResponseDto>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("User không tồn tại");

        if (user.EmailVerified)
            throw new DomainException("Email đã được xác thực");

        if (user.EmailVerificationToken == null || user.EmailVerificationTokenExpiresAt == null)
            throw new DomainException("Chưa có OTP. Vui lòng gửi lại OTP");

        if (DateTime.UtcNow > user.EmailVerificationTokenExpiresAt)
            throw new DomainException("OTP đã hết hạn");

        var otpHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(request.Otp))).ToLower();
        if (user.EmailVerificationToken != otpHash)
            throw new DomainException("OTP không đúng");

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
