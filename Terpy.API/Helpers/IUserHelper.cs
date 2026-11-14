using Microsoft.AspNetCore.Identity;
using Terpy.Shared.DTOs;
using Terpy.Shared.Entities;

namespace Terpy.API.Helpers;

public interface IUserHelper
{
    Task<User?> GetUserAsync(string email);
    Task<IdentityResult> AddUserAsync(User user, string password);
    Task CheckRoleAsync(string roleName);
    Task AddUserToRoleAsync(User user, string roleName);
    Task<bool> IsUserInRoleAsync(User user, string roleName);
    Task<SignInResult> LoginAsync(LoginDTO model);
    Task<IList<string>> GetRolesAsync(User user);
}
