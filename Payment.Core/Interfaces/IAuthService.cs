using Payment.Core.DTOs;
using Payment.Domain.Models;

namespace Payment.Core.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseDto<string>> Login(ApplicationUserDto request);
        Task<ResponseDto<string>> RefreshToken();
        Task<ResponseDto<ApplicationUser>> Register(ApplicationUserDto request);
    }
}