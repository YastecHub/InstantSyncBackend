using InstantSyncBackend.Application.Common;
using InstantSyncBackend.Application.Dtos;

namespace InstantSyncBackend.Application.Interfaces.IServices;

public interface IAuthService
{
    Task<BaseResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<BaseResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto);
    Task<BaseResponse<string>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<BaseResponse<string>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
}
