namespace AppPickleball.Application.Common.Settings
{
    public class AuthSettings
    {
        public int AccessTokenExpiryMinutes { get; set; } = 30;
        public int RefreshTokenExpiryDays { get; set; } = 7;
        public int SessionExpirationDays { get; set; } = 7;
        public string FrontendUrl { get; set; } = string.Empty;
        public string Domain { get; set; } = ".yourapp.com";
    }
}
