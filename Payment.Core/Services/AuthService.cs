using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Payment.Core.DTOs;
using Payment.Core.Interfaces;
using Payment.Domain.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Payment.Core.Services.AuthService;

namespace Payment.Core.Services
{
    public class AuthService : IAuthService
    {

        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public AuthService(IConfiguration configuration, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ResponseDto<ApplicationUser>> Register(ApplicationUserDto request)
        {
            var checkIfEmailAlreadyExist = _unitOfWork.Users.GetUser(request.Email);
            if (checkIfEmailAlreadyExist is not null)
            {
                return ResponseDto<ApplicationUser>.Fail("Email already exist", (int)HttpStatusCode.BadRequest);
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
            };
            await _unitOfWork.Users.AddAsync(user);
            return ResponseDto<ApplicationUser>.Success("Registration is successful", user, (int)HttpStatusCode.OK);
        }

        public async Task<ResponseDto<string>> Login(ApplicationUserDto request)
        {
            var user = await _unitOfWork.Users.GetUser(request.Email);

            if (user is null)
            {
                return ResponseDto<string>.Fail("User not found", (int)HttpStatusCode.NotFound);
            }
            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return ResponseDto<string>.Fail("Wrong password", (int)HttpStatusCode.BadRequest);
            }

            string token = CreateToken(user);

            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(refreshToken);

            return ResponseDto<string>.Success("Login is successful", token, (int)HttpStatusCode.OK);
        }
        public async Task<ResponseDto<string>> RefreshToken()
        {
            var refreshToken = _httpContextAccessor?.HttpContext?.Request.Cookies["refreshToken"];
            var user = await _unitOfWork.Users.GetUser(u => u.RefreshToken.Equals(refreshToken));
            if (user is null)
            {
                return ResponseDto<string>.Fail("Invalid Refresh Token.", (int)HttpStatusCode.Unauthorized);
            }
            else if (user.TokenExpires < DateTime.Now)
            {
                return ResponseDto<string>.Fail("Token expired.", (int)HttpStatusCode.Unauthorized);
            }

            string token = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            SetRefreshToken(newRefreshToken);
            return ResponseDto<string>.Success("Token successfully refreshed.", token, (int)HttpStatusCode.OK);
        }

        private string CreateToken(ApplicationUser user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("Jwt:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            //var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return token.ToString();
        }
        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };

            return refreshToken;
        }

        private void SetRefreshToken(RefreshToken newRefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };
            _httpContextAccessor?.HttpContext?.Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);
            var users = _unitOfWork.Users.GetUser().ToList();
            users.ForEach(user =>
            {
                user.RefreshToken = newRefreshToken.Token;
                user.TokenCreated = newRefreshToken.Created;
                user.TokenExpires = newRefreshToken.Expires;
            });
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
