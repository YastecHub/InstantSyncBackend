using Microsoft.AspNetCore.Identity;

namespace InstantSyncBackend.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
}
