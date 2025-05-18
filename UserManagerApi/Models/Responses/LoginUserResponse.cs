namespace UserManagerApi.Models.Responses
{
    public class LoginUserResponse
    {
        public string Login { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
