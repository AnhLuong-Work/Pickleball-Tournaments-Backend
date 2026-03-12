using AppPickleball.Application.Common.Exceptions;
using AppPickleball.Application.Common.Interfaces;
using AppPickleball.Application.Common.Services;
using MediatR;
using System.Security.Cryptography;
using Shared.Kernel.Wrappers;

namespace AppPickleball.Application.Features.Auth.Commands.SendEmailVerification;

public class SendEmailVerificationCommandHandler : IRequestHandler<SendEmailVerificationCommand, ApiResponse<SendVerificationResponseDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUser;

    public SendEmailVerificationCommandHandler(IUserRepository userRepo, IUnitOfWork uow,
        IEmailService emailService, ICurrentUserService currentUser)
    {
        _userRepo = userRepo; _uow = uow;
        _emailService = emailService; _currentUser = currentUser;
    }

    public async Task<ApiResponse<SendVerificationResponseDto>> Handle(SendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("User không tồn tại");

        if (user.EmailVerified)
            throw new DomainException("Email đã được xác thực");

        // Generate 6-digit OTP
        var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var otpHash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(otp))).ToLower();

        user.EmailVerificationToken = otpHash;
        user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddMinutes(10);
        _userRepo.Update(user);
        await _uow.SaveChangesAsync(cancellationToken);

        await _emailService.SendEmailAsync(user.Email,
            "Xác thực email AppPickleball",
            $"Mã OTP của bạn là: <strong>{otp}</strong>. Có hiệu lực trong 10 phút.",
            cancellationToken);

        return ApiResponse<SendVerificationResponseDto>.SuccessResponse(
            new SendVerificationResponseDto("OTP đã được gửi đến email của bạn", 600));
    }
}
