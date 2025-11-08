using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyCOLL.Data;

namespace MyCOLL.Services;

public class UserAdminService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole> _roles;

    public UserAdminService(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles)
    {
        _users = users; _roles = roles;
    }

    public async Task<List<(ApplicationUser user, IList<string> roles)>> GetAllAsync()
    {
        var list = new List<(ApplicationUser, IList<string>)>();
        foreach (var u in await _users.Users.AsNoTracking().ToListAsync())
            list.Add((u, await _users.GetRolesAsync(u)));
        return list;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string email) =>
        await _users.FindByEmailAsync(email);

    public async Task<IdentityResult> CreateUserAsync(string email, string password)
    {
        var u = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        return await _users.CreateAsync(u, password);
    }

    public async Task EnsureRoleAsync(string role)
    {
        if (!await _roles.RoleExistsAsync(role))
            await _roles.CreateAsync(new IdentityRole(role));
    }

    public async Task<IdentityResult> AddToRoleAsync(string userId, string role)
    {
        var u = await _users.FindByIdAsync(userId) ?? throw new InvalidOperationException("User not found");
        await EnsureRoleAsync(role);
        return await _users.AddToRoleAsync(u, role);
    }

    public async Task<IdentityResult> RemoveFromRoleAsync(string userId, string role)
    {
        var u = await _users.FindByIdAsync(userId) ?? throw new InvalidOperationException("User not found");
        return await _users.RemoveFromRoleAsync(u, role);
    }

    public async Task<IdentityResult> DisableUserAsync(string userId)
    {
        var u = await _users.FindByIdAsync(userId) ?? throw new InvalidOperationException("User not found");
        u.LockoutEnabled = true;
        u.LockoutEnd = DateTimeOffset.MaxValue;
        return await _users.UpdateAsync(u);
    }
}
