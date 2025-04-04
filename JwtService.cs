using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Authorization
{
    public class JwtService
    {
        private readonly IConfiguration _config;
        private static Dictionary<string, string> _refreshTokens = new(); // Store in DB in production
        private readonly ILogger<JwtService> _logger;
        private readonly MyDbContext myDbContext;

        public JwtService(IConfiguration config, ILogger<JwtService> logger, MyDbContext myDbContext)
        {
            _config = config;
            _logger = logger;
            this.myDbContext = myDbContext;
        }

        public string GenerateToken(string username)
        {
            string response = "";

            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);            

            //var role = myDbContext.Users.Include(u => u.Roles).FirstOrDefault(u => u.Name == username)?.Roles.Name;

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            //new Claim(ClaimTypes.Role,role??"User")
        };

            var accessToken = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"])),
                signingCredentials: credentials
            );

           // var refreshToken = GenerateRefreshToken(); // Generate a new refresh token

            // Store the refresh token in DB or memory
            //SaveRefreshToken(username, refreshToken);

            var token = new TokenResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                //RefreshToken = refreshToken
            };

            response= token.AccessToken;

            return response;
        }

        // Generate a secure random refresh token
        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }


        //public void SaveRefreshToken(string username, string refreshToken)
        //{
        //    _userSecurity.SaveRefreshToken(username, refreshToken);
        //}

        //public bool ValidateRefreshToken(string username, string refreshToken)
        //{
        //    return _userSecurity.GetRefreshToken(username) == refreshToken;
        //}

        public void RevokeRefreshToken(string username)
        {
            _refreshTokens.Remove(username);
        }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
