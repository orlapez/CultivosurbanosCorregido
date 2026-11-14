using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Terpy.API.Data;
using Terpy.Shared.DTOs;
using Terpy.Shared.Entities;

namespace Terpy.API.Helpers;

public class UserHelper : IUserHelper
{
    private readonly DataContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<User> _signInManager;

    public UserHelper(DataContext context, UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
    }

    public async Task<User?> GetUserAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<IdentityResult> AddUserAsync(User user, string password)
    {
        user.UserName = user.Email;
        return await _userManager.CreateAsync(user, password);
    }

    public async Task CheckRoleAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var result = await _roleManager.CreateAsync(new IdentityRole { Name = roleName });
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("No se pudo crear el rol " + roleName + ".");
            }
        }
    }

    public async Task AddUserToRoleAsync(User user, string roleName)
    {
        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("No se pudo asignar el rol " + roleName + ".");
        }
    }

    public async Task<bool> IsUserInRoleAsync(User user, string roleName)
    {
        return await _userManager.IsInRoleAsync(user, roleName);
    }

    public async Task<SignInResult> LoginAsync(LoginDTO model)
    {
        var user = await GetUserAsync(model.Email.Trim());
        if (user == null)
        {
            return SignInResult.Failed;
        }

        return await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
    }

    public async Task<IList<string>> GetRolesAsync(User user)
    {
        return await _userManager.GetRolesAsync(user);
    }
}
