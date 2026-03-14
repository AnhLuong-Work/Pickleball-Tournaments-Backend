namespace AppPickleball.Api.Controllers;

public record RegisterRequest(string Email, string Password, string Name);
public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record VerifyEmailRequest(string Otp);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Otp, string NewPassword);
public record GoogleLoginRequest(string IdToken);
public record FacebookLoginRequest(string AccessToken);
