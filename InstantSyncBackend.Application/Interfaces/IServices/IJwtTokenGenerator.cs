using InstantSyncBackend.Domain.Entities;

namespace InstantSyncBackend.Application.Interfaces.IServices;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user);
}
