using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UserManagerApi.Repositories;
using UserManagerApi.Models.Requests;
using UserManagerApi.Models.Responses;

public class JwtService
{
    private readonly IUserRepository _repo;
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiresInMinutes;

    public JwtService(IConfiguration config, IUserRepository repo)
    {
        _repo = repo;
        _key = config["JwtConfig:Key"] ?? throw new ArgumentNullException("JwtConfig:Key");
        _issuer = config["JwtConfig:Issuer"] ?? throw new ArgumentNullException("JwtConfig:Issuer");
        _audience = config["JwtConfig:Audience"] ?? throw new ArgumentNullException("JwtConfig:Audience");
        _expiresInMinutes = int.Parse(config["JwtConfig:ExpiresInMinutes"] ?? "60");
    }

    public LoginUserResponse? Authenticate(LoginUserRequest request)
    {
        var user = _repo.GetByLogin(request.Login.Trim().ToLowerInvariant());

        if (user == null || user.Password != request.Password || user.RevokedOn != null)
            return null;

        var token = GenerateToken(user.Login, user.Admin);
        var expiration = DateTime.Now.AddMinutes(_expiresInMinutes);

        return new LoginUserResponse
        {
            Login = user.Login,
            AccessToken = token,
            ExpiresAt = expiration
        };
    }

    public string GenerateToken(string login, bool isAdmin)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, login),
            new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "User")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_expiresInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}