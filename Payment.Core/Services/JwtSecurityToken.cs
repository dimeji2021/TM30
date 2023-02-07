using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Payment.Core.Services
{
    internal class JwtSecurityToken
    {
        private List<Claim> claims;
        private DateTime expires;
        private SigningCredentials signingCredentials;

        public JwtSecurityToken(List<Claim> claims, DateTime expires, SigningCredentials signingCredentials)
        {
            this.claims = claims;
            this.expires = expires;
            this.signingCredentials = signingCredentials;
        }
    }
}